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

        private StructuredUavBuffer<GlobalProbeData> globalProbes;

        private StructuredUavBuffer<DirectionalLightData> directionalLights;
        private StructuredUavBuffer<PointLightData> pointLights;
        private StructuredUavBuffer<SpotlightData> spotlights;

        public StructuredUavBuffer<ShadowDirectionalLightData> ShadowDirectionalLights;
        public StructuredUavBuffer<ShadowPointLightData> ShadowPointLights;
        public StructuredUavBuffer<ShadowSpotlightData> ShadowSpotlights;

        private ConstantBuffer<ProbeBufferParams> probeParamsBuffer;
        private ConstantBuffer<LightBufferParams> lightParamsBuffer;
        private ConstantBuffer<ForwardLightBufferParams> forwardLightParamsBuffer;

        private Quad quad;
        private ISamplerState pointSampler;
        private ISamplerState linearSampler;
        private ISamplerState anisoSampler;
        private unsafe void** cbs;
        private unsafe void** smps;

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

        private IGraphicsPipeline forwardSoild;
        private unsafe void** solidSrvs;
        private const uint nSolidSrvs = 8;

        private IGraphicsPipeline forwardWireframe;

        public const int MaxGlobalLightProbes = 4;
        public const int MaxDirectionalLightSDs = 1;
        public const int MaxPointLightSDs = 32;
        public const int MaxSpotlightSDs = 32;

        public IRenderTargetView Output;
        public ResourceRef<IDepthStencilView> DSV;

        public ResourceRef<TextureArray> GBuffers;

        public ResourceRef<Texture> LUT;
        public ResourceRef<Texture> SSAO;

        public IShaderResourceView[] CSMs;
        public IShaderResourceView[] OSMs;
        public IShaderResourceView[] PSMs;
        public ResourceRef<IBuffer> Camera;

        public LightManager()
        {
        }

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

            globalProbes = new(device, true, false);

            directionalLights = new(device, true, false);
            pointLights = new(device, true, false);
            spotlights = new(device, true, false);

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

            pointSampler = ResourceManager2.Shared.GetOrAddSamplerState("PointClamp", SamplerDescription.PointClamp).Value;
            linearSampler = ResourceManager2.Shared.GetOrAddSamplerState("LinearClamp", SamplerDescription.LinearClamp).Value;
            anisoSampler = ResourceManager2.Shared.GetOrAddSamplerState("AnisotropicClamp", SamplerDescription.AnisotropicClamp).Value;

            unsafe
            {
                smps = AllocArray(2);
                smps[0] = (void*)pointSampler.NativePointer;
                smps[1] = (void*)linearSampler.NativePointer;

                cbs = AllocArray(3);

                forwardSrvs = AllocArray(nForwardSrvs);
                directSrvs = AllocArray(nDirectSrvs);
                indirectSrvs = AllocArray(nIndirectSrvs);
                shadowSrvs = AllocArray(nShadowSrvs);
                solidSrvs = AllocArray(nSolidSrvs);
                Zero(forwardSrvs, (uint)(nForwardSrvs * sizeof(nint)));
                Zero(directSrvs, (uint)(nDirectSrvs * sizeof(nint)));
                Zero(shadowSrvs, (uint)(nShadowSrvs * sizeof(nint)));
                Zero(indirectSrvs, (uint)(nIndirectSrvs * sizeof(nint)));
                Zero(solidSrvs, (uint)(nSolidSrvs * sizeof(nint)));

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

            forwardSoild = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "forward/solid/vs.hlsl",
                PixelShader = "forward/solid/ps.hlsl",
            },
            new GraphicsPipelineState()
            {
                Blend = BlendDescription.Opaque,
                BlendFactor = Vector4.Zero,
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.TriangleList
            });

            forwardWireframe = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "forward/wireframe/vs.hlsl",
                HullShader = "forward/wireframe/hs.hlsl",
                DomainShader = "forward/wireframe/ds.hlsl",
                PixelShader = "forward/wireframe/ps.hlsl",
            },
            new GraphicsPipelineState()
            {
                Blend = BlendDescription.Opaque,
                BlendFactor = Vector4.Zero,
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.Wireframe,
                Topology = PrimitiveTopology.PatchListWith3ControlPoints
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

            UpdateLights(context);

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

            globalProbes.Update(context);

            directionalLights.Update(context);
            pointLights.Update(context);
            spotlights.Update(context);

            ShadowDirectionalLights.Update(context);
            ShadowPointLights.Update(context);
            ShadowSpotlights.Update(context);
        }

        public void BeginResize()
        {
        }

        public Task EndResize(int width, int height)
        {
#nullable disable
            Output = ResourceManager2.Shared.UpdateTexture("LightBuffer", TextureDescription.CreateTexture2DWithRTV(width, height, 1)).Value.RenderTargetView;
            DSV = ResourceManager2.Shared.GetDepthStencilView("SwapChain.DSV");

            Camera = ResourceManager2.Shared.GetBuffer("CBCamera");
            GBuffers = ResourceManager2.Shared.GetTextureArray("GBuffer");
            LUT = ResourceManager2.Shared.GetTexture("BRDFLUT");
            SSAO = ResourceManager2.Shared.GetTexture("SSAOBuffer");

#nullable enable
            UpdateResources();
            return Task.CompletedTask;
        }

        private unsafe void UpdateResources()
        {
            cbs[1] = (void*)Camera.Value?.NativePointer;
#nullable disable

            if (GBuffers != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (i < GBuffers.Value.Count)
                    {
                        solidSrvs[i] = shadowSrvs[i] = indirectSrvs[i] = directSrvs[i] = (void*)GBuffers.Value.SRVs[i]?.NativePointer;
                    }
                }
            }

            forwardSrvs[8] = indirectSrvs[8] = (void*)SSAO.Value?.ShaderResourceView.NativePointer;
            forwardSrvs[9] = indirectSrvs[9] = (void*)LUT.Value?.ShaderResourceView.NativePointer;
            forwardSrvs[10] = indirectSrvs[10] = (void*)globalProbes.SRV.NativePointer;

            directSrvs[8] = (void*)SSAO.Value?.ShaderResourceView.NativePointer;
            forwardSrvs[11] = directSrvs[9] = (void*)directionalLights.SRV.NativePointer;
            forwardSrvs[12] = directSrvs[10] = (void*)pointLights.SRV.NativePointer;
            forwardSrvs[13] = directSrvs[11] = (void*)spotlights.SRV.NativePointer;

            shadowSrvs[8] = (void*)SSAO.Value?.ShaderResourceView.NativePointer;
            forwardSrvs[14] = shadowSrvs[9] = (void*)ShadowDirectionalLights.SRV.NativePointer;
            forwardSrvs[15] = shadowSrvs[10] = (void*)ShadowPointLights.SRV.NativePointer;
            forwardSrvs[16] = shadowSrvs[11] = (void*)ShadowSpotlights.SRV.NativePointer;

            forwardRtvs[0] = (void*)Output.NativePointer;
            forwardRtvs[1] = GBuffers.Value.PRTVs[1];
            forwardRtvs[2] = GBuffers.Value.PRTVs[2];

