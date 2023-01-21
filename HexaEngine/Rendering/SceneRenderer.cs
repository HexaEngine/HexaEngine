using HexaEngine.Core.Graphics;

namespace HexaEngine.Rendering
{
    using HexaEngine.Core;
    using HexaEngine.Core.Events;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.Scenes.Managers;
    using HexaEngine.Core.Windows;
    using HexaEngine.Editor;
    using HexaEngine.Editor.Widgets;
    using HexaEngine.Mathematics;
    using HexaEngine.Pipelines.Deferred;
    using HexaEngine.Pipelines.Effects;
    using HexaEngine.Pipelines.Forward;
    using ImGuiNET;
    using System;
    using System.Numerics;
    using Texture = Texture;

    // TODO: Cleanup and specialization
    public class SceneRenderer : ISceneRenderer
    {
#nullable disable
        private bool initialized;
        private bool dirty = true;
        private bool disposedValue;
        private IGraphicsDevice device;
        private IGraphicsContext context;
        private IGraphicsContext deferredContext;
        private ISwapChain swapChain;
        private bool forwardMode;

        private Quad quad;

        private ConstantBuffer<CBCamera> cameraBuffer;
        private ConstantBuffer<CBLight> lightBuffer;
        private ConstantBuffer<CBWorld> skyboxBuffer;
        private ConstantBuffer<CBTessellation> tesselationBuffer;
        private Geometry geometry;

        private DeferredPrincipledBSDF deferred;
        private ForwardPrincipledBSDF forward;
        private Skybox skybox;

        private DepthBuffer depthStencil;
        private DepthBuffer occlusionStencil;
        private IDepthStencilView dsv;
        private TextureArray gbuffer;

        private DepthMipChain hizBuffer;

        private Texture ssrBuffer;
        private Texture depthbuffer;

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

        private ISamplerState pointSampler;
        private ISamplerState anisoSampler;
        private ISamplerState linearSampler;

        private readonly List<IEffect> effects = new();
        private DepthOfField dof;
        private HBAO ssao;
        private DDASSR ssr;
        private Tonemap tonemap;
        private FXAA fxaa;
        private AutoExposure autoExposure;
        private Bloom bloom;

        private BRDFLUT brdfLUT;

        private ISamplerState envsmp;
        private Texture env;
        private Texture envirr;
        private Texture envfilter;

        private Texture brdflut;

        private ConfigKey configKey;
        private float renderResolution;
        private int width;
        private int height;
        private int rendererWidth;
        private int rendererHeight;
        private int windowWidth;
        private int windowHeight;
        private bool windowResized;
        private bool sceneChanged;
        private bool sceneVariablesChanged;

#nullable enable

        public SceneRenderer()
        {
        }

