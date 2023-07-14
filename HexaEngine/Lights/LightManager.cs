namespace HexaEngine.Lights
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Lights.Probes;
    using HexaEngine.Lights.Structs;
    using HexaEngine.Lights.Types;
    using HexaEngine.Rendering;
    using HexaEngine.Scenes;
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

        public StructuredUavBuffer<LightData> LightBuffer;
        public StructuredUavBuffer<ShadowData> ShadowDataBuffer;

        public StructuredUavBuffer<Cluster> ClusterBuffer;
        public StructuredUavBuffer<uint> LightIndexCounter;
        public StructuredUavBuffer<uint> LightIndexList;
        public StructuredUavBuffer<LightGrid> LightGridBuffer;

        private IComputePipeline clusterBuilding;
        private IComputePipeline clusterCulling;

        private ConstantBuffer<ProbeBufferParams> probeParamsBuffer;
        private ConstantBuffer<LightParams> lightParamsBuffer;
        private ConstantBuffer<ForwardLightBufferParams> forwardLightParamsBuffer;
        private ConstantBuffer<ClusterSizes> clusterSizesBuffer;

        private ShadowAtlas shadowPool;

        private Quad quad;
        private ISamplerState linearClampSampler;
        private ISamplerState linearWrapSampler;
        private ISamplerState pointClampSampler;
        private ISamplerState shadowSampler;
        private unsafe void** cbs;
        private const uint nConstantBuffers = 3;
        private unsafe void** smps;
        private const uint nSamplers = 4;

        private unsafe void** forwardRtvs;
        private const uint nForwardRtvs = 3;

        private unsafe void** forwardSrvs;
        private const uint nForwardSrvs = 8 + 9;
        private const uint nForwardIndirectSrvsBase = 8 + 7;

        private unsafe void** forwardClusterdSrvs;
        private const uint nForwardClusterdSrvs = 8 + 11;
        private const uint nForwardClusterdIndirectSrvsBase = 8 + 9;

        private IGraphicsPipeline deferredIndirect;
        private unsafe void** indirectSrvs;
        private const uint nIndirectSrvsBase = 8 + 3;
        private const uint nIndirectSrvs = 8 + 3 + MaxGlobalLightProbes * 2;

        private IGraphicsPipeline deferred;
        private unsafe void** deferredSrvs;
        private const uint nDeferredSrvs = 9 + MaxDirectionalLightSDs;

        private IGraphicsPipeline deferredClusterd;
        private unsafe void** deferredClusterdSrvs;
        private const uint nDeferredClusterdSrvs = 11 + MaxDirectionalLightSDs;

        public const int MaxGlobalLightProbes = 1;
        public const int MaxDirectionalLightSDs = 1;
        public const int MaxPointLightSDs = 32;
        public const int MaxSpotlightSDs = 32;

        public IRenderTargetView Output;
        public ResourceRef<IDepthStencilView> DSV;
        public ResourceRef<GBuffer> GBuffers;
        public ResourceRef<IShaderResourceView> Depth;
        public ResourceRef<Texture2D> LUT;
        public ResourceRef<Texture2D> AO;
        public ResourceRef<IBuffer> Camera;
        public ResourceRef<IBuffer> Weather;

        private bool recreateClusters = true;
        public const uint CLUSTERS_X = 16;
        public const uint CLUSTERS_Y = 16;
        public const uint CLUSTERS_Z = 16;

        public const uint CLUSTERS_X_THREADS = 16;
        public const uint CLUSTERS_Y_THREADS = 16;
        public const uint CLUSTERS_Z_THREADS = 4;

        public const uint CLUSTER_COUNT = CLUSTERS_X * CLUSTERS_Y * CLUSTERS_Z;

        public const uint CLUSTER_MAX_LIGHTS = 128;

        public bool clustered = true;

        public LightManager()
        {
        }

        public static LightManager? Current => SceneManager.Current?.LightManager;

        public IReadOnlyList<ILightProbeComponent> Probes => probes;

        public IReadOnlyList<Light> Lights => lights;

        public IReadOnlyList<Light> Active => activeLights;

        public ShadowAtlas ShadowPool => shadowPool;

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
            Weather = ResourceManager2.Shared.GetBuffer("CBWeather");
            GBuffers = ResourceManager2.Shared.GetGBuffer("GBuffer");
            LUT = ResourceManager2.Shared.GetTexture("BRDFLUT");
            AO = ResourceManager2.Shared.GetTexture("AOBuffer");

            GlobalProbes = new(device, true, false);

            LightBuffer = new(device, true, false);
            ShadowDataBuffer = new(device, true, false);

            quad = new(device);

            shadowPool = new(device);

            linearClampSampler = ResourceManager2.Shared.GetOrAddSamplerState("PointClamp", SamplerDescription.LinearClamp).Value;
            linearWrapSampler = ResourceManager2.Shared.GetOrAddSamplerState("LinearWrap", SamplerDescription.LinearWrap).Value;
            pointClampSampler = ResourceManager2.Shared.GetOrAddSamplerState("PointClamp", SamplerDescription.PointClamp).Value;
            shadowSampler = ResourceManager2.Shared.GetOrAddSamplerState("LinearComparisonBorder", SamplerDescription.ComparisonLinearBorder).Value;

            unsafe
            {
                smps = AllocArrayAndZero(nSamplers);
                smps[0] = (void*)linearClampSampler.NativePointer;
                smps[1] = (void*)linearWrapSampler.NativePointer;
                smps[2] = (void*)pointClampSampler.NativePointer;
                smps[3] = (void*)shadowSampler.NativePointer;

                cbs = AllocArrayAndZero(nConstantBuffers);

                forwardSrvs = AllocArrayAndZero(nForwardSrvs);
                forwardClusterdSrvs = AllocArrayAndZero(nForwardClusterdSrvs);
                indirectSrvs = AllocArrayAndZero(nIndirectSrvs);
                deferredSrvs = AllocArrayAndZero(nDeferredSrvs);
                deferredClusterdSrvs = AllocArrayAndZero(nDeferredClusterdSrvs);

                forwardRtvs = AllocArrayAndZero(nForwardRtvs);
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

            deferred = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "deferred/brdf/vs.hlsl",
                PixelShader = "deferred/brdf/light.hlsl",
            },
            new GraphicsPipelineState()
            {
                Blend = BlendDescription.Additive,
                BlendFactor = Vector4.One,
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.TriangleList
            });

            deferredClusterd = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "deferred/brdf/vs.hlsl",
                PixelShader = "deferred/brdf/light.hlsl",
            },
            new GraphicsPipelineState()
            {
                Blend = BlendDescription.Additive,
                BlendFactor = Vector4.One,
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.TriangleList
            }, new ShaderMacro[] { new("CLUSTERED_DEFERRED", 1) });

            var window = Application.MainWindow;
            float screenWidth = window.RenderViewport.Width;
            float screenHeight = window.RenderViewport.Height;

            ClusterBuffer = new(device, CLUSTER_COUNT, false, false);
            LightIndexCounter = new(device, 1, false, false);
            LightIndexList = new(device, CLUSTER_COUNT * CLUSTER_MAX_LIGHTS, false, false);
            LightGridBuffer = new(device, CLUSTER_COUNT, false, false);
            clusterSizesBuffer = new(device, new ClusterSizes(screenWidth / CLUSTERS_X, screenHeight / CLUSTERS_Y), CpuAccessFlags.Write);

            clusterBuilding = await device.CreateComputePipelineAsync(new()
            {
                Path = "compute/clustered/building.hlsl"
            });
            clusterCulling = await device.CreateComputePipelineAsync(new()
            {
                Path = "compute/clustered/culling.hlsl"
            });
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
                light.DestroyShadowMap(shadowPool);
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

                    if (light.ShadowMapEnable)
                    {
                        light.CreateShadowMap(context.Device, shadowPool);
                    }
                    else
                    {
                        light.DestroyShadowMap(shadowPool);
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
                    if (!light.ShadowMapEnable || light.ShadowMapUpdateMode != ShadowUpdateMode.OnDemand)
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
                if (light.ShadowMapEnable && light is DirectionalLight && !light.InUpdateQueue || light.ShadowMapEnable && light.ShadowMapUpdateMode == ShadowUpdateMode.EveryFrame)
                {
                    light.InUpdateQueue = true;
                    UpdateShadowLightQueue.Enqueue(light);
                }
            }
        }

        public void BeginResize()
        {
        }

        public Task EndResize(int width, int height)
        {
#nullable disable
            Output = ResourceManager2.Shared.UpdateTexture("LightBuffer", new Texture2DDescription(Format.R16G16B16A16Float, width, height, 1, 1, BindFlags.ShaderResource | BindFlags.RenderTarget)).Value.RTV;
            DSV = ResourceManager2.Shared.GetDepthStencilView("GBuffer.DepthStencil");
            Depth = ResourceManager2.Shared.GetShaderResourceView("GBuffer.Depth");

#nullable enable
            UpdateResources();

            clusterSizesBuffer.Update(device.Context, new(width / CLUSTERS_X, height / CLUSTERS_Y));
            recreateClusters = true;

            return Task.CompletedTask;
        }

        private unsafe void UpdateResources()
        {
            cbs[1] = (void*)Camera.Value?.NativePointer;
            cbs[2] = (void*)Weather.Value?.NativePointer;
#nullable disable

            if (GBuffers != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (i < GBuffers.Value.Count)
                    {
                        deferredSrvs[i] = indirectSrvs[i] = deferredClusterdSrvs[i] = (void*)GBuffers.Value.SRVs[i]?.NativePointer;
                    }
                }
                deferredSrvs[4] = indirectSrvs[4] = deferredClusterdSrvs[4] = (void*)Depth.Value.NativePointer;
            }

            forwardSrvs[8] = forwardClusterdSrvs[8] = indirectSrvs[5] = deferredSrvs[5] = deferredClusterdSrvs[5] = (void*)AO.Value?.SRV.NativePointer;
            forwardSrvs[9] = forwardClusterdSrvs[9] = indirectSrvs[9] = (void*)LUT.Value?.SRV.NativePointer;
            forwardSrvs[10] = indirectSrvs[10] = (void*)GlobalProbes.SRV.NativePointer;

            forwardSrvs[11] = forwardClusterdSrvs[11] = deferredSrvs[6] = deferredClusterdSrvs[6] = (void*)LightBuffer.SRV.NativePointer;
            forwardSrvs[12] = forwardClusterdSrvs[12] = deferredSrvs[7] = deferredClusterdSrvs[7] = (void*)ShadowDataBuffer.SRV.NativePointer;
            forwardClusterdSrvs[13] = deferredClusterdSrvs[8] = (void*)LightIndexList.SRV.NativePointer;
            forwardClusterdSrvs[14] = deferredClusterdSrvs[9] = (void*)LightGridBuffer.SRV.NativePointer;
            forwardSrvs[13] = forwardClusterdSrvs[15] = deferredClusterdSrvs[10] = deferredSrvs[8] = (void*)shadowPool.SRV.NativePointer;

            forwardRtvs[0] = (void*)Output.NativePointer;
            forwardRtvs[1] = GBuffers.Value.PRTVs[1];
            forwardRtvs[2] = GBuffers.Value.PRTVs[2];

