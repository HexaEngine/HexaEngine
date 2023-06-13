namespace HexaEngine.Core.Lights
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Lights.Probes;
    using HexaEngine.Core.Lights.Structs;
    using HexaEngine.Core.Lights.Types;
    using HexaEngine.Core.Renderers;
    using HexaEngine.Core.Rendering;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using HexaEngine.ImGuiNET;
    using HexaEngine.Mathematics;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public partial class LightManager : ISystem
    {
        private readonly List<ILightProbeComponent> probes = new();
        private readonly List<Light> lights = new();
        private readonly List<Light> activeLights = new();
        private IGraphicsDevice device;
        private readonly ConcurrentQueue<ILightProbeComponent> probeUpdateQueue = new();
        private readonly ConcurrentQueue<Light> lightUpdateQueue = new();
        public readonly ConcurrentQueue<IRendererComponent> RendererUpdateQueue = new();

        private StructuredUavBuffer<GlobalProbeData> GlobalProbes;

        private StructuredUavBuffer<DirectionalLightData> DirectionalLights;
        private StructuredUavBuffer<PointLightData> PointLights;
        private StructuredUavBuffer<SpotlightData> Spotlights;

        public StructuredUavBuffer<ShadowDirectionalLightData> ShadowDirectionalLights;
        public StructuredUavBuffer<ShadowPointLightData> ShadowPointLights;
        public StructuredUavBuffer<ShadowSpotlightData> ShadowSpotlights;

        private ConstantBuffer<ProbeBufferParams> probeParamsBuffer;
        private ConstantBuffer<LightBufferParams> lightParamsBuffer;
        private ConstantBuffer<ForwardLightBufferParams> forwardLightParamsBuffer;

        private Quad quad;
        private ISamplerState linearClampSampler;
        private ISamplerState linearWrapSampler;
        private ISamplerState pointClampSampler;
        private ISamplerState shadowSampler;
        private unsafe void** cbs;
        private const uint nConstantBuffers = 2;
        private unsafe void** smps;
        private const uint nSamplers = 4;

        private unsafe void** forwardRtvs;
        private const uint nForwardRtvs = 3;
        private unsafe void** forwardSrvs;
        private const uint nForwardSrvs = 8 + 2 + 1 + 3 + 3 + MaxGlobalLightProbes * 2 + MaxDirectionalLightSDs + MaxPointLightSDs + MaxSpotlightSDs;
        private const uint nForwardIndirectSrvsBase = 8 + 2 + 1 + 3 + 3;
        private const uint nForwardShadowSrvsBase = 8 + 2 + 1 + 3 + 3 + MaxGlobalLightProbes * 2;

        private IGraphicsPipeline deferredDirect;
        private unsafe void** directSrvs;
        private const uint nDirectSrvs = 8 + 4;

        private IGraphicsPipeline deferredIndirect;
        private unsafe void** indirectSrvs;
        private const uint nIndirectSrvsBase = 8 + 3;
        private const uint nIndirectSrvs = 8 + 3 + MaxGlobalLightProbes * 2;

        private IGraphicsPipeline deferredShadow;
        private unsafe void** shadowSrvs;
        private const uint nShadowSrvs = 8 + 4 + MaxDirectionalLightSDs + MaxPointLightSDs + MaxSpotlightSDs;

        private IComputePipeline computeFrustums;
        private IComputePipeline lightCulling;
        private DispatchPass lightCullingDispatchPass;
        private ushort lightCullingBlockSize = 16;

        private ConstantBuffer<CBDispatchParams> frustumParams;
        private ConstantBuffer<CBDispatchParams> lightCullParams;
        private ConstantBuffer<UPoint4> lightCountParams;
        private StructuredUavBuffer<Frustum> frustumBuffer;
        private StructuredBuffer<LightData> lightsBuffer;
        private StructuredUavBuffer<uint> lightListIndexCounterOpaque;
        private StructuredUavBuffer<uint> lightListIndexCounterTransparent;
        private StructuredUavBuffer<uint> lightIndexListOpaque;
        private StructuredUavBuffer<uint> lightIndexListTransparent;
        private TextureUav2D lightGridOpaque;
        private TextureUav2D lightGridTransparent;
        private TextureUav2D lightCullingDebugTexture;
        private Texture2D lightCullingHeatMap;

        public const int MaxGlobalLightProbes = 4;
        public const int MaxDirectionalLightSDs = 1;
        public const int MaxPointLightSDs = 32;
        public const int MaxSpotlightSDs = 32;

        public IRenderTargetView Output;
        public ResourceRef<IDepthStencilView> DSV;
        public ResourceRef<IShaderResourceView> DepthSRV;

        public ResourceRef<GBuffer> GBuffers;

        public ResourceRef<Texture> LUT;
        public ResourceRef<Texture> SSAO;

        public IShaderResourceView[] CSMs;
        public IShaderResourceView[] OSMs;
        public IShaderResourceView[] PSMs;
        public ResourceRef<IBuffer> Camera;

        public LightManager()
        {
        }

        public static LightManager? Current => SceneManager.Current?.LightManager;

        public IReadOnlyList<ILightProbeComponent> Probes => probes;

        public IReadOnlyList<Light> Lights => lights;

        public IReadOnlyList<Light> Active => activeLights;

        public int Count => lights.Count;

        public int ActiveCount => activeLights.Count;

        public string Name => "Lights";

        public SystemFlags Flags { get; } = SystemFlags.None;

        public async Task Initialize(IGraphicsDevice device)
        {
            this.device = device;
            probeParamsBuffer = new(device, CpuAccessFlags.Write);
            lightParamsBuffer = new(device, CpuAccessFlags.Write);
            forwardLightParamsBuffer = new(device, CpuAccessFlags.Write);

            Camera = ResourceManager2.Shared.GetBuffer("CBCamera");
            GBuffers = ResourceManager2.Shared.GetGBuffer("GBuffer");
            LUT = ResourceManager2.Shared.GetTexture("BRDFLUT");
            SSAO = ResourceManager2.Shared.GetTexture("SSAOBuffer");

            GlobalProbes = new(device, true, false);

            DirectionalLights = new(device, true, false);
            PointLights = new(device, true, false);
            Spotlights = new(device, true, false);

            ShadowDirectionalLights = new(device, true, false);
            ShadowPointLights = new(device, true, false);
            ShadowSpotlights = new(device, true, false);

            quad = new(device);
            deferredDirect = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "deferred/brdf/vs.hlsl",
                PixelShader = "deferred/brdf/direct.hlsl",
            },
            new GraphicsPipelineState()
            {
                Blend = BlendDescription.Additive,
                BlendFactor = Vector4.One,
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.TriangleList
            });

            linearClampSampler = ResourceManager2.Shared.GetOrAddSamplerState("PointClamp", SamplerDescription.LinearClamp).Value;
            linearWrapSampler = ResourceManager2.Shared.GetOrAddSamplerState("LinearWrap", SamplerDescription.LinearWrap).Value;
            pointClampSampler = ResourceManager2.Shared.GetOrAddSamplerState("PointClamp", SamplerDescription.PointClamp).Value;
            shadowSampler = ResourceManager2.Shared.GetOrAddSamplerState("LinearComparisonBorder", SamplerDescription.ComparisonLinearBorder).Value;

            unsafe
            {
                smps = AllocArray(nSamplers);
                smps[0] = (void*)linearClampSampler.NativePointer;
                smps[1] = (void*)linearWrapSampler.NativePointer;
                smps[2] = (void*)pointClampSampler.NativePointer;
                smps[3] = (void*)shadowSampler.NativePointer;

                cbs = AllocArray(nConstantBuffers);

                forwardSrvs = AllocArray(nForwardSrvs);
                directSrvs = AllocArray(nDirectSrvs);
                indirectSrvs = AllocArray(nIndirectSrvs);
                shadowSrvs = AllocArray(nShadowSrvs);
                Zero(forwardSrvs, (uint)(nForwardSrvs * sizeof(nint)));
                Zero(directSrvs, (uint)(nDirectSrvs * sizeof(nint)));
                Zero(shadowSrvs, (uint)(nShadowSrvs * sizeof(nint)));
                Zero(indirectSrvs, (uint)(nIndirectSrvs * sizeof(nint)));

                forwardRtvs = AllocArray(nForwardRtvs);
                Zero(forwardRtvs, (uint)(nForwardRtvs * sizeof(nint)));
            }

            deferredIndirect = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "deferred/brdf/vs.hlsl",
                PixelShader = "deferred/brdf/indirect.hlsl",
            },
            new GraphicsPipelineState()
            {
                Blend = BlendDescription.Additive,
                BlendFactor = Vector4.One,
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.TriangleList
            });

            deferredShadow = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "deferred/brdf/vs.hlsl",
                PixelShader = "deferred/brdf/shadow.hlsl",
            },
            new GraphicsPipelineState()
            {
                Blend = BlendDescription.Additive,
                BlendFactor = Vector4.One,
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.TriangleList
            });

            var window = Application.MainWindow;
            float screenWidth = window.RenderViewport.Width;
            float screenHeight = window.RenderViewport.Height;
            var macros = new ShaderMacro[] { new ShaderMacro("BLOCK_SIZE", lightCullingBlockSize.ToString()) };
            {
                computeFrustums = await device.CreateComputePipelineAsync(new()
                {
                    Path = "compute/lights/frustum.hlsl"
                }, macros);

                // To compute the frustums for the grid tiles, each thread will compute a single
                // frustum for the tile.
                UPoint3 numThreads = new Vector3(screenWidth / lightCullingBlockSize, screenHeight / lightCullingBlockSize, 1).Ceiling();
                UPoint3 numThreadGroups = new Vector3(numThreads.X / (float)lightCullingBlockSize, numThreads.Y / (float)lightCullingBlockSize, 1).Ceiling();

                // Update the number of thread groups for the compute frustums compute shader.
                CBDispatchParams dispatchParams = default;
                dispatchParams.NumThreadGroups = numThreadGroups;
                dispatchParams.NumThreads = numThreads;

                frustumParams = new(device, dispatchParams, CpuAccessFlags.Write);
                frustumBuffer = new(device, numThreads.X * numThreads.Y * numThreads.Z, false, false);
            }

            {
                lightCulling = await device.CreateComputePipelineAsync(new()
                {
                    Path = "compute/lights/culling.hlsl"
                }, macros);

                UPoint3 numThreadGroups = new Vector3(screenWidth / lightCullingBlockSize, screenHeight / lightCullingBlockSize, 1).Ceiling();

                lightCullingDispatchPass = new(lightCulling, numThreadGroups);

                CBDispatchParams dispatchParams = default;
                dispatchParams.NumThreadGroups = numThreadGroups;
                dispatchParams.NumThreads = numThreadGroups * new UPoint3(lightCullingBlockSize, lightCullingBlockSize, 1);

                lightCullParams = new(device, dispatchParams, CpuAccessFlags.Write);
                lightCountParams = new(device, CpuAccessFlags.Write);

                lightsBuffer = new(device, CpuAccessFlags.Write);
                lightListIndexCounterOpaque = new(device, 1, false, false);
                lightListIndexCounterTransparent = new(device, 1, false, false);
                lightIndexListOpaque = new(device, false, false);
                lightIndexListTransparent = new(device, false, false);
                lightGridOpaque = new(device, Format.R32G32UInt, (int)numThreadGroups.X, (int)numThreadGroups.Y, (int)numThreadGroups.Z, 1, true, false);
                lightGridTransparent = new(device, Format.R32G32UInt, (int)numThreadGroups.X, (int)numThreadGroups.Y, (int)numThreadGroups.Z, 1, true, false);
                lightCullingDebugTexture = new(device, Format.R8G8B8A8UNorm, (int)screenWidth, (int)screenHeight, 1, 1, true, false);
                lightCullingHeatMap = new(device, new TextureFileDescription(Paths.CurrentAssetsPath + "textures/debugging/heatmap.png"));
            }
        }

        private void LightTransformed(GameObject gameObject)
        {
            if (gameObject is Light light)
            {
                lightUpdateQueue.Enqueue(light);
            }
        }

        private void LightPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is Light light)
            {
                lightUpdateQueue.Enqueue(light);
            }
        }

        public void Clear()
        {
            lock (lights)
            {
                lights.Clear();
            }
        }

        public unsafe void Register(GameObject gameObject)
        {
            if (gameObject is Light light)
            {
                AddLight(light);
            }
            if (gameObject.TryGetComponent<ILightProbeComponent>(out var component))
            {
                AddProbe(component);
            }
        }

        public unsafe void Unregister(GameObject gameObject)
        {
            if (gameObject is Light light)
            {
                light.DestroyShadowMap();
                RemoveLight(light);
            }
            if (gameObject.TryGetComponent<ILightProbeComponent>(out var component))
            {
                RemoveProbe(component);
            }
        }

        public unsafe void AddLight(Light light)
        {
            lock (lights)
            {
                lights.Add(light);
                lightUpdateQueue.Enqueue(light);
                light.Transformed += LightTransformed;
                light.PropertyChanged += LightPropertyChanged;
            }
        }

        public unsafe void AddProbe(ILightProbeComponent probe)
        {
            lock (probes)
            {
                probes.Add(probe);
                probeUpdateQueue.Enqueue(probe);
            }
        }

        public unsafe void RemoveLight(Light light)
        {
            lock (lights)
            {
                light.PropertyChanged -= LightPropertyChanged;
                light.Transformed -= LightTransformed;
                lights.Remove(light);
                activeLights.Remove(light);
            }
        }

        public unsafe void RemoveProbe(ILightProbeComponent probe)
        {
            lock (probes)
            {
                probes.Remove(probe);
            }
        }

        public readonly Queue<Light> UpdateShadowLightQueue = new();

        public unsafe void Update(IGraphicsContext context, Camera camera)
        {
            while (lightUpdateQueue.TryDequeue(out var light))
            {
                if (light.IsEnabled)
                {
                    if (!activeLights.Contains(light))
                    {
                        activeLights.Add(light);
                    }

                    if (light.CastShadows)
                    {
                        light.CreateShadowMap(context.Device);
                    }
                    else
                    {
                        light.DestroyShadowMap();
                    }

                    if (!light.InUpdateQueue)
                    {
                        light.InUpdateQueue = true;
                        UpdateShadowLightQueue.Enqueue(light);
                    }
                }
                else
                {
                    activeLights.Remove(light);
                }
            }

            UpdateLights(context, camera);

            while (RendererUpdateQueue.TryDequeue(out var renderer))
            {
                for (int i = 0; i < activeLights.Count; i++)
                {
                    var light = activeLights[i];
                    if (!light.CastShadows)
                    {
                        continue;
                    }

                    if (light.IntersectFrustum(renderer.BoundingBox) && !light.InUpdateQueue)
                    {
                        light.InUpdateQueue = true;
                        UpdateShadowLightQueue.Enqueue(light);
                    }
                }
            }

            for (int i = 0; i < activeLights.Count; i++)
            {
                var light = activeLights[i];
                if (light.CastShadows && light is DirectionalLight && !light.InUpdateQueue)
                {
                    light.InUpdateQueue = true;
                    UpdateShadowLightQueue.Enqueue(light);
                }
            }

            *lightCountParams.Local = new(lightsBuffer.Count, 0, 0, 0);
            lightCountParams.Update(context);
            lightsBuffer.Update(context);
        }

        public void BeginResize()
        {
        }

        public Task EndResize(int width, int height)
        {
#nullable disable
            Output = ResourceManager2.Shared.UpdateTexture("LightBuffer", TextureDescription.CreateTexture2DWithRTV(width, height, 1, Format.R16G16B16A16Float)).Value.RenderTargetView;
            DSV = ResourceManager2.Shared.GetDepthStencilView("SwapChain.DSV");
            DepthSRV = ResourceManager2.Shared.GetShaderResourceView("PrePass.SRV");

#nullable enable
            UpdateResources();

            SetThreadGroupBlockSize(lightCullingBlockSize, width, height);

            return Task.CompletedTask;
        }

        private unsafe void UpdateResources()
        {
            cbs[1] = (void*)Camera.Value?.NativePointer;
#nullable disable

            if (GBuffers != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (i < GBuffers.Value.Count)
                    {
                        shadowSrvs[i] = indirectSrvs[i] = directSrvs[i] = (void*)GBuffers.Value.SRVs[i]?.NativePointer;
                    }
                }
                shadowSrvs[4] = indirectSrvs[4] = directSrvs[4] = (void*)DepthSRV.Value.NativePointer;
            }

            forwardSrvs[8] = indirectSrvs[8] = (void*)SSAO.Value?.ShaderResourceView.NativePointer;
            forwardSrvs[9] = indirectSrvs[9] = (void*)LUT.Value?.ShaderResourceView.NativePointer;
            forwardSrvs[10] = indirectSrvs[10] = (void*)GlobalProbes.SRV.NativePointer;

            directSrvs[8] = (void*)SSAO.Value?.ShaderResourceView.NativePointer;
            forwardSrvs[11] = directSrvs[9] = (void*)DirectionalLights.SRV.NativePointer;
            forwardSrvs[12] = directSrvs[10] = (void*)PointLights.SRV.NativePointer;
            forwardSrvs[13] = directSrvs[11] = (void*)Spotlights.SRV.NativePointer;

            shadowSrvs[8] = (void*)SSAO.Value?.ShaderResourceView.NativePointer;
            forwardSrvs[14] = shadowSrvs[9] = (void*)ShadowDirectionalLights.SRV.NativePointer;
            forwardSrvs[15] = shadowSrvs[10] = (void*)ShadowPointLights.SRV.NativePointer;
            forwardSrvs[16] = shadowSrvs[11] = (void*)ShadowSpotlights.SRV.NativePointer;

            forwardRtvs[0] = (void*)Output.NativePointer;
            forwardRtvs[1] = GBuffers.Value.PRTVs[1];
            forwardRtvs[2] = GBuffers.Value.PRTVs[2];

