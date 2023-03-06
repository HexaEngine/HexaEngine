namespace HexaEngine.Core.Lights
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Instances;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Mathematics;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public enum ViewportShading
    {
        Rendered,
        Solid,
        Wireframe,
    }

    public partial class LightManager : ISystem
    {
        private readonly List<Light> lights = new();
        private readonly List<Light> activeLights = new();
        private IGraphicsDevice device;
        private readonly IInstanceManager instanceManager;
        private readonly ConcurrentQueue<Light> updateQueue = new();
        private readonly ConcurrentQueue<IInstance> modelUpdateQueue = new();

        private StructuredUavBuffer<DirectionalLightData> directionalLights;
        private StructuredUavBuffer<PointLightData> pointLights;
        private StructuredUavBuffer<SpotlightData> spotlights;

        private StructuredUavBuffer<ShadowDirectionalLightData> shadowDirectionalLights;
        private StructuredUavBuffer<ShadowPointLightData> shadowPointLights;
        private StructuredUavBuffer<ShadowSpotlightData> shadowSpotlights;

        private ConstantBuffer<LightBufferParams> paramsBuffer;

        private Quad quad;
        private ISamplerState pointSampler;
        private ISamplerState linearSampler;
        private ISamplerState anisoSampler;
        private unsafe void** cbs;
        private unsafe void** smps;

        private IGraphicsPipeline deferredDirect;
        private unsafe void** directSrvs;
        private const uint ndirectSrvs = 8 + 4;

        private IGraphicsPipeline deferredIndirect;
        private unsafe void** indirectSrvs;
        private const uint nindirectSrvs = 8 + 4;

        private IGraphicsPipeline deferredShadow;
        private unsafe void** shadowSrvs;
        private const uint nShadowSrvs = 8 + 4 + MaxDirectionalLightSDs + MaxPointLightSDs + MaxSpotlightSDs;

        private IGraphicsPipeline forwardSoild;
        private unsafe void** simpleSrvs;
        private const uint nSimpleSrvs = 8;

        private IGraphicsPipeline forwardWireframe;

        public const int MaxDirectionalLightSDs = 1;
        public const int MaxPointLightSDs = 32;
        public const int MaxSpotlightSDs = 32;

        public IRenderTargetView Output;
        public ResourceRef<IDepthStencilView> DSV;

        public ResourceRef<TextureArray> GBuffers;

        public ResourceRef<Texture> Irraidance;
        public ResourceRef<Texture> EnvPrefiltered;
        public ResourceRef<Texture> LUT;
        public ResourceRef<Texture> SSAO;

        public IShaderResourceView[] CSMs;
        public IShaderResourceView[] OSMs;
        public IShaderResourceView[] PSMs;
        public ResourceRef<IBuffer> Camera;

        public LightManager(IInstanceManager instanceManager)
        {
            this.instanceManager = instanceManager;
        }

        public IReadOnlyList<Light> Lights => lights;

        public IReadOnlyList<Light> Active => activeLights;

        public int Count => lights.Count;

        public int ActiveCount => activeLights.Count;

        public string Name => "Lights";

        public async Task Initialize(IGraphicsDevice device)
        {
            this.device = device;
            instanceManager.Updated += InstanceUpdated;
            instanceManager.InstanceCreated += InstanceCreated;
            instanceManager.InstanceDestroyed += InstanceDestroyed;
            paramsBuffer = new(device, CpuAccessFlags.Write);
            directionalLights = new(device, true, false);
            pointLights = new(device, true, false);
            spotlights = new(device, true, false);

            shadowDirectionalLights = new(device, true, false);
            shadowPointLights = new(device, true, false);
            shadowSpotlights = new(device, true, false);

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
                directSrvs = AllocArray(ndirectSrvs);
                cbs = AllocArray(2);

                indirectSrvs = AllocArray(nindirectSrvs);
                shadowSrvs = AllocArray(nShadowSrvs);
                Zero(shadowSrvs, (uint)(nShadowSrvs * sizeof(nint)));

                simpleSrvs = AllocArray(nSimpleSrvs);
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
                HullShader = "forward/solid/hs.hlsl",
                DomainShader = "forward/solid/ds.hlsl",
                PixelShader = "forward/solid/ps.hlsl",
            },
            new GraphicsPipelineState()
            {
                Blend = BlendDescription.Opaque,
                BlendFactor = Vector4.Zero,
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.PatchListWith3ControlPoints
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

        private void InstanceUpdated(IInstanceType type, IInstance instance)
        {
            modelUpdateQueue.Enqueue(instance);
        }

        private void InstanceDestroyed(IInstance instance)
        {
            modelUpdateQueue.Enqueue(instance);
        }

        private void InstanceCreated(IInstance instance)
        {
            modelUpdateQueue.Enqueue(instance);
        }

        private void LightTransformed(GameObject light)
        {
            updateQueue.Enqueue((Light)light);
        }

        private void LightPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is Light light)
                updateQueue.Enqueue(light);
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
        }

        public unsafe void Unregister(GameObject gameObject)
        {
            if (gameObject is Light light)
            {
                light.DestroyShadowMap();
                RemoveLight(light);
            }
        }

        public unsafe void AddLight(Light light)
        {
            lock (lights)
            {
                lights.Add(light);
                updateQueue.Enqueue(light);
                light.Transformed += LightTransformed;
                light.PropertyChanged += LightPropertyChanged;
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

        public unsafe void Update(IGraphicsContext context, Camera camera)
        {
            Queue<Light> updateShadowLightQueue = new();
            while (updateQueue.TryDequeue(out var light))
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
                        updateShadowLightQueue.Enqueue(light);
                    }
                }
                else
                {
                    activeLights.Remove(light);
                }
            }

            UpdateDirectLights(context);

            while (modelUpdateQueue.TryDequeue(out var instance))
            {
                instance.GetBoundingBox(out BoundingBox box);
                for (int i = 0; i < activeLights.Count; i++)
                {
                    var light = activeLights[i];
                    if (!light.CastShadows)
                        continue;
                    if (light.IntersectFrustum(box) && !light.InUpdateQueue)
                    {
                        light.InUpdateQueue = true;
                        updateShadowLightQueue.Enqueue(light);
                    }
                }
            }

            for (int i = 0; i < activeLights.Count; i++)
            {
                var light = activeLights[i];
                if (light.CastShadows && light is DirectionalLight && !light.InUpdateQueue)
                {
                    light.InUpdateQueue = true;
                    updateShadowLightQueue.Enqueue(light);
                }
            }

            UpdateShadowMaps(context, camera, updateShadowLightQueue);

            directionalLights.Update(context);
            pointLights.Update(context);
            spotlights.Update(context);

            shadowDirectionalLights.Update(context);
            shadowPointLights.Update(context);
            shadowSpotlights.Update(context);
        }

        public void BeginResize()
        {
        }

        public async Task EndResize(int width, int height)
        {
#nullable disable
            Output = ResourceManager2.Shared.UpdateTexture("LightBuffer", TextureDescription.CreateTexture2DWithRTV(width, height, 1)).Value.RenderTargetView;
            DSV = ResourceManager2.Shared.GetDepthStencilView("SwapChain.DSV");

            Camera = ResourceManager2.Shared.GetBuffer("CBCamera");
            GBuffers = ResourceManager2.Shared.GetTextureArray("GBuffer");
            Irraidance = ResourceManager2.Shared.GetTexture("EnvIrr");
            EnvPrefiltered = ResourceManager2.Shared.GetTexture("EnvPref");
            LUT = ResourceManager2.Shared.GetTexture("BRDFLUT");
            SSAO = ResourceManager2.Shared.GetTexture("SSAOBuffer");

#nullable enable
            UpdateResources();
        }

        public void UpdateTextures()
        {
            UpdateResources();
        }

        private unsafe void UpdateResources()
        {
#nullable disable
            cbs[0] = (void*)paramsBuffer.Buffer?.NativePointer;
            cbs[1] = (void*)Camera.Value?.NativePointer;

            if (GBuffers != null)
                for (int i = 0; i < 8; i++)
                {
                    if (i < GBuffers.Value.Count)
                    {
                        simpleSrvs[i] = shadowSrvs[i] = indirectSrvs[i] = directSrvs[i] = (void*)GBuffers.Value.SRVs[i]?.NativePointer;
                    }
                }
            directSrvs[8] = (void*)SSAO.Value?.ShaderResourceView.NativePointer;
            directSrvs[9] = (void*)directionalLights.SRV.NativePointer;
            directSrvs[10] = (void*)pointLights.SRV.NativePointer;
            directSrvs[11] = (void*)spotlights.SRV.NativePointer;

            shadowSrvs[8] = (void*)SSAO.Value?.ShaderResourceView.NativePointer;
            shadowSrvs[9] = (void*)shadowDirectionalLights.SRV.NativePointer;
            shadowSrvs[10] = (void*)shadowPointLights.SRV.NativePointer;
            shadowSrvs[11] = (void*)shadowSpotlights.SRV.NativePointer;

            indirectSrvs[8] = (void*)SSAO.Value?.ShaderResourceView.NativePointer;
            indirectSrvs[9] = (void*)Irraidance.Value?.ShaderResourceView.NativePointer;
            indirectSrvs[10] = (void*)EnvPrefiltered.Value?.ShaderResourceView.NativePointer;
            indirectSrvs[11] = (void*)LUT.Value?.ShaderResourceView.NativePointer;

#nullable enable
        }

        private unsafe void UpdateDirectLights(IGraphicsContext context)
        {
            directionalLights.ResetCounter();
            pointLights.ResetCounter();
            spotlights.ResetCounter();
            shadowDirectionalLights.ResetCounter();
            shadowPointLights.ResetCounter();
            shadowSpotlights.ResetCounter();
            uint csmCount = 0;
            uint osmCount = 0;
            uint psmCount = 0;

            for (int i = 0; i < activeLights.Count; i++)
            {
                var light = activeLights[i];
                if (light.CastShadows)
                {
                    switch (light.LightType)
                    {
                        case LightType.Directional:
                            if (csmCount == MaxDirectionalLightSDs)
                                continue;
                            light.QueueIndex = csmCount;
                            shadowDirectionalLights.Add(new((DirectionalLight)light));
#pragma warning disable CS8629 // Nullable value type may be null.
                            shadowSrvs[ndirectSrvs + csmCount] = (void*)light.GetShadowMap()?.NativePointer;
#pragma warning restore CS8629 // Nullable value type may be null.
                            csmCount++;
                            break;

                        case LightType.Point:
                            if (osmCount == MaxPointLightSDs)
                                continue;
                            light.QueueIndex = osmCount;
                            shadowPointLights.Add(new((PointLight)light));
#pragma warning disable CS8629 // Nullable value type may be null.
                            shadowSrvs[ndirectSrvs + MaxDirectionalLightSDs + osmCount] = (void*)light.GetShadowMap()?.NativePointer;
#pragma warning restore CS8629 // Nullable value type may be null.
                            osmCount++;
                            break;

                        case LightType.Spot:
                            if (psmCount == MaxSpotlightSDs)
                                continue;
                            light.QueueIndex = psmCount;
                            shadowSpotlights.Add(new((Spotlight)light));
#pragma warning disable CS8629 // Nullable value type may be null.
                            shadowSrvs[ndirectSrvs + MaxDirectionalLightSDs + MaxPointLightSDs + psmCount] = (void*)light.GetShadowMap()?.NativePointer;
#pragma warning restore CS8629 // Nullable value type may be null.
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
                shadowSrvs[ndirectSrvs + i] = null;
            }
            for (uint i = osmCount; i < MaxPointLightSDs; i++)
            {
                shadowSrvs[ndirectSrvs + MaxDirectionalLightSDs + i] = null;
            }
            for (uint i = psmCount; i < MaxSpotlightSDs; i++)
            {
                shadowSrvs[ndirectSrvs + MaxDirectionalLightSDs + MaxPointLightSDs + i] = null;
            }

            directSrvs[9] = (void*)directionalLights.SRV.NativePointer;
            directSrvs[10] = (void*)pointLights.SRV.NativePointer;
            directSrvs[11] = (void*)spotlights.SRV.NativePointer;
        }

        public unsafe void DeferredPass(IGraphicsContext context, ViewportShading shading, Camera camera)
        {
            if (shading == ViewportShading.Rendered)
            {
                context.SetRenderTarget(Output, default);
                context.SetViewport(Output.Viewport);
                context.PSSetConstantBuffers(cbs, 2, 0);
                context.PSSetSamplers(smps, 2, 0);

                context.PSSetShaderResources(indirectSrvs, nindirectSrvs, 0);

                // Indirect light pass
                quad.DrawAuto(context, deferredIndirect);

                var lightParams = paramsBuffer.Local;
                lightParams->DirectionalLights = directionalLights.Count;
                lightParams->PointLights = pointLights.Count;
                lightParams->Spotlights = spotlights.Count;
                paramsBuffer.Update(context);

                context.PSSetShaderResources(directSrvs, ndirectSrvs, 0);

                // Direct light pass
                quad.DrawAuto(context, deferredDirect);

                lightParams->DirectionalLights = shadowDirectionalLights.Count;
                lightParams->PointLights = shadowPointLights.Count;
                lightParams->Spotlights = shadowSpotlights.Count;
                paramsBuffer.Update(context);

                context.PSSetShaderResources(shadowSrvs, nShadowSrvs, 0);

                // Shadow light pass
                quad.DrawAuto(context, deferredShadow);
            }
        }

        public unsafe void ForwardPass(IGraphicsContext context, ViewportShading shading, Camera camera)
        {
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
        }

        public unsafe void ForwardPass(IGraphicsContext context, ViewportShading shading, Camera camera, IRenderTargetView rtv, IDepthStencilView dsv, Viewport viewport)
        {
            var types = instanceManager.Types;
            if (shading == ViewportShading.Solid)
            {
                context.SetRenderTarget(rtv, dsv);
                context.SetViewport(viewport);
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
                context.SetRenderTarget(rtv, dsv);
                context.SetViewport(viewport);
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
        }

        public unsafe void Dispose()
        {
            instanceManager.Updated -= InstanceUpdated;
            instanceManager.InstanceCreated -= InstanceCreated;
            instanceManager.InstanceDestroyed -= InstanceDestroyed;

            directionalLights.Dispose();
            pointLights.Dispose();
            spotlights.Dispose();

            shadowDirectionalLights.Dispose();
            shadowPointLights.Dispose();
            shadowSpotlights.Dispose();

            paramsBuffer.Dispose();
            quad.Dispose();
            deferredDirect.Dispose();
            deferredIndirect.Dispose();
            deferredShadow.Dispose();
            forwardSoild.Dispose();
            forwardWireframe.Dispose();
            Free(simpleSrvs);
            Free(directSrvs);
            Free(indirectSrvs);
            Free(shadowSrvs);
            Free(smps);
            Free(cbs);
        }

        public unsafe void UpdateShadowMaps(IGraphicsContext context, Camera camera, Queue<Light> lights)
        {
            while (lights.TryDequeue(out var light))
            {
                switch (light.LightType)
                {
                    case LightType.Directional:
                        ((DirectionalLight)light).UpdateShadowMap(context, shadowDirectionalLights, camera, instanceManager);
                        break;

                    case LightType.Point:
                        ((PointLight)light).UpdateShadowMap(context, shadowPointLights, instanceManager);
                        break;

                    case LightType.Spot:
                        ((Spotlight)light).UpdateShadowMap(context, shadowSpotlights, instanceManager);
                        break;
                }
                light.InUpdateQueue = false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Awake(BepuUtilities.ThreadDispatcher dispatcher)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Update(BepuUtilities.ThreadDispatcher dispatcher)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void FixedUpdate(BepuUtilities.ThreadDispatcher dispatcher)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Destroy(BepuUtilities.ThreadDispatcher dispatcher)
        {
        }
    }
}