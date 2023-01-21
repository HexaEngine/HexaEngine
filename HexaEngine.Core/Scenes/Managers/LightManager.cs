namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Mathematics;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Numerics;

    public class LightManager
    {
        private readonly List<Light> lights = new();
        private readonly List<Light> activeLights = new();
        private readonly IInstanceManager instanceManager;
        private readonly ConcurrentQueue<Light> updateQueue = new();
        private readonly ConcurrentQueue<ModelInstance> modelUpdateQueue = new();

        private readonly StructuredUavBuffer<DirectionalLightData> directionalLights;
        private readonly StructuredUavBuffer<PointLightData> pointLights;
        private readonly StructuredUavBuffer<SpotlightData> spotlights;

        private readonly StructuredUavBuffer<ShadowDirectionalLightData> shadowDirectionalLights;
        private readonly StructuredUavBuffer<ShadowPointLightData> shadowPointLights;
        private readonly StructuredUavBuffer<ShadowSpotlightData> shadowSpotlights;

        private readonly ConstantBuffer<LightBufferParams> paramsBuffer;

        private readonly Quad quad;
        private ISamplerState pointSampler;
        private ISamplerState anisoSampler;

        private readonly IGraphicsPipeline deferredDirect;
        private unsafe void** cbs;
        private unsafe void** smps;
        private unsafe void** directSrvs;
        private const uint ndirectSrvs = 8 + 3;
        private const uint nsrvs = 8 + 4 + CBLight.MaxDirectionalLightSDs + CBLight.MaxPointLightSDs + CBLight.MaxSpotlightSDs;

        private readonly IGraphicsPipeline deferredIndirect;
        private unsafe void** indirectSrvs;
        private const uint nindirectSrvs = 8 + 4;

        public IRenderTargetView Output;

        public IShaderResourceView[] GBuffers;

        public IShaderResourceView Irraidance;
        public IShaderResourceView EnvPrefiltered;
        public IShaderResourceView LUT;
        public IShaderResourceView SSAO;

        public IShaderResourceView CSM;
        public IShaderResourceView[] OSMs;
        public IShaderResourceView[] PSMs;
        public IBuffer Camera;

        /*
        private CSMPipeline csmPipeline;
        private Texture csmDepthBuffer;
        private ConstantBuffer<Matrix4x4> csmMvpBuffer;

        private OSMPipeline osmPipeline;
        private ConstantBuffer<Matrix4x4> osmBuffer;
        private IBuffer osmParamBuffer;
        private Texture[] osmDepthBuffers;
        private IShaderResourceView[] osmSRVs;

        private PSMPipeline psmPipeline;
        private IBuffer psmBuffer;
        private Texture[] psmDepthBuffers;
        private IShaderResourceView[] psmSRVs;
        */

        public LightManager(IGraphicsDevice device, IInstanceManager instanceManager)
        {
            this.instanceManager = instanceManager;
            instanceManager.Updated += InstanceManagerUpdated;
            paramsBuffer = new(device, CpuAccessFlags.Write);
            directionalLights = new(device, true, false);
            pointLights = new(device, true, false);
            spotlights = new(device, true, false);

            shadowDirectionalLights = new(device, true, false);
            shadowPointLights = new(device, true, false);
            shadowSpotlights = new(device, true, false);

            quad = new(device);
            deferredDirect = device.CreateGraphicsPipeline(new()
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

            pointSampler = ResourceManager.GetOrAddSamplerState("PointClamp", SamplerDescription.PointClamp);
            anisoSampler = ResourceManager.GetOrAddSamplerState("AnisotropicClamp", SamplerDescription.AnisotropicClamp);

            unsafe
            {
                smps = AllocArray(2);
                smps[0] = (void*)pointSampler.NativePointer;
                smps[1] = (void*)anisoSampler.NativePointer;
                directSrvs = AllocArray(ndirectSrvs);
                cbs = AllocArray(2);

                indirectSrvs = AllocArray(nindirectSrvs);
            }

            deferredIndirect = device.CreateGraphicsPipeline(new()
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
        }

        public IReadOnlyList<Light> Lights => lights;

        public IReadOnlyList<Light> Active => activeLights;

        public int Count => lights.Count;

        public int ActiveCount => activeLights.Count;

        private void InstanceManagerUpdated(ModelInstanceType type, ModelInstance instance)
        {
            modelUpdateQueue.Enqueue(instance);
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
                RemoveLight(light);
            }
        }

        public unsafe void AddLight(Light light)
        {
            lock (lights)
            {
                lights.Add(light);
                if (light.IsEnabled)
                    activeLights.Add(light);
                light.PropertyChanged += LightPropertyChanged;
            }
        }

        public unsafe void RemoveLight(Light light)
        {
            lock (lights)
            {
                light.PropertyChanged -= LightPropertyChanged;
                lights.Remove(light);
                activeLights.Remove(light);
            }
        }

        public unsafe void Update(IGraphicsContext context, Camera camera)
        {
            while (updateQueue.TryDequeue(out var light))
            {
                if (light.IsEnabled)
                {
                    if (!activeLights.Contains(light))
                        activeLights.Add(light);
                }
                else
                {
                    activeLights.Remove(light);
                }
            }
            Queue<Light> updateShadowLightQueue = new();
            while (modelUpdateQueue.TryDequeue(out var instance))
            {
                instance.GetBoundingBox(out BoundingBox box);
                for (int i = 0; i < activeLights.Count; i++)
                {
                    var light = activeLights[i];
                    if (!light.CastShadows)
                        continue;
                    if (light.IntersectFrustum(box) && !updateShadowLightQueue.Contains(light))
                    {
                        updateShadowLightQueue.Enqueue(light);
                    }
                }
            }

            //UpdateShadowMaps(context, camera, lights);
        }

        public void BeginResize()
        {
            ResourceManager.RequireUpdate("LightBuffer");
        }

        public async void EndResize(int width, int height)
        {
            Output = ResourceManager.UpdateTextureRTV("LightBuffer", TextureDescription.CreateTexture2DWithRTV(width, height, 1));

            Camera = await ResourceManager.GetConstantBufferAsync("CBCamera");
            GBuffers = await ResourceManager.GetTextureArraySRVAsync("GBuffer");
            Irraidance = await ResourceManager.GetTextureSRVAsync("EnvironmentIrradiance");
            EnvPrefiltered = await ResourceManager.GetTextureSRVAsync("EnvironmentPrefilter");
            LUT = await ResourceManager.GetTextureSRVAsync("BRDFLUT");
            SSAO = await ResourceManager.GetTextureSRVAsync("SSAOBuffer");
            UpdateResources();
        }

        public void UpdateTextures()
        {
            Irraidance = ResourceManager.GetTextureSRV("EnvironmentIrradiance");
            EnvPrefiltered = ResourceManager.GetTextureSRV("EnvironmentPrefilter");
            UpdateResources();
        }

        private unsafe void UpdateResources()
        {
            cbs[0] = (void*)paramsBuffer.Buffer?.NativePointer;
            cbs[1] = (void*)Camera?.NativePointer;

            if (GBuffers != null)
                for (int i = 0; i < 8; i++)
                {
                    if (i < GBuffers.Length)
                    {
                        indirectSrvs[i] = directSrvs[i] = (void*)GBuffers[i]?.NativePointer;
                    }
                }
            directSrvs[8] = (void*)directionalLights.SRV.NativePointer;
            directSrvs[9] = (void*)pointLights.SRV.NativePointer;
            directSrvs[10] = (void*)spotlights.SRV.NativePointer;

            indirectSrvs[8] = (void*)Irraidance?.NativePointer;
            indirectSrvs[9] = (void*)EnvPrefiltered?.NativePointer;
            indirectSrvs[10] = (void*)LUT?.NativePointer;
            indirectSrvs[11] = (void*)SSAO?.NativePointer;
        }

        private unsafe void UpdateDirectLights(IGraphicsContext context)
        {
            directionalLights.ResetCounter();
            pointLights.ResetCounter();
            spotlights.ResetCounter();

            for (int i = 0; i < lights.Count; i++)
            {
                var light = lights[i];
                switch (light.LightType)
                {
                    case LightType.Directional:
                        directionalLights.Add(new((DirectionalLight)light));
                        break;

                    case LightType.Point:
                        pointLights.Add(new((PointLight)light));
                        break;

                    case LightType.Spot:
                        spotlights.Add(new((Spotlight)light));
                        break;
                }
            }

            directionalLights.Update(context);
            pointLights.Update(context);
            spotlights.Update(context);

            directSrvs[8] = (void*)directionalLights.SRV.NativePointer;
            directSrvs[9] = (void*)pointLights.SRV.NativePointer;
            directSrvs[10] = (void*)spotlights.SRV.NativePointer;

            var lightParams = paramsBuffer.Local;
            lightParams->DirectionalLights = directionalLights.Count;
            lightParams->PointLights = pointLights.Count;
            lightParams->Spotlights = spotlights.Count;
            paramsBuffer.Update(context);
        }

        public unsafe void DeferredPass(IGraphicsContext context)
        {
            UpdateDirectLights(context);

            context.ClearRenderTargetView(Output, default);
            context.SetRenderTarget(Output, default);
            context.PSSetConstantBuffers(cbs, 2, 0);
            context.PSSetSamplers(smps, 2, 0);

            context.PSSetShaderResources(indirectSrvs, nindirectSrvs, 0);

            // Indirect light pass
            quad.DrawAuto(context, deferredIndirect, Output.Viewport);

            context.PSSetShaderResources(directSrvs, ndirectSrvs, 0);

            // Direct light pass
            quad.DrawAuto(context, deferredDirect, Output.Viewport);
        }

        public unsafe void Dispose()
        {
            instanceManager.Updated -= InstanceManagerUpdated;

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
            Free(directSrvs);
            Free(indirectSrvs);
            Free(smps);
            Free(cbs);
        }

        /*
        public unsafe void UpdateShadowMaps(IGraphicsContext context, Camera camera, CBLight* lights)
        {
            uint directsd = 0;
            uint pointsd = 0;
            uint spotsd = 0;

            // Draw light depth
            for (int i = 0; i < activeLights.Count; i++)
            {
                Light light = activeLights[i];

                switch (light.LightType)
                {
                    case LightType.Directional:
                        if (!light.CastShadows || !light.Updated)
                        {
                            if (light.CastShadows)
                                directsd++;
                            continue;
                        }
                        light.Updated = false;
                        UpdateDirectionalLight(context, directsd, camera, (DirectionalLight)light, lights);
                        directsd++;
                        break;

                    case LightType.Point:
                        if (!light.CastShadows || !light.Updated)
                        {
                            if (light.CastShadows)
                                pointsd++;
                            continue;
                        }
                        light.Updated = false;
                        UpdatePointLight(context, pointsd, (PointLight)light);
                        pointsd++;
                        break;

                    case LightType.Spot:
                        if (!light.CastShadows || !light.Updated)
                        {
                            if (light.CastShadows)
                                spotsd++;
                            continue;
                        }
                        light.Updated = false;
                        UpdateSpotlight(context, spotsd, (Spotlight)light, lights);
                        spotsd++;
                        break;
                }
            }
        }*/
        /*
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void UpdateDirectionalLight(IGraphicsContext context, uint index, Camera camera, DirectionalLight light, CBLight* lights)
        {
            CBDirectionalLightSD* directionalLight = lights->GetDirectionalLightSDs();
            Matrix4x4* views = directionalLight->GetViews();
            float* cascades = directionalLight->GetCascades();
            var mtxs = CSMHelper.GetLightSpaceMatrices(camera.Transform, light.Transform, views, cascades);
            context.Write(csmMvpBuffer.Buffer, mtxs, sizeof(Matrix4x4) * 16);

            csmDepthBuffer.ClearTarget(context, Vector4.Zero, DepthStencilClearFlags.All);
            context.SetRenderTarget(csmDepthBuffer.RenderTargetView, csmDepthBuffer.DepthStencilView);
            DrawScene(context, csmPipeline, csmDepthBuffer.Viewport, light.Transform.Frustum);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void UpdatePointLight(IGraphicsContext context, uint index, PointLight light)
        {
            OSMHelper.GetLightSpaceMatrices(light.Transform, light.ShadowRange, osmBuffer.Local, light.ShadowBox);
            osmBuffer.Update(context);
            context.Write(osmParamBuffer, new Vector4(light.Transform.GlobalPosition, light.ShadowRange));

            osmDepthBuffers[index].ClearTarget(context, Vector4.Zero, DepthStencilClearFlags.All);
            context.SetRenderTarget(osmDepthBuffers[index].RenderTargetView, osmDepthBuffers[index].DepthStencilView);
            DrawScene(context, osmPipeline, osmDepthBuffers[index].Viewport, *light.ShadowBox);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void UpdateSpotlight(IGraphicsContext context, uint index, Spotlight light, CBLight* lights)
        {
            CBSpotlightSD* spotlights = lights->GetSpotlightSDs();
            context.Write(psmBuffer, spotlights[index].View);

            psmDepthBuffers[index].ClearTarget(context, Vector4.Zero, DepthStencilClearFlags.All);
            context.SetRenderTarget(psmDepthBuffers[index].RenderTargetView, psmDepthBuffers[index].DepthStencilView);
            DrawScene(context, psmPipeline, psmDepthBuffers[index].Viewport, light.Transform.Frustum);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void DrawScene(IGraphicsContext context, IGraphicsPipeline pipeline, Viewport viewport, BoundingBox box)
        {
            pipeline.BeginDraw(context, viewport);
            for (int j = 0; j < instanceManager.TypeCount; j++)
            {
                instanceManager.Types[j].UpdateFrustumInstanceBuffer(box);
                instanceManager.Types[j].DrawNoOcclusion(context);
            }
            context.ClearState();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void DrawScene(IGraphicsContext context, IGraphicsPipeline pipeline, Viewport viewport, BoundingFrustum frustum)
        {
            pipeline.BeginDraw(context, viewport);
            for (int j = 0; j < instanceManager.TypeCount; j++)
            {
                instanceManager.Types[j].UpdateFrustumInstanceBuffer(frustum);
                instanceManager.Types[j].DrawNoOcclusion(context);
            }
            context.ClearState();
        }*/
    }
}