#nullable enable
        }

        private unsafe void UpdateLights(IGraphicsContext context, Camera camera)
        {
            GlobalProbes.ResetCounter();
            LightBuffer.ResetCounter();
            LightBuffer.Clear(context);
            ShadowDataBuffer.ResetCounter();
            uint globalProbesCount = 0;
            uint csmCount = 0;

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
                        if (globalProbesCount == MaxGlobalLightProbes)
                        {
                            continue;
                        }
                        GlobalProbes.Add((GlobalLightProbeComponent)probe);
                        forwardSrvs[nForwardIndirectSrvsBase + globalProbesCount] = forwardClusterdSrvs[nForwardClusterdIndirectSrvsBase + globalProbesCount] = indirectSrvs[nIndirectSrvsBase + globalProbesCount] = (void*)(probe.DiffuseTex?.SRV?.NativePointer ?? 0);
                        forwardSrvs[nForwardIndirectSrvsBase + MaxGlobalLightProbes + globalProbesCount] = forwardClusterdSrvs[nForwardClusterdIndirectSrvsBase + MaxGlobalLightProbes + globalProbesCount] = indirectSrvs[nIndirectSrvsBase + MaxGlobalLightProbes + globalProbesCount] = (void*)(probe.SpecularTex?.SRV?.NativePointer ?? 0);
                        globalProbesCount++;
                        break;

                    case ProbeType.Local:
                        break;
                }
            }

            for (uint i = globalProbesCount; i < MaxGlobalLightProbes; i++)
            {
                forwardSrvs[nForwardIndirectSrvsBase + i] = forwardClusterdSrvs[nForwardClusterdIndirectSrvsBase + i] = indirectSrvs[nIndirectSrvsBase + i] = null;
                forwardSrvs[nForwardIndirectSrvsBase + MaxGlobalLightProbes + i] = forwardClusterdSrvs[nForwardClusterdIndirectSrvsBase + MaxGlobalLightProbes + i] = indirectSrvs[nIndirectSrvsBase + MaxGlobalLightProbes + i] = null;
            }

            for (int i = 0; i < activeLights.Count; i++)
            {
                var light = activeLights[i];

                if (light.ShadowMapEnable)
                {
                    switch (light.LightType)
                    {
                        case LightType.Directional:
                            if (csmCount == MaxDirectionalLightSDs)
                            {
                                continue;
                            }

                            light.QueueIndex = ShadowDataBuffer.Count;
                            LightBuffer.Add(new((DirectionalLight)light, camera));
                            ShadowDataBuffer.Add(new((DirectionalLight)light, DirectionalLight.ShadowMapSize));
                            forwardSrvs[14] = forwardClusterdSrvs[16] = deferredSrvs[9] = deferredClusterdSrvs[11] = (void*)light.GetShadowMap()?.NativePointer;
                            ((DirectionalLight)light).UpdateShadowBuffer(ShadowDataBuffer, camera);
                            csmCount++;
                            break;

                        case LightType.Point:
                            light.QueueIndex = ShadowDataBuffer.Count;
                            LightBuffer.Add(new((PointLight)light, camera));
                            ShadowDataBuffer.Add(new((PointLight)light, PointLight.ShadowMapSize));
                            ((PointLight)light).UpdateShadowBuffer(ShadowDataBuffer);
                            break;

                        case LightType.Spot:
                            light.QueueIndex = ShadowDataBuffer.Count;
                            LightBuffer.Add(new((Spotlight)light, camera));
                            ShadowDataBuffer.Add(new((Spotlight)light, Spotlight.ShadowMapSize));
                            ((Spotlight)light).UpdateShadowBuffer(ShadowDataBuffer);
                            break;
                    }
                }
                else
                {
                    switch (light.LightType)
                    {
                        case LightType.Directional:
                            light.QueueIndex = LightBuffer.Count;
                            LightBuffer.Add(new((DirectionalLight)light, camera));
                            break;

                        case LightType.Point:
                            light.QueueIndex = LightBuffer.Count;
                            LightBuffer.Add(new((PointLight)light, camera));
                            break;

                        case LightType.Spot:
                            light.QueueIndex = LightBuffer.Count;
                            LightBuffer.Add(new((Spotlight)light, camera));
                            break;
                    }
                }
            }

            for (uint i = csmCount; i < MaxDirectionalLightSDs; i++)
            {
                forwardSrvs[14] = forwardClusterdSrvs[16] = deferredSrvs[9] = deferredClusterdSrvs[11] = null;
            }

            forwardSrvs[10] = forwardClusterdSrvs[10] = indirectSrvs[10] = (void*)GlobalProbes.SRV.NativePointer;

            forwardSrvs[11] = forwardClusterdSrvs[11] = deferredSrvs[6] = deferredClusterdSrvs[6] = (void*)LightBuffer.SRV.NativePointer;
            forwardSrvs[12] = forwardClusterdSrvs[12] = deferredSrvs[7] = deferredClusterdSrvs[7] = (void*)ShadowDataBuffer.SRV.NativePointer;
        }

        public unsafe void UpdateBuffers(IGraphicsContext context)
        {
            GlobalProbes.Update(context);
            LightBuffer.Update(context);
            ShadowDataBuffer.Update(context);
            lightParamsBuffer.Local->LightCount = LightBuffer.Count;
            lightParamsBuffer.Update(context);
        }

        public unsafe void CullLights(IGraphicsContext context)
        {
            context.CSSetConstantBuffer(1, Camera.Value);
            if (recreateClusters)
            {
                context.CSSetConstantBuffer(0, clusterSizesBuffer);
                context.CSSetUnorderedAccessView(0, (void*)ClusterBuffer.UAV.NativePointer);

                context.SetComputePipeline(clusterBuilding);
                context.Dispatch(CLUSTERS_X, CLUSTERS_Y, CLUSTERS_Z);
                context.SetComputePipeline(null);

                context.CSSetUnorderedAccessView(0, null);
                recreateClusters = false;
            }

            context.CSSetConstantBuffer(0, lightParamsBuffer);

            nint* srvs = stackalloc nint[] { ClusterBuffer.SRV.NativePointer, LightBuffer.SRV.NativePointer };
            context.CSSetShaderResources(0, 2, (void**)srvs);
            nint* uavs = stackalloc nint[] { LightIndexCounter.UAV.NativePointer, LightIndexList.UAV.NativePointer, LightGridBuffer.UAV.NativePointer };
            context.CSSetUnorderedAccessViews(0, 3, (void**)uavs, null);

            context.SetComputePipeline(clusterCulling);
            context.Dispatch(CLUSTERS_X / CLUSTERS_X_THREADS, CLUSTERS_Y / CLUSTERS_Y_THREADS, CLUSTERS_Z / CLUSTERS_Z_THREADS);
            context.SetComputePipeline(null);

            ZeroMemory(srvs, sizeof(nint) * 2);
            context.CSSetShaderResources(0, 2, (void**)srvs);
            ZeroMemory(uavs, sizeof(nint) * 3);
            context.CSSetUnorderedAccessViews(0, 3, (void**)uavs, null);
            context.CSSetConstantBuffer(1, null);
            context.CSSetConstantBuffer(0, null);
        }

        public void DeferredPass(IGraphicsContext context)
        {
            DeferredIndirect(context);
            if (clustered)
            {
                DeferredClustered(context);
            }
            else
            {
                Deferred(context);
            }
        }

        public unsafe void DeferredIndirect(IGraphicsContext context)
        {
            if (GlobalProbes.Count > 0)
            {
                // Indirect light pass
                var probeParams = probeParamsBuffer.Local;
                probeParams->GlobalProbes = GlobalProbes.Count;
                probeParamsBuffer.Update(context);
                cbs[0] = (void*)probeParamsBuffer.Buffer?.NativePointer;

                context.PSSetSamplers(0, nSamplers, smps);
                context.PSSetConstantBuffers(0, nConstantBuffers, cbs);
                context.PSSetShaderResources(0, nIndirectSrvs, indirectSrvs);

                quad.DrawAuto(context, deferredIndirect);
                deferredIndirect.EndDraw(context);

                nint* null_samplers = stackalloc nint[(int)nSamplers];
                context.PSSetSamplers(0, nSamplers, (void**)null_samplers);

                nint* null_cbs = stackalloc nint[(int)nConstantBuffers];
                context.PSSetConstantBuffers(0, nConstantBuffers, (void**)null_cbs);

                nint* null_srvs = stackalloc nint[(int)nIndirectSrvs];
                context.PSSetShaderResources(0, nIndirectSrvs, (void**)null_srvs);
            }
        }

        public unsafe void Deferred(IGraphicsContext context)
        {
            if (ActiveCount > 0)
            {
                // Direct light pass
                var lightParams = lightParamsBuffer.Local;
                lightParams->LightCount = LightBuffer.Count;
                lightParamsBuffer.Update(context);
                cbs[0] = (void*)lightParamsBuffer.Buffer?.NativePointer;

                context.PSSetSamplers(0, nSamplers, smps);
                context.PSSetConstantBuffers(0, nConstantBuffers, cbs);
                context.PSSetShaderResources(0, nDeferredSrvs, deferredSrvs);

                quad.DrawAuto(context, deferred);

                nint* null_samplers = stackalloc nint[(int)nSamplers];
                context.PSSetSamplers(0, nSamplers, (void**)null_samplers);

                nint* null_cbs = stackalloc nint[(int)nConstantBuffers];
                context.PSSetConstantBuffers(0, nConstantBuffers, (void**)null_cbs);

                nint* null_srvs = stackalloc nint[(int)nDeferredSrvs];
                context.PSSetShaderResources(0, nDeferredSrvs, (void**)null_srvs);
            }
        }

        public unsafe void DeferredClustered(IGraphicsContext context)
        {
            if (ActiveCount > 0)
            {
                // Direct clusterd light pass
                context.PSSetSamplers(0, nSamplers, smps);
                context.PSSetConstantBuffers(1, 1, &cbs[1]);
                context.PSSetShaderResources(0, nDeferredClusterdSrvs, deferredClusterdSrvs);

                quad.DrawAuto(context, deferredClusterd);

                nint* null_samplers = stackalloc nint[(int)nSamplers];
                context.PSSetSamplers(0, nSamplers, (void**)null_samplers);

                nint* null_cbs = stackalloc nint[(int)nConstantBuffers];
                context.PSSetConstantBuffers(0, nConstantBuffers, (void**)null_cbs);

                nint* null_srvs = stackalloc nint[(int)nDeferredClusterdSrvs];
                context.PSSetShaderResources(0, nDeferredClusterdSrvs, (void**)null_srvs);
            }
        }

        public unsafe void ForwardPass(IGraphicsContext context, IRendererComponent renderer, Camera camera)
        {
            if ((renderer.Flags & RendererFlags.Clustered) != 0)
            {
                ForwardClustered(context, renderer, camera);
            }
            else
            {
                Forward(context, renderer, camera);
            }
        }

        private unsafe void Forward(IGraphicsContext context, IRendererComponent renderer, Camera camera)
        {
            var lightParams = forwardLightParamsBuffer.Local;
            lightParams->LightCount = LightBuffer.Count;
            lightParams->GlobalProbes = GlobalProbes.Count;
            forwardLightParamsBuffer.Update(context);
            cbs[0] = (void*)forwardLightParamsBuffer.Buffer?.NativePointer;

            if ((renderer.Flags & RendererFlags.NoDepthTest) != 0)
            {
                context.SetRenderTargets(nForwardRtvs, forwardRtvs, null);
            }
            else
            {
                context.SetRenderTargets(nForwardRtvs, forwardRtvs, DSV.Value);
            }

            context.SetViewport(Output.Viewport);
            context.VSSetConstantBuffers(1, 1, &cbs[1]);
            context.DSSetConstantBuffers(1, 1, &cbs[1]);
            context.CSSetConstantBuffers(1, 1, &cbs[1]);
            context.PSSetConstantBuffers(0, nConstantBuffers, cbs);
            context.PSSetShaderResources(8, nForwardSrvs, forwardSrvs);
            context.PSSetSamplers(8, nSamplers, smps);

            renderer.Draw(context, RenderPath.Forward);

            nint* null_samplers = stackalloc nint[(int)nSamplers];
            context.PSSetSamplers(8, nSamplers, (void**)null_samplers);

            nint* null_srvs = stackalloc nint[(int)nForwardSrvs];
            context.PSSetShaderResources(8, nForwardSrvs, (void**)null_srvs);

            nint* null_cbs = stackalloc nint[(int)nConstantBuffers];
            context.PSSetConstantBuffers(0, nConstantBuffers, (void**)null_cbs);
        }

        private unsafe void ForwardClustered(IGraphicsContext context, IRendererComponent renderer, Camera camera)
        {
            var lightParams = forwardLightParamsBuffer.Local;
            lightParams->LightCount = LightBuffer.Count;
            lightParams->GlobalProbes = GlobalProbes.Count;
            forwardLightParamsBuffer.Update(context);
            cbs[0] = (void*)forwardLightParamsBuffer.Buffer?.NativePointer;

            if ((renderer.Flags & RendererFlags.NoDepthTest) != 0)
            {
                context.SetRenderTargets(nForwardRtvs, forwardRtvs, null);
            }
            else
            {
                context.SetRenderTargets(nForwardRtvs, forwardRtvs, DSV.Value);
            }

            context.SetViewport(Output.Viewport);
            context.VSSetConstantBuffers(1, 1, &cbs[1]);
            context.DSSetConstantBuffers(1, 1, &cbs[1]);
            context.CSSetConstantBuffers(1, 1, &cbs[1]);
            context.PSSetConstantBuffers(0, nConstantBuffers, cbs);
            context.PSSetShaderResources(0, nForwardClusterdSrvs, forwardClusterdSrvs);
            context.PSSetSamplers(8, nSamplers, smps);

            renderer.Draw(context, RenderPath.Forward);

            nint* null_samplers = stackalloc nint[(int)nSamplers];
            context.PSSetSamplers(8, nSamplers, (void**)null_samplers);

            nint* null_srvs = stackalloc nint[(int)nForwardClusterdSrvs];
            context.PSSetShaderResources(8, nForwardClusterdSrvs, (void**)null_srvs);

            nint* null_cbs = stackalloc nint[(int)nConstantBuffers];
            context.PSSetConstantBuffers(0, nConstantBuffers, (void**)null_cbs);
        }

        public unsafe void BakePass(IGraphicsContext context, IRendererComponent renderer, Camera camera)
        {
            if ((renderer.Flags & RendererFlags.Bake) == 0)
            {
                return;
            }
            if ((renderer.Flags & RendererFlags.Clustered) != 0)
            {
                BakeClustered(context, renderer, camera);
            }
            else
            {
                Bake(context, renderer, camera);
            }
        }

        private unsafe void Bake(IGraphicsContext context, IRendererComponent renderer, Camera camera)
        {
            var lightParams = forwardLightParamsBuffer.Local;
            lightParams->LightCount = LightBuffer.Count;
            lightParams->GlobalProbes = GlobalProbes.Count;
            forwardLightParamsBuffer.Update(context);
            cbs[0] = (void*)forwardLightParamsBuffer.Buffer?.NativePointer;

            if ((renderer.Flags & RendererFlags.NoDepthTest) != 0)
            {
                context.SetRenderTargets(nForwardRtvs, forwardRtvs, null);
            }
            else
            {
                context.SetRenderTargets(nForwardRtvs, forwardRtvs, DSV.Value);
            }

            context.SetViewport(Output.Viewport);
            context.VSSetConstantBuffers(1, 1, &cbs[1]);
            context.DSSetConstantBuffers(1, 1, &cbs[1]);
            context.CSSetConstantBuffers(1, 1, &cbs[1]);
            context.PSSetConstantBuffers(0, nConstantBuffers, cbs);
            context.PSSetShaderResources(8, nForwardSrvs, forwardSrvs);
            context.PSSetSamplers(8, nSamplers, smps);

            renderer.Draw(context, RenderPath.Forward);

            nint* null_samplers = stackalloc nint[(int)nSamplers];
            context.PSSetSamplers(8, nSamplers, (void**)null_samplers);

            nint* null_srvs = stackalloc nint[(int)nForwardSrvs];
            context.PSSetShaderResources(8, nForwardSrvs, (void**)null_srvs);

            nint* null_cbs = stackalloc nint[(int)nConstantBuffers];
            context.PSSetConstantBuffers(0, nConstantBuffers, (void**)null_cbs);
        }

        private unsafe void BakeClustered(IGraphicsContext context, IRendererComponent renderer, Camera camera)
        {
            var lightParams = forwardLightParamsBuffer.Local;
            lightParams->LightCount = LightBuffer.Count;
            lightParams->GlobalProbes = GlobalProbes.Count;
            forwardLightParamsBuffer.Update(context);
            cbs[0] = (void*)forwardLightParamsBuffer.Buffer?.NativePointer;

            if ((renderer.Flags & RendererFlags.NoDepthTest) != 0)
            {
                context.SetRenderTargets(nForwardRtvs, forwardRtvs, null);
            }
            else
            {
                context.SetRenderTargets(nForwardRtvs, forwardRtvs, DSV.Value);
            }

            context.SetViewport(Output.Viewport);
            context.VSSetConstantBuffers(1, 1, &cbs[1]);
            context.DSSetConstantBuffers(1, 1, &cbs[1]);
            context.CSSetConstantBuffers(1, 1, &cbs[1]);
            context.PSSetConstantBuffers(0, nConstantBuffers, cbs);
            context.PSSetShaderResources(0, nForwardClusterdSrvs, forwardClusterdSrvs);
            context.PSSetSamplers(8, nSamplers, smps);

            renderer.Bake(context);

            nint* null_samplers = stackalloc nint[(int)nSamplers];
            context.PSSetSamplers(8, nSamplers, (void**)null_samplers);

            nint* null_srvs = stackalloc nint[(int)nForwardClusterdSrvs];
            context.PSSetShaderResources(8, nForwardClusterdSrvs, (void**)null_srvs);

            nint* null_cbs = stackalloc nint[(int)nConstantBuffers];
            context.PSSetConstantBuffers(0, nConstantBuffers, (void**)null_cbs);
        }

        public unsafe void Dispose()
        {
            GlobalProbes.Dispose();

            LightBuffer.Dispose();
            ShadowDataBuffer.Dispose();

            ClusterBuffer.Dispose();
            LightIndexCounter.Dispose();
            LightIndexList.Dispose();
            LightGridBuffer.Dispose();

            clusterBuilding.Dispose();
            clusterCulling.Dispose();

            probeParamsBuffer.Dispose();
            lightParamsBuffer.Dispose();
            forwardLightParamsBuffer.Dispose();
            clusterSizesBuffer.Dispose();

            shadowPool.Dispose();

            quad.Dispose();

            deferredIndirect.Dispose();
            deferred.Dispose();
            deferredClusterd.Dispose();

            Free(indirectSrvs);
            Free(deferredSrvs);
            Free(deferredClusterdSrvs);
            Free(forwardSrvs);
            Free(forwardClusterdSrvs);
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
    }
}