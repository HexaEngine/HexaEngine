namespace HexaEngine.Core.Lights
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Lights.Probes;
    using HexaEngine.Core.Lights.Structs;
    using HexaEngine.Core.Lights.Types;
    using HexaEngine.Core.Renderers;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
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
        private ConstantBuffer<LightBufferParams> lightParamsBuffer;
        private ConstantBuffer<ForwardLightBufferParams> forwardLightParamsBuffer;
        private ConstantBuffer<ClusterSizes> clusterSizesBuffer;

        private ShadowAtlas shadowPool;

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

        private IGraphicsPipeline deferredClusterd;
        private unsafe void** clusterdSrvs;
        private const uint nClusterdSrvs = 11 + MaxDirectionalLightSDs;

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
            GBuffers = ResourceManager2.Shared.GetGBuffer("GBuffer");
            LUT = ResourceManager2.Shared.GetTexture("BRDFLUT");
            SSAO = ResourceManager2.Shared.GetTexture("AOBuffer");

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
                directSrvs = AllocArrayAndZero(nDirectSrvs);
                indirectSrvs = AllocArrayAndZero(nIndirectSrvs);
                shadowSrvs = AllocArrayAndZero(nShadowSrvs);

                clusterdSrvs = AllocArrayAndZero(nClusterdSrvs);

                forwardRtvs = AllocArrayAndZero(nForwardRtvs);
            }

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

            deferredClusterd = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "deferred/clustered/vs.hlsl",
                PixelShader = "deferred/clustered/light.hlsl",
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

                    if (light.ShadowMapEnable)
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
                if ((light.ShadowMapEnable && light is DirectionalLight && !light.InUpdateQueue) || (light.ShadowMapEnable && light.ShadowMapUpdateMode == ShadowUpdateMode.EveryFrame))
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
            Output = ResourceManager2.Shared.UpdateTexture("LightBuffer", TextureDescription.CreateTexture2DWithRTV(width, height, 1, Format.R16G16B16A16Float)).Value.RenderTargetView;
            DSV = ResourceManager2.Shared.GetDepthStencilView("GBuffer.DepthStencil");
            DepthSRV = ResourceManager2.Shared.GetShaderResourceView("GBuffer.Depth");