        /// <summary>
        /// Initializes the renderer.
        /// </summary>
        /// <param dbgName="device">The device.</param>
        /// <param dbgName="window">The window.</param>
        /// <returns></returns>
        public Task Initialize(IGraphicsDevice device, ISwapChain swapChain, IRenderWindow window)
        {
            this.device = device;
            context = device.Context;
            this.swapChain = swapChain ?? throw new NotSupportedException("Device needs a swapchain to operate properly");
            swapChain.Resized += OnWindowResizeEnd;
            ResourceManager.SetOrAddResource("SwapChain.RTV", swapChain.BackbufferRTV);
            SceneManager.SceneChanged += SceneChanged;

            configKey = Config.Global.GetOrCreateKey("Renderer");
            renderResolution = configKey.TryGet(nameof(renderResolution), 1f);

            return
            Task.Factory.StartNew(async () =>
            {
                InitializeSettings();

                quad = new(device);

                pointSampler = ResourceManager.GetOrAddSamplerState("PointClamp", SamplerDescription.PointClamp);
                anisoSampler = ResourceManager.GetOrAddSamplerState("AnisotropicClamp", SamplerDescription.AnisotropicClamp);
                linearSampler = ResourceManager.GetOrAddSamplerState("LinearClamp", SamplerDescription.LinearClamp);

                cameraBuffer = ResourceManager.AddConstantBuffer<CBCamera>("CBCamera", CpuAccessFlags.Write);
                lightBuffer = ResourceManager.AddConstantBuffer<CBLight>("CBLight", CpuAccessFlags.Write);
                skyboxBuffer = new(device, CpuAccessFlags.Write);
                tesselationBuffer = new(device, CpuAccessFlags.Write);

                csmDepthBuffer = new(device, TextureDescription.CreateTexture2DArrayWithRTV(4096, 4096, 4, 1, Format.R32Float), DepthStencilDesc.Default);
                csmMvpBuffer = new(device, 16, CpuAccessFlags.Write);
                csmPipeline = new(device);
                csmPipeline.View = csmMvpBuffer.Buffer;

                osmDepthBuffers = new Texture[8];
                osmSRVs = new IShaderResourceView[8];
                for (int i = 0; i < 8; i++)
                {
                    osmDepthBuffers[i] = new(device, TextureDescription.CreateTextureCubeWithRTV(2048, 1, Format.R32Float), DepthStencilDesc.Default);
                    osmSRVs[i] = osmDepthBuffers[i].ShaderResourceView;
                }

                osmBuffer = new(device, 6, CpuAccessFlags.Write);
                osmParamBuffer = device.CreateBuffer(new Vector4(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
                osmPipeline = new(device);
                osmPipeline.View = osmBuffer;
                osmPipeline.Light = osmParamBuffer;

                psmDepthBuffers = new Texture[8];
                psmSRVs = new IShaderResourceView[8];
                for (int i = 0; i < 8; i++)
                {
                    psmDepthBuffers[i] = new(device, TextureDescription.CreateTexture2DWithRTV(2048, 2048, 1, Format.R32Float), DepthStencilDesc.Default);
                    psmSRVs[i] = psmDepthBuffers[i].ShaderResourceView;
                }

                psmBuffer = device.CreateBuffer(new Matrix4x4(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
                psmPipeline = new(device);
                psmPipeline.View = psmBuffer;

                geometry = new();
                geometry.Camera = cameraBuffer.Buffer;
                effects.Add(geometry);

                gbuffer = new TextureArray(device, width, height, 8, Format.RGBA32Float);
                depthStencil = new(device, width, height, Format.Depth24UNormStencil8);
                occlusionStencil = new(device, width, height, Format.Depth32Float);
                dsv = depthStencil.DSV;
                hizBuffer = new(device, width, height);

                ResourceManager.AddTextureArray("GBuffer", gbuffer);
                ResourceManager.AddShaderResourceView("GBuffer.Color", gbuffer.SRVs[0]);
                ResourceManager.AddShaderResourceView("GBuffer.Position", gbuffer.SRVs[1]);
                ResourceManager.AddShaderResourceView("GBuffer.Normal", gbuffer.SRVs[2]);
                ResourceManager.AddDepthStencilView("SwapChain.DSV", depthStencil.DSV);
                ResourceManager.AddShaderResourceView("SwapChain.SRV", depthStencil.SRV);

                depthbuffer = new(device, TextureDescription.CreateTexture2DWithRTV(width, height, 1, Format.R32Float, Usage.Default, BindFlags.ShaderResource), DepthStencilDesc.Default);

                fxaa = new();
                effects.Add(fxaa);

                bloom = new();
                effects.Add(bloom);

                tonemap = new();
                effects.Add(tonemap);

                autoExposure = new();
                effects.Add(autoExposure);

                dof = new();
                effects.Add(dof);

                ssao = new();
                effects.Add(ssao);

                deferred = new();
                deferred.OSMs = osmDepthBuffers.Select(x => x.ShaderResourceView).ToArray();
                deferred.PSMs = psmDepthBuffers.Select(x => x.ShaderResourceView).ToArray();
                effects.Add(deferred);

                forward = new();
                forward.OSMs = osmDepthBuffers.Select(x => x.ShaderResourceView).ToArray();
                forward.PSMs = psmDepthBuffers.Select(x => x.ShaderResourceView).ToArray();
                effects.Add(forward);

                Vector4 solidColor = new(0.001f, 0.001f, 0.001f, 1);
                Vector4 ambient = new(0.1f, 0.1f, 0.1f, 1);

                env = ResourceManager.AddTextureColor("Environment", TextureDimension.TextureCube, solidColor);
                envirr = ResourceManager.AddTextureColor("EnvironmentIrradiance", TextureDimension.TextureCube, ambient);
                envfilter = ResourceManager.AddTextureColor("EnvironmentPrefilter", TextureDimension.TextureCube, solidColor);

                envsmp = device.CreateSamplerState(SamplerDescription.AnisotropicClamp);

                brdflut = ResourceManager.AddTexture("BRDFLUT", TextureDescription.CreateTexture2DWithRTV(512, 512, 1, Format.RGBA32Float));

                window.Dispatcher.InvokeBlocking(() =>
                {
                    brdfLUT = new();
                    brdfLUT.Initialize(device, 0, 0).Wait();
                    brdfLUT.Target = brdflut.RenderTargetView;
                    brdfLUT.Draw(context);
                    context.ClearState();
                    brdfLUT.Dispose();
                });

                effects.ParallelForEachAsync(x => x.Initialize(device, width, height)).Wait();

                skybox = new(device);
                skybox.Output = await ResourceManager.GetTextureRTVAsync("DepthOfField");
                skybox.DSV = dsv;
                skybox.Env = env.ShaderResourceView;
                skybox.World = skyboxBuffer.Buffer;
                skybox.Camera = cameraBuffer.Buffer;
                skybox.Resize();

                ssrBuffer = new(device, TextureDescription.CreateTexture2DWithRTV(width, height, 1));
                ssr = new(device, width, height);
                ssr.Output = await ResourceManager.GetTextureRTVAsync("DepthOfField");
                ssr.Camera = await ResourceManager.GetConstantBufferAsync("CBCamera");
                ssr.Color = ResourceManager.GetTextureSRV("LightBuffer");
                ssr.Position = ResourceManager.GetTextureSRV("GBuffer.Position");
                ssr.Normal = ResourceManager.GetTextureSRV("GBuffer.Normal");
                ssr.Depth = depthbuffer.ShaderResourceView;

                deferredContext = device.CreateDeferredContext();

                initialized = true;
                window.Dispatcher.Invoke(() => WidgetManager.Register(new RendererWidget(this)));

                configKey.GenerateSubKeyAuto(bloom, "Bloom");
                configKey.GenerateSubKeyAuto(tonemap, "Tonemap");
                configKey.GenerateSubKeyAuto(autoExposure, "AutoExposure");
                configKey.GenerateSubKeyAuto(ssao, "HBAO");
                configKey.GenerateSubKeyAuto(ssr, "SSR");
                configKey.GenerateSubKeyAuto(dof, "Dof");
            });
        }

        private void SceneChanged(object? sender, SceneChangedEventArgs e)
        {
            sceneChanged = true;
        }

        private void VariablesCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
        }

        private void InitializeSettings()
        {
            {
                configKey.TryGetOrAddKeyValue("Width", "1920", DataType.Int32, false, out var val);
                rendererWidth = val.GetInt32();
                val.ValueChanged += (ss, ee) =>
                {
                    rendererWidth = ss.GetInt32();
                    OnRendererResizeBegin();
                    OnRendererResizeEnd(rendererWidth, rendererHeight);
                    Config.Global.Save();
                };
            }
            {
                configKey.TryGetOrAddKeyValue("Height", "1080", DataType.Int32, false, out var val);
                rendererHeight = val.GetInt32();
                val.ValueChanged += (ss, ee) =>
                {
                    rendererHeight = ss.GetInt32();
                    OnRendererResizeBegin();
                    OnRendererResizeEnd(rendererWidth, rendererHeight);
                    Config.Global.Save();
                };
            }
            {
                configKey.TryGetOrAddKeyValue("Forward Rendering", false.ToString(), DataType.Bool, false, out var val);
                forwardMode = val.GetBool();
                val.ValueChanged += (ss, ee) =>
                {
                    forwardMode = ss.GetBool();
                    Config.Global.Save();
                };
            }
            {
                configKey.TryGetOrAddKeyValue("VSync", false.ToString(), DataType.Bool, false, out var val);
                swapChain.VSync = val.GetBool();
                val.ValueChanged += (ss, ee) =>
                {
                    swapChain.VSync = ss.GetBool();
                    Config.Global.Save();
                };
            }
            {
                configKey.TryGetOrAddKeyValue("LimitFPS", false.ToString(), DataType.Bool, false, out var val);
                swapChain.LimitFPS = val.GetBool();
                val.ValueChanged += (ss, ee) =>
                {
                    swapChain.LimitFPS = ss.GetBool();
                    Config.Global.Save();
                };
            }
            {
                configKey.TryGetOrAddKeyValue("TargetFPS", 120.ToString(), DataType.Int32, false, out var val);
                swapChain.TargetFPS = val.GetInt32();
                val.ValueChanged += (ss, ee) =>
                {
                    swapChain.TargetFPS = ss.GetInt32();
                    Config.Global.Save();
                };
            }
            {
                configKey.TryGetOrAddKeyValue("Frustum culling", true.ToString(), DataType.Bool, false, out var val);
                if (val.GetBool())
                    CullingManager.CullingFlags |= CullingFlags.Frustum;
                val.ValueChanged += (ss, ee) =>
                {
                    if (val.GetBool())
                        CullingManager.CullingFlags |= CullingFlags.Frustum;
                    else
                        CullingManager.CullingFlags &= ~CullingFlags.Frustum;
                    Config.Global.Save();
                };
            }
            {
                configKey.TryGetOrAddKeyValue("Occlusion culling", true.ToString(), DataType.Bool, false, out var val);
                if (val.GetBool())
                    CullingManager.CullingFlags |= CullingFlags.Occlusion;
                val.ValueChanged += (ss, ee) =>
                {
                    if (val.GetBool())
                        CullingManager.CullingFlags |= CullingFlags.Frustum;
                    else
                        CullingManager.CullingFlags &= ~CullingFlags.Frustum;
                    Config.Global.Save();
                };
            }

            width = rendererWidth;
            height = rendererHeight;

            Config.Global.Save();
        }

        private void OnWindowResizeBegin()
        {
        }

        private void OnWindowResizeEnd(object? sender, ResizedEventArgs args)
        {
            windowResized = true;
        }

        private void OnRendererResizeBegin()
        {
            if (!initialized) return;
            depthStencil.Dispose();
            occlusionStencil.Dispose();
            gbuffer.Dispose();
            ssrBuffer.Dispose();
            depthbuffer.Dispose();

            ResourceManager.RequireUpdate("SwapChain.RTV");
            ResourceManager.RequireUpdate("GBuffer.Color");
            ResourceManager.RequireUpdate("GBuffer.Position");
            ResourceManager.RequireUpdate("GBuffer.Normal");
            ResourceManager.RequireUpdate("SwapChain.DSV");

            fxaa.BeginResize();
            bloom.BeginResize();
            tonemap.BeginResize();
            autoExposure.BeginResize();
            dof.BeginResize();
            ssao.BeginResize();
            deferred.BeginResize();
            forward.BeginResize();
        }

        private void OnRendererResizeEnd(int width, int height)
        {
            if (!initialized) return;

            dirty = true;
            gbuffer = new TextureArray(device, width, height, 8, Format.RGBA32Float);
            depthStencil = new(device, width, height, Format.Depth24UNormStencil8);
            occlusionStencil = new(device, width, height, Format.Depth32Float);
            dsv = depthStencil.DSV;

            ResourceManager.SetOrAddResource("GBuffer", gbuffer);
            ResourceManager.SetOrAddResource("GBuffer.Color", gbuffer.SRVs[0]);
            ResourceManager.SetOrAddResource("GBuffer.Position", gbuffer.SRVs[1]);
            ResourceManager.SetOrAddResource("GBuffer.Normal", gbuffer.SRVs[2]);
            ResourceManager.SetOrAddResource("SwapChain.DSV", depthStencil.DSV);

            depthbuffer = new(device, TextureDescription.CreateTexture2DWithRTV(width, height, 1, Format.R32Float), DepthStencilDesc.Default);
            hizBuffer.Resize(device, width, height);

            fxaa.EndResize(width, height);
            bloom.EndResize(width, height);
            tonemap.EndResize(width, height);
            autoExposure.EndResize(width, height);
            dof.EndResize(width, height);
            ssao.EndResize(width, height);
            deferred.CSM = csmDepthBuffer.ShaderResourceView;
            deferred.OSMs = osmDepthBuffers.Select(x => x.ShaderResourceView).ToArray();
            deferred.PSMs = psmDepthBuffers.Select(x => x.ShaderResourceView).ToArray();
            deferred.EndResize(width, height);

            forward.Output = ResourceManager.GetTextureRTV("LightBuffer");
            forward.CSM = csmDepthBuffer.ShaderResourceView;
            forward.OSMs = osmDepthBuffers.Select(x => x.ShaderResourceView).ToArray();
            forward.PSMs = psmDepthBuffers.Select(x => x.ShaderResourceView).ToArray();
            forward.EndResize(width, height);

            skybox.Output = ResourceManager.GetTextureRTV("DepthOfField");
            skybox.DSV = dsv;
            skybox.Resize();

            ssrBuffer = new(device, TextureDescription.CreateTexture2DWithRTV(width, height, 1));
            ssr.Resize(device, width, height);
            ssr.Output = ResourceManager.GetTextureRTV("DepthOfField");
            ssr.Camera = cameraBuffer.Buffer;
            ssr.Color = ResourceManager.GetTextureSRV("LightBuffer");
            ssr.Position = gbuffer.SRVs[1];
            ssr.Normal = gbuffer.SRVs[2];
            ssr.Depth = depthbuffer.ShaderResourceView;

            var scene = SceneManager.Current;
            if (scene == null) return;

            scene.LightManager.BeginResize();
            scene.LightManager.EndResize(width, height);
        }

        public unsafe void LoadScene(Scene scene)
        {
            SceneVariables variables = scene.Variables;

            Vector4 solidColor = new(0.001f, 0.001f, 0.001f, 1);

            Vector4 ambient = new(0.1f, 0.1f, 0.1f, 1);

            if (variables.TryGetValue("AmbientLight", out string? vl))
            {
                if (float.TryParse(vl, out float value))
                {
                    ambient = new(value);
                }
            }

            if (variables.TryGetValue("Environment", out string? envp) && FileSystem.Exists(Paths.CurrentTexturePath + envp))
            {
                env = ResourceManager.AddOrUpdateTextureFile("Environment", new TextureFileDescription(Paths.CurrentTexturePath + envp, TextureDimension.TextureCube));
                skybox.Env = env.ShaderResourceView;
            }
            else
            {
                env = ResourceManager.AddOrUpdateTextureColor("Environment", TextureDimension.TextureCube, solidColor);
                skybox.Env = env.ShaderResourceView;
            }

            if (variables.TryGetValue("EnvironmentIrradiance", out string? envirrp) && FileSystem.Exists(Paths.CurrentTexturePath + envirrp))
            {
                envirr = ResourceManager.AddOrUpdateTextureFile("EnvironmentIrradiance", new TextureFileDescription(Paths.CurrentTexturePath + envirrp, TextureDimension.TextureCube));
            }
            else
            {
                envirr = ResourceManager.AddOrUpdateTextureColor("EnvironmentIrradiance", TextureDimension.TextureCube, ambient);
            }

            if (variables.TryGetValue("Environment", out string? envfilterp) && FileSystem.Exists(Paths.CurrentTexturePath + envfilterp))
            {
                envfilter = ResourceManager.AddOrUpdateTextureFile("EnvironmentPrefilter", new TextureFileDescription(Paths.CurrentTexturePath + envfilterp, TextureDimension.TextureCube));
            }
            else
            {
                envfilter = ResourceManager.AddOrUpdateTextureColor("EnvironmentPrefilter", TextureDimension.TextureCube, solidColor);
            }

            deferred.UpdateTextures();
            forward.UpdateTextures();
            scene.LightManager.BeginResize();
            scene.LightManager.EndResize(width, height);
        }

        public unsafe void Render(IGraphicsContext context, IRenderWindow window, Viewport viewport, Scene scene, Camera? camera)
        {
            if (sceneChanged)
            {
                LoadScene(scene);
                sceneChanged = false;
            }

            if (windowResized)
            {
                ResourceManager.RequireUpdate("SwapChain.RTV");
                windowResized = false;
                fxaa.Output = swapChain.BackbufferRTV;
                ResourceManager.SetOrAddResource("SwapChain.RTV", swapChain.BackbufferRTV);
            }

            if (!initialized) return;
            if (camera == null) return;

            var mm = scene.MeshManager;
            var types = scene.InstanceManager.Types;

            // Note the "new" doesn't apply any gc pressure, because the buffer as an array in the background that is already allocated on the unmanaged heap.
            cameraBuffer[0] = new CBCamera(camera);
            cameraBuffer.Update(context);
            skyboxBuffer[0] = new CBWorld(Matrix4x4.CreateScale(camera.Transform.Far - 0.1f) * Matrix4x4.CreateTranslation(camera.Transform.Position));
            skyboxBuffer.Update(context);
            CullingManager.UpdateCamera(context);

            if (CameraManager.Culling != camera)
            {
                context.ClearDepthStencilView(occlusionStencil.DSV, DepthStencilClearFlags.Depth, 1, 0);
                context.SetRenderTargets(null, 0, occlusionStencil.DSV);
                geometry.BeginDrawDepth(context, CullingManager.OcclusionCameraBuffer.Buffer, gbuffer.Viewport);
                for (int i = 0; i < types.Count; i++)
                {
                    var type = types[i];
                    if (type.BeginDraw(context))
                    {
                        context.DrawIndexedInstancedIndirect(type.ArgBuffer, type.ArgBufferOffset);
                    }
                }
                context.ClearState();
                hizBuffer.Generate(context, occlusionStencil.SRV);
                CullingManager.DoCulling(context, hizBuffer);
            }
            else
            {
                hizBuffer.Generate(context, depthStencil.SRV);
                CullingManager.DoCulling(context, hizBuffer);
            }

            context.ClearDepthStencilView(dsv, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);

            if (!forwardMode)
            {
                // Fill Geometry Buffer
                context.ClearRenderTargetViews(gbuffer.PRTVs, gbuffer.Count, Vector4.Zero);
                context.SetRenderTargets(gbuffer.PRTVs, gbuffer.Count, dsv);
                geometry.BeginDraw(context, gbuffer.Viewport);
                for (int i = 0; i < types.Count; i++)
                {
                    var type = types[i];
                    if (type.BeginDraw(context))
                    {
                        context.DrawIndexedInstancedIndirect(type.ArgBuffer, type.ArgBufferOffset);
                    }
                }
                context.ClearState();
            }

            // Draw backface depth
            if (ssr.Enabled)
            {
                depthbuffer.ClearTarget(context, Vector4.Zero, DepthStencilClearFlags.Stencil | DepthStencilClearFlags.Depth);
                depthbuffer.SetTarget(context);
                geometry.BeginDrawDepthBack(context, depthbuffer.Viewport);
                for (int i = 0; i < types.Count; i++)
                {
                    var type = types[i];
                    if (type.BeginDraw(context))
                    {
                        context.DrawIndexedInstancedIndirect(type.ArgBuffer, type.ArgBufferOffset);
                    }
                }
                context.ClearState();
            }

            CBLight.Update(lightBuffer.Local, scene.Lights);

            uint pointsd = 0;
            uint spotsd = 0;

            // Draw light depth
            for (int i = 0; i < scene.Lights.Count; i++)
            {
                Light light = scene.Lights[i];

                switch (light.LightType)
                {
                    case LightType.Directional:
                        if (!light.CastShadows) continue;
                        Matrix4x4* views = (Matrix4x4*)&lightBuffer.Local->DirectionalLightSD1;
                        float* cascades = (float*)((byte*)&lightBuffer.Local->DirectionalLightSD1 + CBDirectionalLightSD.CascadePointerOffset);
                        var mtxs = CSMHelper.GetLightSpaceMatrices(camera.Transform, light.Transform, views, cascades);
                        context.Write(csmMvpBuffer.Buffer, mtxs, sizeof(Matrix4x4) * 16);

                        csmDepthBuffer.ClearTarget(context, Vector4.Zero, DepthStencilClearFlags.All);
                        context.SetRenderTarget(csmDepthBuffer.RenderTargetView, csmDepthBuffer.DepthStencilView);
                        csmPipeline.BeginDraw(context, csmDepthBuffer.Viewport);
                        for (int j = 0; j < types.Count; j++)
                        {
                            var type = types[j];
                            if (type.BeginDrawNoOcculusion(context))
                            {
                                context.DrawIndexedInstanced((uint)type.IndexCount, (uint)type.Visible, 0, 0, 0);
                            }
                        }
                        context.ClearState();

                        break;

                    case LightType.Point:
                        if (!light.CastShadows || !light.Updated)//
                        {
                            if (light.CastShadows)
                                pointsd++;
                            continue;
                        }
                        light.Updated = false;
                        var pointLight = (PointLight)light;
                        OSMHelper.GetLightSpaceMatrices(light.Transform, pointLight.ShadowRange, osmBuffer.Local, pointLight.ShadowBox);
                        osmBuffer.Update(context);
                        context.Write(osmParamBuffer, new Vector4(light.Transform.GlobalPosition, pointLight.ShadowRange));

                        osmDepthBuffers[pointsd].ClearTarget(context, Vector4.Zero, DepthStencilClearFlags.All);
                        context.SetRenderTarget(osmDepthBuffers[pointsd].RenderTargetView, osmDepthBuffers[pointsd].DepthStencilView);
                        osmPipeline.BeginDraw(context, osmDepthBuffers[pointsd].Viewport);

                        for (int j = 0; j < types.Count; j++)
                        {
                            var type = types[j];
                            type.UpdateFrustumInstanceBuffer(*pointLight.ShadowBox);
                            if (type.BeginDrawNoOcculusion(context))
                            {
                                context.DrawIndexedInstanced((uint)type.IndexCount, (uint)type.Visible, 0, 0, 0);
                            }
                        }
                        context.ClearState();

                        pointsd++;
                        break;

                    case LightType.Spot:
                        if (!light.CastShadows)//|| !light.Updated
                        {
                            if (light.CastShadows)
                                spotsd++;
                            continue;
                        }
                        light.Updated = false;
                        CBSpotlightSD* spotlights = lightBuffer.Local->GetSpotlightSDs();
                        context.Write(psmBuffer, spotlights[spotsd].View);

                        psmDepthBuffers[spotsd].ClearTarget(context, Vector4.Zero, DepthStencilClearFlags.All);
                        context.SetRenderTarget(psmDepthBuffers[spotsd].RenderTargetView, psmDepthBuffers[spotsd].DepthStencilView);
                        psmPipeline.BeginDraw(context, psmDepthBuffers[spotsd].Viewport);

                        for (int j = 0; j < types.Count; j++)
                        {
                            var type = types[j];
                            if (type.BeginDrawNoOcculusion(context))
                            {
                                context.DrawIndexedInstanced((uint)type.IndexCount, (uint)type.Visible, 0, 0, 0);
                            }
                        }
                        context.ClearState();

                        spotsd++;
                        break;
                }
            }

            lightBuffer.Update(context);

            // SSAO Pass
            ssao.Draw(context);

            // Light Pass
            if (forwardMode)
            {
                forward.Draw(context);
                for (int i = 0; i < types.Count; i++)
                {
                    var type = types[i];
                    if (type.BeginDraw(context))
                    {
                        context.DrawIndexedInstancedIndirect(type.ArgBuffer, type.ArgBufferOffset);
                    }
                }
                context.ClearState();
            }
            else
            {
                scene.LightManager.DeferredPass(context);
                context.ClearState();
            }

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

            // Fast approximate anti-aliasing
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
                ImGui.Image(gbuffer.SRVs[0].NativePointer, size, Vector2.One / 2 - Vector2.One / 2 * zoom, Vector2.One / 2 + Vector2.One / 2 * zoom);

            if (ImGui.CollapsingHeader("Position"))
                ImGui.Image(gbuffer.SRVs[1].NativePointer, size, Vector2.One / 2 - Vector2.One / 2 * zoom, Vector2.One / 2 + Vector2.One / 2 * zoom);

            if (ImGui.CollapsingHeader("Normals"))
                ImGui.Image(gbuffer.SRVs[2].NativePointer, size, Vector2.One / 2 - Vector2.One / 2 * zoom, Vector2.One / 2 + Vector2.One / 2 * zoom);

            if (ImGui.CollapsingHeader("SSR"))
                ImGui.Image(ssr.ssrSRV.NativePointer, size, Vector2.One / 2 - Vector2.One / 2 * zoom, Vector2.One / 2 + Vector2.One / 2 * zoom);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (!initialized) return;

                dsv.Dispose();
                depthStencil.Dispose();
                occlusionStencil.Dispose();

                hizBuffer.Dispose();
                deferredContext.Dispose();
                lightBuffer.Dispose();
                cameraBuffer.Dispose();
                skyboxBuffer.Dispose();
                tesselationBuffer.Dispose();
                quad.Dispose();
                geometry.Dispose();
                deferred.Dispose();
                forward.Dispose();
                skybox.Dispose();
                gbuffer.Dispose();

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
                ResourceManager.ReleaseShared();
                disposedValue = true;
            }
        }

        ~SceneRenderer()
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
}