#nullable enable
        }

        private unsafe void UpdateLights(IGraphicsContext context)
        {
            globalProbes.ResetCounter();
            directionalLights.ResetCounter();
            pointLights.ResetCounter();
            spotlights.ResetCounter();
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
                        globalProbes.Add((GlobalLightProbeComponent)probe);
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
                            light.QueueIndex = directionalLights.Count;
                            directionalLights.Add(new((DirectionalLight)light));
                            break;

                        case LightType.Point:
                            light.QueueIndex = pointLights.Count;
                            pointLights.Add(new((PointLight)light));
                            break;

                        case LightType.Spot:
                            light.QueueIndex = spotlights.Count;
                            spotlights.Add(new((Spotlight)light));
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

            forwardSrvs[10] = indirectSrvs[10] = (void*)globalProbes.SRV.NativePointer;

            forwardSrvs[11] = directSrvs[9] = (void*)directionalLights.SRV.NativePointer;
            forwardSrvs[12] = directSrvs[10] = (void*)pointLights.SRV.NativePointer;
            forwardSrvs[13] = directSrvs[11] = (void*)spotlights.SRV.NativePointer;

            forwardSrvs[14] = shadowSrvs[9] = (void*)ShadowDirectionalLights.SRV.NativePointer;
            forwardSrvs[15] = shadowSrvs[10] = (void*)ShadowPointLights.SRV.NativePointer;
            forwardSrvs[16] = shadowSrvs[11] = (void*)ShadowSpotlights.SRV.NativePointer;
        }

        public unsafe void DeferredPass(IGraphicsContext context, ViewportShading shading, Camera camera)
        {
            if (shading == ViewportShading.Rendered)
            {
                context.SetRenderTarget(Output, default);
                context.SetViewport(Output.Viewport);
                context.PSSetSamplers(smps, 2, 0);

                // Indirect light pass
                var probeParams = probeParamsBuffer.Local;
                probeParams->GlobalProbes = globalProbes.Count;
                probeParamsBuffer.Update(context);
                cbs[0] = (void*)probeParamsBuffer.Buffer?.NativePointer;

                context.PSSetConstantBuffers(cbs, 2, 0);
                context.PSSetShaderResources(indirectSrvs, nIndirectSrvs, 0);

                quad.DrawAuto(context, deferredIndirect);

                // Direct light pass
                var lightParams = lightParamsBuffer.Local;
                lightParams->DirectionalLights = directionalLights.Count;
                lightParams->PointLights = pointLights.Count;
                lightParams->Spotlights = spotlights.Count;
                lightParamsBuffer.Update(context);
                cbs[0] = (void*)lightParamsBuffer.Buffer?.NativePointer;

                context.PSSetConstantBuffers(cbs, 2, 0);
                context.PSSetShaderResources(directSrvs, nDirectSrvs, 0);

                quad.DrawAuto(context, deferredDirect);

                // Shadow light pass
                lightParams->DirectionalLights = ShadowDirectionalLights.Count;
                lightParams->PointLights = ShadowPointLights.Count;
                lightParams->Spotlights = ShadowSpotlights.Count;
                lightParamsBuffer.Update(context);
                cbs[0] = (void*)lightParamsBuffer.Buffer?.NativePointer;

                context.PSSetConstantBuffers(cbs, 2, 0);
                context.PSSetShaderResources(shadowSrvs, nShadowSrvs, 0);

                quad.DrawAuto(context, deferredShadow);

                context.ClearState();
            }
        }

        public unsafe void ForwardPass(IGraphicsContext context, ViewportShading shading, Camera camera)
        {
            /*
            var types = instanceManager.Types;
            if (shading == ViewportShading.Solid)
            {
                context.SetRenderTarget(Output, DSV.Value);
                context.SetViewport(Output.Viewport);
                context.DSSetConstantBuffers(&cbs[1], 1, 1);
                context.PSSetConstantBuffers(&cbs[1], 1, 1);
                context.PSSetSamplers(smps, 2, 0);

                forwardSoild.BeginDraw(context);

                for (int j = 0; j < types.Count; j++)
                {
                    var type = types[j];
                    if (type.BeginDraw(context))
                    {
                        context.DrawIndexedInstancedIndirect(type.ArgBuffer, type.ArgBufferOffset);
                    }
                }
                context.ClearState();
            }

            if (shading == ViewportShading.Wireframe)
            {
                context.SetRenderTarget(Output, DSV.Value);
                context.SetViewport(Output.Viewport);
                context.DSSetConstantBuffers(&cbs[1], 1, 1);
                context.PSSetSamplers(smps, 2, 0);

                forwardWireframe.BeginDraw(context);

                for (int j = 0; j < types.Count; j++)
                {
                    var type = types[j];
                    if (type.BeginDraw(context))
                    {
                        context.DrawIndexedInstancedIndirect(type.ArgBuffer, type.ArgBufferOffset);
                    }
                }
                context.ClearState();
            }

            if (shading == ViewportShading.Rendered)
            {
                var lightParams = forwardLightParamsBuffer.Local;
                lightParams->DirectionalLights = directionalLights.Count;
                lightParams->PointLights = pointLights.Count;
                lightParams->Spotlights = spotlights.Count;
                lightParams->DirectionalLightSDs = shadowDirectionalLights.Count;
                lightParams->PointLightSDs = shadowPointLights.Count;
                lightParams->SpotlightSDs = shadowSpotlights.Count;
                lightParams->GlobalProbes = globalProbes.Count;
                forwardLightParamsBuffer.Update(context);
                cbs[0] = (void*)forwardLightParamsBuffer.Buffer?.NativePointer;

                context.SetRenderTargets(forwardRtvs, nForwardRtvs, DSV.Value);
                context.SetViewport(Output.Viewport);
                context.VSSetConstantBuffers(&cbs[1], 1, 1);
                context.DSSetConstantBuffers(&cbs[1], 1, 1);
                context.PSSetConstantBuffers(cbs, 2, 0);
                context.PSSetShaderResources(forwardSrvs, nForwardSrvs, 0);
                context.PSSetSamplers(smps, 2, 8);

                for (int j = 0; j < types.Count; j++)
                {
                    var type = types[j];
                    if (type.Forward && type.BeginDrawForward(context))
                    {
                        context.DrawIndexedInstancedIndirect(type.ArgBuffer, type.ArgBufferOffset);
                    }
                }

                context.ClearState();
            }
            */
        }

        public unsafe void ForwardPass(IGraphicsContext context, ViewportShading shading, Camera camera, IRenderTargetView rtv, IDepthStencilView dsv, Viewport viewport)
        {
            /*
            var types = instanceManager.Types;
            if (shading == ViewportShading.Solid && forwardSoild.IsValid)
            {
                context.SetGraphicsPipeline(forwardSoild);
                context.SetRenderTarget(rtv, dsv);
                context.SetViewport(viewport);
                context.VSSetConstantBuffers(&cbs[1], 1, 1);
                context.PSSetConstantBuffers(&cbs[1], 1, 1);
                context.PSSetSamplers(smps, 2, 0);

                for (int j = 0; j < types.Count; j++)
                {
                    var type = types[j];
                    if (type.BeginDraw(context))
                    {
                        context.DrawIndexedInstancedIndirect(type.ArgBuffer, type.ArgBufferOffset);
                    }
                }
                context.ClearState();
            }

            if (shading == ViewportShading.Wireframe && forwardWireframe.IsValid)
            {
                context.SetGraphicsPipeline(forwardWireframe);
                context.SetRenderTarget(rtv, dsv);
                context.SetViewport(viewport);
                context.DSSetConstantBuffers(&cbs[1], 1, 1);
                context.PSSetSamplers(smps, 2, 0);

                for (int j = 0; j < types.Count; j++)
                {
                    var type = types[j];
                    if (type.BeginDraw(context))
                    {
                        context.DrawIndexedInstancedIndirect(type.ArgBuffer, type.ArgBufferOffset);
                    }
                }
                context.ClearState();
            }
            */
        }

        public unsafe void Dispose()
        {
            globalProbes.Dispose();

            directionalLights.Dispose();
            pointLights.Dispose();
            spotlights.Dispose();

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
            forwardSoild.Dispose();
            forwardWireframe.Dispose();
            Free(solidSrvs);
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