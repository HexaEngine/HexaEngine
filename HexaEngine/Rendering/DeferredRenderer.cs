namespace HexaEngine.Rendering
{
    using HexaEngine.Cameras;
    using HexaEngine.Core;
    using HexaEngine.Core.Events;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Editor;
    using HexaEngine.Editor.Widgets;
    using HexaEngine.Graphics;
    using HexaEngine.IO;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Objects;
    using HexaEngine.Objects.Primitives;
    using HexaEngine.Pipelines.Deferred.Lighting;
    using HexaEngine.Pipelines.Deferred.PrePass;
    using HexaEngine.Pipelines.Effects;
    using HexaEngine.Pipelines.Forward;
    using HexaEngine.Rendering.ConstantBuffers;
    using HexaEngine.Resources;
    using HexaEngine.Scenes;
    using HexaEngine.Windows;
    using ImGuiNET;
    using System;
    using System.Numerics;

    public class DeferredRenderer : ISceneRenderer
    {
#nullable disable
        private bool initialized;
        private bool dirty = true;
        private ICommandList commandList;
        private ResourceManager resourceManager;
        private bool disposedValue;
        private IGraphicsDevice device;
        private IGraphicsContext context;
        private IGraphicsContext deferredContext;
        private ISwapChain swapChain;

        private Quad quad;

        private readonly unsafe CBLight* lights = Alloc<CBLight>();
        private ConstantBuffer<CBCamera> cameraBuffer;

        private IBuffer lightBuffer;
        private ConstantBuffer<CBWorld> skyboxBuffer;
        private IBuffer tesselationBuffer;
        private GeometryPass geometry;
        private MTLDepthShaderBack materialDepthBackface;
        private MTLDepthShaderFront materialDepthFrontface;
        private PrincipledBSDF light;
        private Skybox skybox;

        private DepthBuffer depthStencil;
        private IDepthStencilView dsv;
        private RenderTextureArray gbuffers;

        private RenderTexture ssaoBuffer;
        private RenderTexture lightMap;
        private RenderTexture dofBuffer;
        private RenderTexture autoExposureBuffer;
        private RenderTexture tonemapBuffer;
        private RenderTexture fxaaBuffer;
        private RenderTexture ssrBuffer;
        private RenderTexture depthbuffer;

        private CSMPipeline csmPipeline;
        private RenderTexture csmDepthBuffer;
        private IBuffer csmMvpBuffer;

        private OSMPipeline osmPipeline;
        private IBuffer osmBuffer;
        private IBuffer osmParamBuffer;
        private RenderTexture[] osmDepthBuffers;
        private IShaderResourceView[] osmSRVs;

        private PSMPipeline psmPipeline;
        private IBuffer psmBuffer;
        private RenderTexture[] psmDepthBuffers;
        private IShaderResourceView[] psmSRVs;

        private ISamplerState pointSampler;
        private ISamplerState anisoSampler;
        private ISamplerState linearSampler;

        private DepthOfField dof;
        private HBAO ssao;
        private DDASSR ssr;
        private Tonemap tonemap;
        private FXAA fxaa;
        private AutoExposure autoExposure;
        private Bloom bloom;

        private BRDFLUT brdfLUT;

        private ISamplerState envsmp;
        private RenderTexture env;
        private RenderTexture envirr;
        private RenderTexture envfilter;

        private RenderTexture brdflut;

        private ConfigKey configKey;
        private int currentShadingModelIndex;
        private float renderResolution;
        private int width;
        private int height;
        private int windowWidth;
        private int windowHeight;

#nullable enable

        public DeferredRenderer()
        {
        }

        public Task Initialize(IGraphicsDevice device, Window window)
        {
            return
            Task.Factory.StartNew(() =>
            {
                unsafe
                {
                    Zero(lights);
                }
                configKey = Config.Global.GetOrCreateKey("Renderer");
                renderResolution = configKey.TryGet(nameof(renderResolution), 1f);

                windowWidth = window.Width;
                windowHeight = window.Height;
                width = (int)(window.Width * renderResolution);
                height = (int)(window.Height * renderResolution);
                SceneManager.SceneChanged += SceneManager_SceneChanged;
                resourceManager = new ResourceManager(device);

                this.device = device;
                context = device.Context;
                swapChain = device.SwapChain ?? throw new NotSupportedException("Device needs a swapchain to operate properly");
                swapChain.Resizing += OnResizeBegin;
                swapChain.Resized += OnResizeEnd;

                #region Settings

                #region Common

                {
                    configKey.TryGetOrAddKeyValue("VSync", false.ToString(), DataType.Bool, out var val);
                    swapChain.VSync = val.GetBool();
                    val.ValueChanged += (ss, ee) =>
                    {
                        swapChain.VSync = ss.GetBool();
                        Config.Global.Save();
                    };
                }
                {
                    configKey.TryGetOrAddKeyValue("LimitFPS", false.ToString(), DataType.Bool, out var val);
                    swapChain.LimitFPS = val.GetBool();
                    val.ValueChanged += (ss, ee) =>
                    {
                        swapChain.LimitFPS = ss.GetBool();
                        Config.Global.Save();
                    };
                }
                {
                    configKey.TryGetOrAddKeyValue("TargetFPS", 120.ToString(), DataType.Int32, out var val);
                    swapChain.TargetFPS = val.GetInt32();
                    val.ValueChanged += (ss, ee) =>
                    {
                        swapChain.TargetFPS = ss.GetInt32();
                        Config.Global.Save();
                    };
                }

                #endregion Common

                #endregion Settings

                Config.Global.Save();

                quad = new(device);

                pointSampler = device.CreateSamplerState(SamplerDescription.PointClamp);
                anisoSampler = device.CreateSamplerState(SamplerDescription.AnisotropicClamp);
                linearSampler = device.CreateSamplerState(SamplerDescription.LinearClamp);

                cameraBuffer = new(device, CpuAccessFlags.Write); //device.CreateBuffer(new CBCamera(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
                lightBuffer = device.CreateBuffer(new CBLight(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
                skyboxBuffer = new(device, CpuAccessFlags.Write);
                tesselationBuffer = device.CreateBuffer(new CBTessellation(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);

                csmDepthBuffer = new(device, TextureDescription.CreateTexture2DArrayWithRTV(4096, 4096, 3, 1, Format.R32Float), DepthStencilDesc.Default);
                csmMvpBuffer = device.CreateBuffer(new Matrix4x4[16], BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
                csmPipeline = new(device);
                csmPipeline.View = csmMvpBuffer;

                osmDepthBuffers = new RenderTexture[8];
                osmSRVs = new IShaderResourceView[8];
                for (int i = 0; i < 8; i++)
                {
                    osmDepthBuffers[i] = new(device, TextureDescription.CreateTextureCubeWithRTV(2048, 1, Format.R32Float), DepthStencilDesc.Default);
                    osmSRVs[i] = osmDepthBuffers[i].ResourceView;
                }

                osmBuffer = device.CreateBuffer(new Matrix4x4[6], BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
                osmParamBuffer = device.CreateBuffer(new Vector4(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
                osmPipeline = new(device);
                osmPipeline.View = osmBuffer;
                osmPipeline.Light = osmParamBuffer;

                psmDepthBuffers = new RenderTexture[8];
                psmSRVs = new IShaderResourceView[8];
                for (int i = 0; i < 8; i++)
                {
                    psmDepthBuffers[i] = new(device, TextureDescription.CreateTexture2DWithRTV(2048, 2048, 1, Format.R32Float), DepthStencilDesc.Default);
                    psmSRVs[i] = psmDepthBuffers[i].ResourceView;
                }

                psmBuffer = device.CreateBuffer(new Matrix4x4(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
                psmPipeline = new(device);
                psmPipeline.View = psmBuffer;

                geometry = new(device);
                geometry.Camera = cameraBuffer.Buffer;
                materialDepthBackface = new(device);
                materialDepthBackface.Camera = cameraBuffer.Buffer;
                materialDepthFrontface = new(device);
                materialDepthFrontface.Camera = cameraBuffer.Buffer;

                gbuffers = new RenderTextureArray(device, width, height, 8, Format.RGBA32Float);
                depthStencil = new DepthBuffer(device, new(width, height, 1, Format.Depth24UNormStencil8, BindFlags.DepthStencil, Usage.Default, CpuAccessFlags.None, DepthStencilViewFlags.None, SampleDescription.Default));
                dsv = depthStencil.DSV;

                depthbuffer = new(device, TextureDescription.CreateTexture2DWithRTV(width, height, 1, Format.R32Float), DepthStencilDesc.Default);

                ssaoBuffer = new(device, TextureDescription.CreateTexture2DWithRTV(width, height, 1, Format.R32Float));
                ssao = new(device, width, height);
                ssao.Output = ssaoBuffer.RenderTargetView;
                ssao.Camera = cameraBuffer.Buffer;
                ssao.Position = gbuffers.SRVs[1];
                ssao.Normal = gbuffers.SRVs[2];
                ssao.Resize(device, width, height);
                configKey.GenerateSubKeyAuto(ssao, "HBAO");

                env = new(device, new TextureFileDescription(Paths.CurrentTexturePath + "env_o.dds", TextureDimension.TextureCube));
                envirr = new(device, new TextureFileDescription(Paths.CurrentTexturePath + "irradiance_o.dds", TextureDimension.TextureCube));
                envfilter = new(device, new TextureFileDescription(Paths.CurrentTexturePath + "prefilter_o.dds", TextureDimension.TextureCube));

                envsmp = device.CreateSamplerState(SamplerDescription.AnisotropicClamp);

                brdflut = new(device, TextureDescription.CreateTexture2DWithRTV(512, 512, 1, Format.RGBA32Float));

                window.RenderDispatcher.InvokeBlocking(() =>
                {
                    brdfLUT = new(device);
                    brdfLUT.Target = brdflut.RenderTargetView;
                    brdfLUT.Draw(context);
                    context.ClearState();
                    brdfLUT.Dispose();
                });

                lightMap = new(device, TextureDescription.CreateTexture2DWithRTV(width, height, 1));
                light = new(device);
                light.Output = lightMap.RenderTargetView;
                light.Lights = lightBuffer;
                light.Camera = cameraBuffer.Buffer;
                light.GBuffers = gbuffers.SRVs;
                light.Irraidance = envirr.ResourceView;
                light.EnvPrefiltered = envfilter.ResourceView;
                light.LUT = brdflut.ResourceView;
                light.SSAO = ssaoBuffer.ResourceView;
                light.CSM = csmDepthBuffer.ResourceView;
                light.OSMs = osmDepthBuffers.Select(x => x.ResourceView).ToArray();
                light.PSMs = psmDepthBuffers.Select(x => x.ResourceView).ToArray();
                light.Resize();

                fxaaBuffer = new(device, null, TextureDescription.CreateTexture2DWithRTV(width, height, 1));
                fxaa = new(device);
                fxaa.Output = swapChain.BackbufferRTV;
                fxaa.Source = fxaaBuffer.ResourceView;

                tonemapBuffer = new(device, null, TextureDescription.CreateTexture2DWithRTV(width, height, 1));
                tonemap = new(device);
                tonemap.Output = fxaaBuffer.RenderTargetView;
                tonemap.HDR = tonemapBuffer.ResourceView;
                configKey.GenerateSubKeyAuto(tonemap, "Tonemap");

                bloom = new(device);
                bloom.Source = tonemapBuffer.ResourceView;
                bloom.Resize(device, width, height);
                tonemap.Bloom = bloom.Output;
                tonemap.Resize();
                configKey.GenerateSubKeyAuto(bloom, "Bloom");

                autoExposureBuffer = new(device, TextureDescription.CreateTexture2DWithRTV(width, height, 1));
                autoExposure = new(device, width, height);
                autoExposure.Output = tonemapBuffer.RenderTargetView;
                autoExposure.Color = autoExposureBuffer.ResourceView;
                configKey.GenerateSubKeyAuto(autoExposure, "AutoExposure");

                dofBuffer = new(device, dsv, TextureDescription.CreateTexture2DWithRTV(width, height, 1));
                dof = new(device, width, height);
                dof.Target = autoExposureBuffer.RenderTargetView;
                dof.Color = dofBuffer.ResourceView;
                dof.Position = gbuffers.SRVs[1];
                dof.Camera = cameraBuffer.Buffer;
                configKey.GenerateSubKeyAuto(dof, "Dof");

                skybox = new(device);
                skybox.Output = dofBuffer.RenderTargetView;
                skybox.DSV = dsv;
                skybox.Env = env.ResourceView;
                skybox.World = skyboxBuffer.Buffer;
                skybox.Camera = cameraBuffer.Buffer;
                skybox.Resize();

                ssrBuffer = new(device, TextureDescription.CreateTexture2DWithRTV(width, height, 1));
                ssr = new(device, width, height);
                ssr.Output = dofBuffer.RenderTargetView;
                ssr.Camera = cameraBuffer.Buffer;
                ssr.Color = lightMap.ResourceView;
                ssr.Position = gbuffers.SRVs[1];
                ssr.Normal = gbuffers.SRVs[2];
                ssr.Depth = depthbuffer.ResourceView;
                configKey.GenerateSubKeyAuto(ssr, "SSR");

                deferredContext = device.CreateDeferredContext();

                initialized = true;
                window.RenderDispatcher.Invoke(() => WidgetManager.Register(new RendererWidget(this)));
            });
        }

        private void SceneManager_SceneChanged(object? sender, SceneChangedEventArgs e)
        {
            if (!initialized) return;
            if (e.Old != null && e.Old != e.New)
            {
                Update(e.Old).Wait();
            }
        }

        private void OnResizeBegin(object? sender, EventArgs e)
        {
            if (!initialized) return;
            depthStencil.Dispose();
            gbuffers.Dispose();
            lightMap.Dispose();
            ssaoBuffer.Dispose();
            fxaaBuffer.Dispose();
            dofBuffer.Dispose();
            autoExposureBuffer.Dispose();
            tonemapBuffer.Dispose();
            ssrBuffer.Dispose();
            depthbuffer.Dispose();
        }

        private void OnResizeEnd(object? sender, ResizedEventArgs e)
        {
            if (!initialized) return;
            windowWidth = e.NewWidth;
            windowHeight = e.NewHeight;
            width = (int)(e.NewWidth * renderResolution);
            height = (int)(e.NewHeight * renderResolution);
            dirty = true;
            gbuffers = new RenderTextureArray(device, width, height, 8, Format.RGBA32Float);
            depthStencil = new DepthBuffer(device, new(width, height, 1, Format.Depth24UNormStencil8, BindFlags.DepthStencil, Usage.Default, CpuAccessFlags.None, DepthStencilViewFlags.None, SampleDescription.Default));
            dsv = depthStencil.DSV;

            depthbuffer = new(device, TextureDescription.CreateTexture2DWithRTV(width, height, 1), DepthStencilDesc.Default);

            ssaoBuffer = new(device, TextureDescription.CreateTexture2DWithRTV(width / 2, height / 2, 1, Format.R32Float));
            ssao.Output = ssaoBuffer.RenderTargetView;
            ssao.Position = gbuffers.SRVs[1];
            ssao.Normal = gbuffers.SRVs[2];
            ssao.Resize(device, width, height);

            lightMap = new(device, TextureDescription.CreateTexture2DWithRTV(width, height, 1));
            light.Output = lightMap.RenderTargetView;
            light.Lights = lightBuffer;
            light.Camera = cameraBuffer.Buffer;
            light.GBuffers = gbuffers.SRVs;
            light.Irraidance = envirr.ResourceView;
            light.EnvPrefiltered = envfilter.ResourceView;
            light.LUT = brdflut.ResourceView;
            light.SSAO = ssaoBuffer.ResourceView;
            light.CSM = csmDepthBuffer.ResourceView;
            light.OSMs = osmDepthBuffers.Select(x => x.ResourceView).ToArray();
            light.PSMs = psmDepthBuffers.Select(x => x.ResourceView).ToArray();
            light.Resize();

            fxaaBuffer = new(device, TextureDescription.CreateTexture2DWithRTV(width, height, 1));
            fxaa.Output = swapChain.BackbufferRTV;
            fxaa.Source = fxaaBuffer.ResourceView;

            tonemapBuffer = new(device, dsv, TextureDescription.CreateTexture2DWithRTV(width, height, 1));
            tonemap.Output = fxaaBuffer.RenderTargetView;
            tonemap.HDR = tonemapBuffer.ResourceView;

            bloom.Source = tonemapBuffer.ResourceView;
            bloom.Resize(device, width, height);
            tonemap.Bloom = bloom.Output;
            tonemap.Resize();

            autoExposureBuffer = new(device, TextureDescription.CreateTexture2DWithRTV(width, height, 1));
            autoExposure.Output = tonemapBuffer.RenderTargetView;
            autoExposure.Color = autoExposureBuffer.ResourceView;
            autoExposure.Resize(width, height);

            dofBuffer = new(device, TextureDescription.CreateTexture2DWithRTV(width, height, 1));
            dof.Target = autoExposureBuffer.RenderTargetView;
            dof.Color = dofBuffer.ResourceView;
            dof.Position = gbuffers.SRVs[1];
            dof.Resize(device, width, height);

            skybox.Output = dofBuffer.RenderTargetView;
            skybox.DSV = dsv;
            skybox.Resize();

            ssrBuffer = new(device, TextureDescription.CreateTexture2DWithRTV(width, height, 1));
            ssr.Resize(device, width, height);
            ssr.Output = dofBuffer.RenderTargetView;
            ssr.Color = lightMap.ResourceView;
            ssr.Position = gbuffers.SRVs[1];
            ssr.Normal = gbuffers.SRVs[2];
            ssr.Depth = depthbuffer.ResourceView;
        }

        public async Task Update(Scene scene)
        {
            if (!initialized) return;
            while (scene.CommandQueue.TryDequeue(out var cmd))
            {
                if (cmd.Sender is SceneNode node)
                {
                    switch (cmd.Type)
                    {
                        case CommandType.Load:
                            for (int i = 0; i < node.Meshes.Count; i++)
                            {
                                await resourceManager.AsyncCreateInstance(scene.Meshes[node.Meshes[i]], node);
                                dirty = true;
                            }
                            break;

                        case CommandType.Unload:
                            for (int i = 0; i < node.Meshes.Count; i++)
                            {
                                await resourceManager.AsyncDestroyInstance(scene.Meshes[node.Meshes[i]], node);
                                dirty = true;
                            }
                            break;

                        case CommandType.Update:
                            if (cmd.Child is int child)
                            {
                                switch (cmd.ChildCommand)
                                {
                                    case ChildCommandType.Added:
                                        await resourceManager.AsyncCreateInstance(scene.Meshes[child], node);
                                        dirty = true;
                                        break;

                                    case ChildCommandType.Removed:
                                        await resourceManager.AsyncDestroyInstance(scene.Meshes[child], node);
                                        dirty = true;
                                        break;
                                }
                            }
                            break;
                    }
                }

                if (cmd.Sender is Mesh mesh)
                {
                    await resourceManager.AsyncUpdateMesh(mesh);
                    dirty = true;
                }

                if (cmd.Sender is Material material)
                {
                    await resourceManager.AsyncUpdateMaterial(material);
                    dirty = true;
                }

                if (cmd.Sender is Light)
                {
                    switch (cmd.Type)
                    {
                        case CommandType.Update:
                            dirty = true;
                            break;
                    }
                }
            }
        }

        public unsafe void Render(IGraphicsContext context, SdlWindow window, Viewport viewport, Scene scene, Camera? camera)
        {
            if (!initialized) return;
            if (camera == null) return;

            cameraBuffer[0] = new CBCamera(camera);
            cameraBuffer.Update(context);
            skyboxBuffer[0] = new CBWorld(Matrix4x4.CreateScale(camera.Transform.Far - 0.1f) * Matrix4x4.CreateTranslation(camera.Transform.Position));
            skyboxBuffer.Update(context);

            context.ClearDepthStencilView(dsv, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
            for (int i = 0; i < gbuffers.Count; i++)
            {
                //context.ClearRenderTargetView(gbuffers.ppRTVs[i], default);
            }

            // Fill Geometry Buffer
            context.ClearRenderTargetViews(gbuffers.RTVs, gbuffers.Count, Vector4.Zero);

            for (int i = 0; i < scene.Meshes.Count; i++)
            {
                if (resourceManager.GetMesh(scene.Meshes[i], out var mesh))
                {
                    mesh.UpdateInstanceBuffer(context);
                    context.SetRenderTargets(gbuffers.RTVs, gbuffers.Count, dsv);
                    mesh.DrawAuto(context, geometry, gbuffers.Viewport);
                }
            }

            // Draw backface depth
            if (ssr.Enabled)
            {
                depthbuffer.ClearTarget(context, Vector4.Zero, DepthStencilClearFlags.Stencil | DepthStencilClearFlags.Depth);
                for (int i = 0; i < scene.Meshes.Count; i++)
                {
                    if (resourceManager.GetMesh(scene.Meshes[i], out var mesh))
                    {
                        depthbuffer.SetTarget(context);
                        mesh.DrawAuto(context, materialDepthBackface, depthbuffer.Viewport);
                    }
                }
            }

            CBLight.Update(lights, scene.Lights);

            uint pointsd = 0;
            uint spotsd = 0;

            // Draw light depth
            for (int i = 0; i < scene.Lights.Count; i++)
            {
                Light light = scene.Lights[i];

                switch (light.Type)
                {
                    case LightType.Directional:
                        if (!light.CastShadows) continue;
                        Matrix4x4* views = (Matrix4x4*)&lights->DirectionalLightSD1;
                        float* cascades = (float*)((byte*)&lights->DirectionalLightSD1 + CBDirectionalLightSD.CascadePointerOffset);
                        var mtxs = CSMHelper.GetLightSpaceMatrices(camera.Transform, light.Transform, views, cascades);
                        context.Write(csmMvpBuffer, mtxs, sizeof(Matrix4x4) * 16);
                        csmDepthBuffer.ClearTarget(context, Vector4.Zero, DepthStencilClearFlags.All);

                        for (int j = 0; j < scene.Meshes.Count; j++)
                        {
                            if (resourceManager.GetMesh(scene.Meshes[j], out var mesh))
                            {
                                context.SetRenderTarget(csmDepthBuffer.RenderTargetView, csmDepthBuffer.DepthStencilView);
                                mesh.DrawAuto(context, csmPipeline, csmDepthBuffer.Viewport);
                            }
                        }

                        break;

                    case LightType.Point:
                        if (!light.CastShadows || !light.Updated)
                        {
                            if (light.CastShadows)
                                pointsd++;
                            continue;
                        }
                        light.Updated = false;
                        var far = ((PointLight)light).ShadowRange;
                        var mt = OSMHelper.GetLightSpaceMatrices(light.Transform, far);
                        context.Write(osmBuffer, mt);
                        context.Write(osmParamBuffer, new Vector4(light.Transform.GlobalPosition, far));
                        osmDepthBuffers[pointsd].ClearTarget(context, Vector4.Zero, DepthStencilClearFlags.All);

                        for (int j = 0; j < scene.Meshes.Count; j++)
                        {
                            if (resourceManager.GetMesh(scene.Meshes[j], out var mesh))
                            {
                                context.SetRenderTarget(osmDepthBuffers[pointsd].RenderTargetView, osmDepthBuffers[pointsd].DepthStencilView);
                                mesh.DrawAuto(context, osmPipeline, osmDepthBuffers[pointsd].Viewport);
                            }
                        }

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
                        CBSpotlightSD* spotlights = (CBSpotlightSD*)(lights + CBLight.SpotlightSDPointerOffset);
                        context.Write(psmBuffer, spotlights[spotsd].View);
                        psmDepthBuffers[spotsd].ClearTarget(context, Vector4.Zero, DepthStencilClearFlags.All);
                        for (int j = 0; j < scene.Meshes.Count; j++)
                        {
                            if (resourceManager.GetMesh(scene.Meshes[j], out var mesh))
                            {
                                context.SetRenderTarget(psmDepthBuffers[spotsd].RenderTargetView, psmDepthBuffers[spotsd].DepthStencilView);
                                mesh.DrawAuto(context, psmPipeline, psmDepthBuffers[spotsd].Viewport);
                            }
                        }

                        spotsd++;
                        break;
                }
            }

            context.Write(lightBuffer, lights, sizeof(CBLight));

            // SSAO Pass
            ssao.Draw(context);

            // Light Pass
            light.Draw(context);

            // Screen Space Reflections
            ssr.Draw(context);

            // Skybox
            skybox.Draw(context);

            // Depth of field
            dof.Draw(context);

            // Eye adaptation
            autoExposure.Draw(context);

            // Bloom
            bloom.Draw(context);

            // Compose and Tonemap
            tonemap.Draw(context);

            // FXAA
            fxaa.Draw(context, viewport);

            context.SetRenderTarget(swapChain.BackbufferRTV, swapChain.BackbufferDSV);
            DebugDraw.Render(camera, viewport);
        }

        private float zoom = 1;

        public void DrawSettings()
        {
            if (!initialized) return;
            var size = ImGui.GetWindowContentRegionMax();
            size.Y = size.X / 16 * 9;
            ImGui.DragFloat("Zoom", ref zoom, 0.01f, 0, 1);
            if (ImGui.CollapsingHeader("Color"))
                ImGui.Image(gbuffers.SRVs[0].NativePointer, size, Vector2.One / 2 - Vector2.One / 2 * zoom, Vector2.One / 2 + Vector2.One / 2 * zoom);

            if (ImGui.CollapsingHeader("Position"))
                ImGui.Image(gbuffers.SRVs[1].NativePointer, size, Vector2.One / 2 - Vector2.One / 2 * zoom, Vector2.One / 2 + Vector2.One / 2 * zoom);

            if (ImGui.CollapsingHeader("Normals"))
                ImGui.Image(gbuffers.SRVs[2].NativePointer, size, Vector2.One / 2 - Vector2.One / 2 * zoom, Vector2.One / 2 + Vector2.One / 2 * zoom);

            /*
            {
                if (ImGui.InputFloat("Render Resolution", ref renderResolution, 0, 0, "%.3f", ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    if (renderResolution == 0)
                    {
                        renderResolution = 1;
                    }
                    configKey.SetValue(nameof(renderResolution), renderResolution);
                    OnResizeBegin(null, EventArgs.Empty);
                    OnResizeEnd(null, new(windowWidth, windowHeight, windowWidth, windowHeight));
                    Config.Global.Save();
                }
                bool vsync = swapChain.VSync;
                if (ImGui.Checkbox("VSync", ref vsync))
                {
                    swapChain.VSync = vsync;
                    configKey.SetValue("VSync", vsync);
                }

                ImGui.BeginDisabled(vsync);

                bool limitFPS = swapChain.LimitFPS;
                if (ImGui.Checkbox("Limit FPS", ref limitFPS))
                {
                    swapChain.LimitFPS = limitFPS;
                    configKey.SetValue("LimitFPS", limitFPS);
                }

                int target = swapChain.TargetFPS;
                if (ImGui.InputInt("FPS Target", ref target, 1, 2, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    swapChain.TargetFPS = target;
                    configKey.SetValue("TargetFPS", target);
                }

                ImGui.EndDisabled();
            }

            if (ImGui.Button("Reload Skybox"))
            {
                env.Dispose();
                envirr.Dispose();
                envfilter.Dispose();
                env = new(device, new TextureFileDescription(Paths.CurrentTexturePath + "env_o.dds", TextureDimension.TextureCube));
                envirr = new(device, new TextureFileDescription(Paths.CurrentTexturePath + "irradiance_o.dds", TextureDimension.TextureCube));
                envfilter = new(device, new TextureFileDescription(Paths.CurrentTexturePath + "prefilter_o.dds", TextureDimension.TextureCube));
                dirty = true;
            }

            if (ImGui.Combo("Shading Model", ref currentShadingModelIndex, availableShadingModelStrings, availableShadingModelStrings.Length))
            {
                currentShadingModel = availableShadingModels[currentShadingModelIndex];
                switch (currentShadingModel)
                {
                    case ShadingModel.Flat:
                        activeShader = flatlightShader;
                        break;

                    case ShadingModel.PbrBrdf:
                        activeShader = pbrlightShader;
                        break;
                }
                dirty = true;
            }*/
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (!initialized) return;
                dsv.Dispose();
                depthStencil.Dispose();
                commandList?.Dispose();
                deferredContext.Dispose();
                resourceManager.Dispose();
                lightBuffer.Dispose();
                cameraBuffer.Dispose();
                skyboxBuffer.Dispose();
                tesselationBuffer.Dispose();
                quad.Dispose();
                geometry.Dispose();
                materialDepthBackface.Dispose();
                materialDepthFrontface.Dispose();
                light.Dispose();
                skybox.Dispose();
                gbuffers.Dispose();

                csmPipeline.Dispose();
                csmMvpBuffer.Dispose();
                csmDepthBuffer.Dispose();

                osmPipeline.Dispose();
                osmBuffer.Dispose();
                osmParamBuffer.Dispose();
                foreach (var buffer in osmDepthBuffers)
                    buffer.Dispose();

                psmPipeline.Dispose();
                psmBuffer.Dispose();
                foreach (var buffer in psmDepthBuffers)
                    buffer.Dispose();

                ssaoBuffer.Dispose();
                lightMap.Dispose();
                fxaaBuffer.Dispose();
                tonemapBuffer.Dispose();
                dofBuffer.Dispose();
                autoExposureBuffer.Dispose();
                ssrBuffer.Dispose();
                depthbuffer.Dispose();

                pointSampler.Dispose();
                anisoSampler.Dispose();
                linearSampler.Dispose();

                ssao.Dispose();
                ssr.Dispose();
                fxaa.Dispose();
                tonemap.Dispose();
                dof.Dispose();
                autoExposure.Dispose();
                bloom.Dispose();

                brdfLUT.Dispose();

                envsmp.Dispose();
                env.Dispose();
                envirr.Dispose();
                envfilter.Dispose();
                brdflut.Dispose();

                disposedValue = true;
            }
        }

        ~DeferredRenderer()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class UniversalDeferredRenderer
    {
    }
}