#nullable enable
            UpdateResources();

            clusterSizesBuffer.Update(device.Context, new(width / CLUSTERS_X, height / CLUSTERS_Y));
            recreateClusters = true;

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
                        shadowSrvs[i] = indirectSrvs[i] = directSrvs[i] = clusterdSrvs[i] = (void*)GBuffers.Value.SRVs[i]?.NativePointer;
                    }
                }
                shadowSrvs[4] = indirectSrvs[4] = directSrvs[4] = clusterdSrvs[4] = (void*)DepthSRV.Value.NativePointer;
            }

            indirectSrvs[5] = directSrvs[5] = shadowSrvs[5] = clusterdSrvs[5] = forwardSrvs[8] = (void*)SSAO.Value?.ShaderResourceView.NativePointer;
            forwardSrvs[9] = indirectSrvs[9] = (void*)LUT.Value?.ShaderResourceView.NativePointer;
            forwardSrvs[10] = indirectSrvs[10] = (void*)GlobalProbes.SRV.NativePointer;

            forwardSrvs[11] = directSrvs[6] = clusterdSrvs[6] = (void*)LightBuffer.SRV.NativePointer;
            forwardSrvs[12] = directSrvs[7] = clusterdSrvs[7] = (void*)ShadowDataBuffer.SRV.NativePointer;
            clusterdSrvs[8] = (void*)LightIndexList.SRV.NativePointer;
            clusterdSrvs[9] = (void*)LightGridBuffer.SRV.NativePointer;
            clusterdSrvs[10] = (void*)shadowPool.SRV.NativePointer;

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

                            light.QueueIndex = LightBuffer.Count;
                            LightBuffer.Add(new((DirectionalLight)light, camera));
                            ShadowDataBuffer.Add(new((DirectionalLight)light, DirectionalLight.ShadowMapSize));
                            forwardSrvs[nForwardShadowSrvsBase + csmCount] = shadowSrvs[nDirectSrvs + csmCount] = clusterdSrvs[11] = (void*)light.GetShadowMap()?.NativePointer;
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
                forwardSrvs[nForwardShadowSrvsBase + i] = shadowSrvs[nDirectSrvs + i] = null;
            }

            forwardSrvs[10] = indirectSrvs[10] = (void*)GlobalProbes.SRV.NativePointer;

            forwardSrvs[11] = directSrvs[6] = clusterdSrvs[6] = (void*)LightBuffer.SRV.NativePointer;
            forwardSrvs[12] = directSrvs[7] = clusterdSrvs[7] = (void*)ShadowDataBuffer.SRV.NativePointer;
        }

        public void UpdateBuffers(IGraphicsContext context)
        {
            GlobalProbes.Update(context);

            LightBuffer.Update(context);
            ShadowDataBuffer.Update(context);
        }

        public unsafe void CullLights(IGraphicsContext context)
        {
            lightParamsBuffer.Local->LightCount = LightBuffer.Count;
            lightParamsBuffer.Update(context);

            context.CSSetConstantBuffer(1, Camera.Value);
            if (recreateClusters)
            {
                context.CSSetConstantBuffer(0, clusterSizesBuffer);
                context.CSSetUnorderedAccessView(0, (void*)ClusterBuffer.UAV.NativePointer);

                context.SetComputePipeline(clusterBuilding);
                context.Dispatch(CLUSTERS_X, CLUSTERS_Y, CLUSTERS_Z);
                context.SetComputePipeline(null);

                context.CSSetUnorderedAccessView(0, null);

                recreateClusters = true;
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
            DeferredPassIndirect(context);
            if (clustered)
            {
                DeferredPassClusterd(context);
            }
            else
            {
                DeferredPassDirect(context);
            }
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

        public unsafe void DeferredPassDirect(IGraphicsContext context)
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
                context.PSSetShaderResources(0, nDirectSrvs, directSrvs);

                quad.DrawAuto(context, deferredDirect);

                nint* null_samplers = stackalloc nint[(int)nSamplers];
                context.PSSetSamplers(0, nSamplers, (void**)null_samplers);

                nint* null_cbs = stackalloc nint[(int)nConstantBuffers];
                context.PSSetConstantBuffers(0, nConstantBuffers, (void**)null_cbs);

                nint* null_srvs = stackalloc nint[(int)nIndirectSrvs];
                context.PSSetShaderResources(0, nIndirectSrvs, (void**)null_srvs);
            }
        }

        public unsafe void DeferredPassClusterd(IGraphicsContext context)
        {
            if (ActiveCount > 0)
            {
                // Direct clusterd light pass
                context.PSSetSamplers(0, nSamplers, smps);
                context.PSSetConstantBuffers(1, 1, &cbs[1]);
                context.PSSetShaderResources(0, nClusterdSrvs, clusterdSrvs);

                quad.DrawAuto(context, deferredClusterd);

                nint* null_samplers = stackalloc nint[(int)nSamplers];
                context.PSSetSamplers(0, nSamplers, (void**)null_samplers);

                nint* null_cbs = stackalloc nint[(int)nConstantBuffers];
                context.PSSetConstantBuffers(0, nConstantBuffers, (void**)null_cbs);

                nint* null_srvs = stackalloc nint[(int)nClusterdSrvs];
                context.PSSetShaderResources(0, nClusterdSrvs, (void**)null_srvs);
            }
        }

        public unsafe void ForwardPass(IGraphicsContext context, IRendererComponent renderer, Camera camera)
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
            context.PSSetShaderResources(0, nForwardSrvs, forwardSrvs);
            context.PSSetSamplers(8, nSamplers, smps);

            renderer.Draw(context);

            context.ClearState();
        }

        public unsafe void Dispose()
        {
            GlobalProbes.Dispose();

            LightBuffer.Dispose();
            ShadowDataBuffer.Dispose();
            shadowPool.Dispose();
            forwardLightParamsBuffer.Dispose();
            probeParamsBuffer.Dispose();
            lightParamsBuffer.Dispose();
            quad.Dispose();
            deferredDirect.Dispose();
            deferredIndirect.Dispose();
            deferredShadow.Dispose();
            deferredClusterd.Dispose();
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
    }
}