#nullable enable
        }

        private unsafe void UpdateLights(IGraphicsContext context, Camera camera)
        {
            GlobalProbes.ResetCounter();
            DirectionalLights.ResetCounter();
            PointLights.ResetCounter();
            Spotlights.ResetCounter();
            ShadowDirectionalLights.ResetCounter();
            ShadowPointLights.ResetCounter();
            ShadowSpotlights.ResetCounter();
            uint globalProbesCount = 0;
            uint csmCount = 0;
            uint osmCount = 0;
            uint psmCount = 0;

            for (int i = 0; i < probes.Count; i++)
            {
                var probe = probes[i];
                if (!(probe.IsEnabled && probe.IsVaild))
                {
                    continue;
                }

                switch (probe.Type)
                {
                    case ProbeType.Global:
                        GlobalProbes.Add((GlobalLightProbeComponent)probe);
                        forwardSrvs[nForwardIndirectSrvsBase + globalProbesCount] = indirectSrvs[nIndirectSrvsBase + globalProbesCount] = (void*)(probe.DiffuseTex?.ShaderResourceView?.NativePointer ?? 0);
                        forwardSrvs[nForwardIndirectSrvsBase + MaxGlobalLightProbes + globalProbesCount] = indirectSrvs[nIndirectSrvsBase + MaxGlobalLightProbes + globalProbesCount] = (void*)(probe.SpecularTex?.ShaderResourceView?.NativePointer ?? 0);
                        globalProbesCount++;
                        break;

                    case ProbeType.Local:
                        break;
                }
            }

            for (uint i = globalProbesCount; i < MaxGlobalLightProbes; i++)
            {
                forwardSrvs[nForwardIndirectSrvsBase + i] = indirectSrvs[nIndirectSrvsBase + i] = null;
                forwardSrvs[nForwardIndirectSrvsBase + MaxGlobalLightProbes + i] = indirectSrvs[nIndirectSrvsBase + MaxGlobalLightProbes + i] = null;
            }
            lightsBuffer.ResetCounter();
            for (int i = 0; i < activeLights.Count; i++)
            {
                var light = activeLights[i];
                lightsBuffer.Add(new(light, camera));
                if (light.CastShadows)
                {
                    switch (light.LightType)
                    {
                        case LightType.Directional:
                            if (csmCount == MaxDirectionalLightSDs)
                            {
                                continue;
                            }

                            light.QueueIndex = csmCount;
                            ShadowDirectionalLights.Add(new((DirectionalLight)light));
                            forwardSrvs[nForwardShadowSrvsBase + csmCount] = shadowSrvs[nDirectSrvs + csmCount] = (void*)light.GetShadowMap()?.NativePointer;
                            csmCount++;
                            break;

                        case LightType.Point:
                            if (osmCount == MaxPointLightSDs)
                            {
                                continue;
                            }

                            light.QueueIndex = osmCount;
                            ShadowPointLights.Add(new((PointLight)light));
                            forwardSrvs[nForwardShadowSrvsBase + MaxDirectionalLightSDs + osmCount] = shadowSrvs[nDirectSrvs + MaxDirectionalLightSDs + osmCount] = (void*)light.GetShadowMap()?.NativePointer;
                            osmCount++;
                            break;

                        case LightType.Spot:
                            if (psmCount == MaxSpotlightSDs)
                            {
                                continue;
                            }

                            light.QueueIndex = psmCount;
                            ShadowSpotlights.Add(new((Spotlight)light));
                            forwardSrvs[nForwardShadowSrvsBase + MaxDirectionalLightSDs + MaxPointLightSDs + psmCount] = shadowSrvs[nDirectSrvs + MaxDirectionalLightSDs + MaxPointLightSDs + psmCount] = (void*)light.GetShadowMap()?.NativePointer;
                            psmCount++;
                            break;
                    }
                }
                else
                {
                    switch (light.LightType)
                    {
                        case LightType.Directional:
                            light.QueueIndex = DirectionalLights.Count;
                            DirectionalLights.Add(new((DirectionalLight)light));
                            break;

                        case LightType.Point:
                            light.QueueIndex = PointLights.Count;
                            PointLights.Add(new((PointLight)light));
                            break;

                        case LightType.Spot:
                            light.QueueIndex = Spotlights.Count;
                            Spotlights.Add(new((Spotlight)light));
                            break;
                    }
                }
            }

            for (uint i = csmCount; i < MaxDirectionalLightSDs; i++)
            {
                forwardSrvs[nForwardShadowSrvsBase + i] = shadowSrvs[nDirectSrvs + i] = null;
            }
            for (uint i = osmCount; i < MaxPointLightSDs; i++)
            {
                forwardSrvs[nForwardShadowSrvsBase + MaxDirectionalLightSDs + i] = shadowSrvs[nDirectSrvs + MaxDirectionalLightSDs + i] = null;
            }
            for (uint i = psmCount; i < MaxSpotlightSDs; i++)
            {
                forwardSrvs[nForwardShadowSrvsBase + MaxDirectionalLightSDs + MaxPointLightSDs + i] = shadowSrvs[nDirectSrvs + MaxDirectionalLightSDs + MaxPointLightSDs + i] = null;
            }

            forwardSrvs[10] = indirectSrvs[10] = (void*)GlobalProbes.SRV.NativePointer;

            forwardSrvs[11] = directSrvs[9] = (void*)DirectionalLights.SRV.NativePointer;
            forwardSrvs[12] = directSrvs[10] = (void*)PointLights.SRV.NativePointer;
            forwardSrvs[13] = directSrvs[11] = (void*)Spotlights.SRV.NativePointer;

            forwardSrvs[14] = shadowSrvs[9] = (void*)ShadowDirectionalLights.SRV.NativePointer;
            forwardSrvs[15] = shadowSrvs[10] = (void*)ShadowPointLights.SRV.NativePointer;
            forwardSrvs[16] = shadowSrvs[11] = (void*)ShadowSpotlights.SRV.NativePointer;
        }

        private void SetThreadGroupBlockSize(ushort blockSize, int width, int height)
        {
            bool hasChanged = blockSize != lightCullingBlockSize;
            lightCullingBlockSize = blockSize;

            if (hasChanged)
            {
                // Recompile the compute shader with the updated macros.
                var macros = new ShaderMacro[] { new ShaderMacro("BLOCK_SIZE", blockSize.ToString()) };
                computeFrustums = device.CreateComputePipeline(new()
                {
                    Path = "compute/lights/frustum.hlsl"
                }, macros);

                lightCulling = device.CreateComputePipeline(new()
                {
                    Path = "compute/lights/culling.hlsl"
                }, macros);
            }

            // Recompute the frustums for the grid.
            UpdateGridFrustums(device.Context, width, height);

            uint screenWidth = (uint)width;
            uint screenHeight = (uint)height;

            UPoint3 numThreadGroups = new Vector3(screenWidth / (float)lightCullingBlockSize, screenHeight / (float)lightCullingBlockSize, 1).Ceiling();

            lightCullingDispatchPass.NumGroups = numThreadGroups;

            CBDispatchParams dispatchParams = default;
            dispatchParams.NumThreadGroups = numThreadGroups;
            dispatchParams.NumThreads = numThreadGroups * new UPoint3(lightCullingBlockSize, lightCullingBlockSize, 1);
            lightCullParams.Set(device.Context, dispatchParams);

            lightIndexListOpaque.Capacity = numThreadGroups.X * numThreadGroups.Y * numThreadGroups.Z * AVERAGE_OVERLAPPING_LIGHTS_PER_TILE;
            lightIndexListTransparent.Capacity = numThreadGroups.X * numThreadGroups.Y * numThreadGroups.Z * AVERAGE_OVERLAPPING_LIGHTS_PER_TILE;

            lightGridOpaque.Resize(device, Format.R32G32UInt, (int)numThreadGroups.X, (int)numThreadGroups.Y, (int)numThreadGroups.Z, 1, true, false);
            lightGridTransparent.Resize(device, Format.R32G32UInt, (int)numThreadGroups.X, (int)numThreadGroups.Y, (int)numThreadGroups.Z, 1, true, false);
        }

        private unsafe void UpdateGridFrustums(IGraphicsContext context, int width, int height)
        {
            // Make sure we can create at least 1 thread (even if the window is minimized)
            uint screenWidth = (uint)width;
            uint screenHeight = (uint)height;

            // To compute the frustums for the grid tiles, each thread will compute a single
            // frustum for the tile.
            UPoint3 numThreads = new Vector3(screenWidth / (float)lightCullingBlockSize, screenHeight / (float)lightCullingBlockSize, 1).Ceiling();
            UPoint3 numThreadGroups = new Vector3(numThreads.X / (float)lightCullingBlockSize, numThreads.Y / (float)lightCullingBlockSize, 1).Ceiling();

            // Update the number of thread groups for the compute frustums compute shader.
            CBDispatchParams dispatchParams = default;
            dispatchParams.NumThreadGroups = numThreadGroups;
            dispatchParams.NumThreads = numThreads;
            frustumParams.Set(context, dispatchParams);

            // Destroy the previous structured buffer for storing gird frustums.
            // Create a new RWStructuredBuffer for storing the grid frustums.
            // We need 1 frustum for each grid cell.
            // For 1280x720 screen resolution and 16x16 tile size, results in 80x45 grid
            // for a total of 3,600 frustums.
            frustumBuffer.Capacity = numThreads.X * numThreads.Y * numThreads.Z;

            // Dispatch the compute shader to recompute the grid frustums.
            context.CSSetConstantBuffer(frustumParams, 0);
            context.CSSetConstantBuffer(Camera.Value, 1);
            context.CSSetUnorderedAccessView((void*)frustumBuffer.UAV.NativePointer, 0);

            computeFrustums.Dispatch(context, numThreadGroups.X, numThreadGroups.Y, numThreadGroups.Z);
        }

        private const uint AVERAGE_OVERLAPPING_LIGHTS_PER_TILE = 200;

        public unsafe void CullLights(IGraphicsContext context)
        {
            context.CSSetConstantBuffer(lightCullParams, 0);
            context.CSSetConstantBuffer(Camera.Value, 1);
            context.CSSetConstantBuffer(lightCountParams, 2);
            context.CSSetSampler(linearWrapSampler, 0);
            context.CSSetShaderResource(lightsBuffer.SRV, 0);
            context.CSSetShaderResource(DepthSRV.Value, 1);
            context.CSSetShaderResource(frustumBuffer.SRV, 2);
            context.CSSetShaderResource(lightCullingHeatMap.SRV, 3);
            nint* array = stackalloc nint[7];
            array[0] = lightCullingDebugTexture.UAV.NativePointer;
            array[1] = lightListIndexCounterOpaque.UAV.NativePointer;
            array[2] = lightListIndexCounterTransparent.UAV.NativePointer;
            array[3] = lightIndexListOpaque.UAV.NativePointer;
            array[4] = lightIndexListTransparent.UAV.NativePointer;
            array[5] = lightGridOpaque.UAV.NativePointer;
            array[6] = lightGridTransparent.UAV.NativePointer;
            context.CSSetUnorderedAccessViews((void**)array, 7);
            context.ClearUnorderedAccessViewUint(lightListIndexCounterOpaque.UAV, 0, 0, 0, 0);
            context.ClearUnorderedAccessViewUint(lightListIndexCounterTransparent.UAV, 0, 0, 0, 0);
            lightCullingDispatchPass.BeginDraw(context);
            lightCullingDispatchPass.Draw(context);
            lightCullingDispatchPass.EndDraw(context);
            context.ClearState();
        }

        public void UpdateBuffers(IGraphicsContext context)
        {
            GlobalProbes.Update(context);

            DirectionalLights.Update(context);
            PointLights.Update(context);
            Spotlights.Update(context);

            ShadowDirectionalLights.Update(context);
            ShadowPointLights.Update(context);
            ShadowSpotlights.Update(context);
        }

        public unsafe void DeferredPassIndirect(IGraphicsContext context)
        {
            if (GlobalProbes.Count > 0)
            {
                // Indirect light pass
                var probeParams = probeParamsBuffer.Local;
                probeParams->GlobalProbes = GlobalProbes.Count;
                probeParamsBuffer.Update(context);
                cbs[0] = (void*)probeParamsBuffer.Buffer?.NativePointer;

                context.PSSetSamplers(smps, nSamplers, 0);
                context.PSSetConstantBuffers(cbs, nConstantBuffers, 0);
                context.PSSetShaderResources(indirectSrvs, nIndirectSrvs, 0);

                quad.DrawAuto(context, deferredIndirect);
                deferredIndirect.EndDraw(context);

                nint* null_samplers = stackalloc nint[(int)nSamplers];
                context.PSSetSamplers((void**)null_samplers, nSamplers, 0);

                nint* null_cbs = stackalloc nint[(int)nConstantBuffers];
                context.PSSetConstantBuffers((void**)null_cbs, nConstantBuffers, 0);

                nint* null_srvs = stackalloc nint[(int)nIndirectSrvs];
                context.PSSetShaderResources((void**)null_srvs, nIndirectSrvs, 0);
            }
        }

        public unsafe void DeferredPassDirect(IGraphicsContext context)
        {
            if (DirectionalLights.Count > 0 || PointLights.Count > 0 || Spotlights.Count > 0)
            {
                // Direct light pass
                var lightParams = lightParamsBuffer.Local;
                lightParams->DirectionalLights = DirectionalLights.Count;
                lightParams->PointLights = PointLights.Count;
                lightParams->Spotlights = Spotlights.Count;
                lightParamsBuffer.Update(context);
                cbs[0] = (void*)lightParamsBuffer.Buffer?.NativePointer;

                context.PSSetSamplers(smps, nSamplers, 0);
                context.PSSetConstantBuffers(cbs, nConstantBuffers, 0);
                context.PSSetShaderResources(directSrvs, nDirectSrvs, 0);

                quad.DrawAuto(context, deferredDirect);

                nint* null_samplers = stackalloc nint[(int)nSamplers];
                context.PSSetSamplers((void**)null_samplers, nSamplers, 0);

                nint* null_cbs = stackalloc nint[(int)nConstantBuffers];
                context.PSSetConstantBuffers((void**)null_cbs, nConstantBuffers, 0);

                nint* null_srvs = stackalloc nint[(int)nIndirectSrvs];
                context.PSSetShaderResources((void**)null_srvs, nIndirectSrvs, 0);
            }
        }

        public unsafe void DeferredPassDirectShadows(IGraphicsContext context)
        {
            if (ShadowDirectionalLights.Count > 0 || ShadowPointLights.Count > 0 || ShadowSpotlights.Count > 0)
            {
                // Shadow light pass
                var lightParams = lightParamsBuffer.Local;
                lightParams->DirectionalLights = ShadowDirectionalLights.Count;
                lightParams->PointLights = ShadowPointLights.Count;
                lightParams->Spotlights = ShadowSpotlights.Count;
                lightParamsBuffer.Update(context);
                cbs[0] = (void*)lightParamsBuffer.Buffer?.NativePointer;

                context.PSSetSamplers(smps, nSamplers, 0);
                context.PSSetConstantBuffers(cbs, nConstantBuffers, 0);
                context.PSSetShaderResources(shadowSrvs, nShadowSrvs, 0);

                quad.DrawAuto(context, deferredShadow);

                nint* null_samplers = stackalloc nint[(int)nSamplers];
                context.PSSetSamplers((void**)null_samplers, nSamplers, 0);

                nint* null_cbs = stackalloc nint[(int)nConstantBuffers];
                context.PSSetConstantBuffers((void**)null_cbs, nConstantBuffers, 0);

                nint* null_srvs = stackalloc nint[(int)nIndirectSrvs];
                context.PSSetShaderResources((void**)null_srvs, nIndirectSrvs, 0);
            }
        }

        public unsafe void ForwardPass(IGraphicsContext context, IRendererComponent renderer, Camera camera)
        {
            var lightParams = forwardLightParamsBuffer.Local;
            lightParams->DirectionalLights = DirectionalLights.Count;
            lightParams->PointLights = PointLights.Count;
            lightParams->Spotlights = Spotlights.Count;
            lightParams->DirectionalLightSDs = ShadowDirectionalLights.Count;
            lightParams->PointLightSDs = ShadowPointLights.Count;
            lightParams->SpotlightSDs = ShadowSpotlights.Count;
            lightParams->GlobalProbes = GlobalProbes.Count;
            forwardLightParamsBuffer.Update(context);
            cbs[0] = (void*)forwardLightParamsBuffer.Buffer?.NativePointer;

            context.SetRenderTargets(forwardRtvs, nForwardRtvs, DSV.Value);
            context.SetViewport(Output.Viewport);
            context.VSSetConstantBuffers(&cbs[1], 1, 1);
            context.DSSetConstantBuffers(&cbs[1], 1, 1);
            context.PSSetConstantBuffers(cbs, nConstantBuffers, 0);
            context.PSSetShaderResources(forwardSrvs, nForwardSrvs, 0);
            context.PSSetSamplers(smps, nSamplers, 8);

            renderer.Draw(context);

            context.ClearState();
        }

        public unsafe void Dispose()
        {
            GlobalProbes.Dispose();

            DirectionalLights.Dispose();
            PointLights.Dispose();
            Spotlights.Dispose();

            ShadowDirectionalLights.Dispose();
            ShadowPointLights.Dispose();
            ShadowSpotlights.Dispose();

            forwardLightParamsBuffer.Dispose();
            probeParamsBuffer.Dispose();
            lightParamsBuffer.Dispose();
            quad.Dispose();
            deferredDirect.Dispose();
            deferredIndirect.Dispose();
            deferredShadow.Dispose();
            Free(directSrvs);
            Free(indirectSrvs);
            Free(shadowSrvs);
            Free(forwardSrvs);
            Free(smps);
            Free(cbs);
            Free(forwardRtvs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Awake()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Update(float dt)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void FixedUpdate()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Destroy()
        {
        }

        public void ShowHeatmap()
        {
            if (ImGui.Begin("Heatmap", ImGuiWindowFlags.NoDocking))
            {
                var size = ImGui.GetContentRegionAvail();
                ImGui.Image(lightCullingDebugTexture.SRV.NativePointer, size);
            }
            ImGui.End();
        }
    }
}