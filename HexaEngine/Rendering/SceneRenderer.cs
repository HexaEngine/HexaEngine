using HexaEngine.Core.Graphics;

namespace HexaEngine.Rendering
{
    using HexaEngine.Core;
    using HexaEngine.Core.Culling;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Core.PostFx;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.Scenes.Managers;
    using HexaEngine.Core.Windows;
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.Editor;
    using HexaEngine.Editor.Widgets;
    using HexaEngine.Effects;
    using HexaEngine.Pipelines.Deferred;
    using HexaEngine.Pipelines.Forward;
    using ImGuiNET;
    using System;
    using System.Numerics;
    using Texture = Texture;

    // TODO: Cleanup and specialization
    public class SceneRenderer : ISceneRenderer
    {
        private readonly object update = new();
        private readonly object culling = new();
        private readonly object debug = new();
#nullable disable
        private bool initialized;
        private bool disposedValue;
        private IGraphicsDevice device;
        private IGraphicsContext context;
        private IGraphicsContext deferredContext;
        private PostProcessManager postProcessing;
        private ISwapChain swapChain;
        private IRenderWindow window;
        private bool forwardMode;

        private Quad quad;

        private ConstantBuffer<CBCamera> cameraBuffer;
        private ConstantBuffer<CBWorld> skyboxBuffer;
        private ConstantBuffer<CBTessellation> tesselationBuffer;
        private Geometry geometry;

        private Skybox skybox;

        private DepthStencil depthStencil;
        private DepthStencil occlusionStencil;
        private IDepthStencilView dsv;
        private TextureArray gbuffer;

        private DepthMipChain hizBuffer;

#pragma warning disable CS0169 // The field 'SceneRenderer.ssrBuffer' is never used
        private Texture ssrBuffer;
#pragma warning restore CS0169 // The field 'SceneRenderer.ssrBuffer' is never used
        private Texture depthbuffer;

        private ISamplerState pointSampler;
        private ISamplerState anisoSampler;
        private ISamplerState linearSampler;

#pragma warning disable CS0169 // The field 'SceneRenderer.dof' is never used
        private DepthOfField dof;
#pragma warning restore CS0169 // The field 'SceneRenderer.dof' is never used
        private HBAO ssao;
#pragma warning disable CS0649 // Field 'SceneRenderer.ssr' is never assigned to, and will always have its default value null
        private DDASSR ssr;
#pragma warning restore CS0649 // Field 'SceneRenderer.ssr' is never assigned to, and will always have its default value null

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
#pragma warning disable CS0169 // The field 'SceneRenderer.windowWidth' is never used
        private int windowWidth;
#pragma warning restore CS0169 // The field 'SceneRenderer.windowWidth' is never used
#pragma warning disable CS0169 // The field 'SceneRenderer.windowHeight' is never used
        private int windowHeight;
#pragma warning restore CS0169 // The field 'SceneRenderer.windowHeight' is never used
        private bool windowResized;
        private bool sceneChanged;
#pragma warning disable CS0169 // The field 'SceneRenderer.sceneVariablesChanged' is never used
        private bool sceneVariablesChanged;
#pragma warning restore CS0169 // The field 'SceneRenderer.sceneVariablesChanged' is never used
        private readonly RendererProfiler profiler = new(10);

        public RendererProfiler Profiler => profiler;
        public object Update => update;

        public object Culling => culling;

        public object Geometry => geometry;

        public object SSAO => ssao;

        public object Lights => lights;

        public PostProcessManager PostProcess => postProcessing;

        public object Debug => debug;

#nullable enable

#pragma warning disable CS8618 // Non-nullable field 'lights' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.

        public SceneRenderer()
#pragma warning restore CS8618 // Non-nullable field 'lights' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
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
            this.window = window;
            swapChain.Resizing += OnWindowResizeBegin;
            swapChain.Resized += OnWindowResizeEnd;
            ResourceManager.SetOrAddResource("SwapChain.RTV", swapChain.BackbufferRTV);
            SceneManager.SceneChanged += SceneChanged;

            configKey = Config.Global.GetOrCreateKey("Renderer");
            renderResolution = configKey.TryGet(nameof(renderResolution), 1f);

            return
            Task.Factory.StartNew(Initialize);
        }

        private async void Initialize()
        {
            InitializeSettings();

            quad = new(device);

            pointSampler = ResourceManager.GetOrAddSamplerState("PointClamp", SamplerDescription.PointClamp);
            anisoSampler = ResourceManager.GetOrAddSamplerState("AnisotropicClamp", SamplerDescription.AnisotropicClamp);
            linearSampler = ResourceManager.GetOrAddSamplerState("LinearClamp", SamplerDescription.LinearClamp);

            cameraBuffer = ResourceManager.AddConstantBuffer<CBCamera>("CBCamera", CpuAccessFlags.Write);
            skyboxBuffer = new(device, CpuAccessFlags.Write);
            tesselationBuffer = new(device, CpuAccessFlags.Write);

            geometry = new();
            geometry.Camera = cameraBuffer.Buffer;

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
#pragma warning disable CS8604 // Possible null reference argument for parameter 'srv' in 'void ResourceManager.AddShaderResourceView(string name, IShaderResourceView srv)'.
            ResourceManager.AddShaderResourceView("SwapChain.SRV", depthStencil.SRV);
#pragma warning restore CS8604 // Possible null reference argument for parameter 'srv' in 'void ResourceManager.AddShaderResourceView(string name, IShaderResourceView srv)'.
            ResourceManager.AddTextureRTV("LightBuffer", TextureDescription.CreateTexture2DWithRTV(width, height, 1));

            depthbuffer = new(device, TextureDescription.CreateTexture2DWithRTV(width, height, 1, Format.R32Float, Usage.Default, BindFlags.ShaderResource | BindFlags.RenderTarget), DepthStencilDesc.Default);

            postProcessing = new(device, width, height);

            postProcessing.Add(new DepthOfField());

            postProcessing.Add(new AutoExposure());

            postProcessing.Add(new Bloom());

            postProcessing.Add(new Tonemap());

            postProcessing.Add(new FXAA());

            ssao = new();

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
                brdfLUT = null;
            });

            postProcessing.Initialize(width, height);
            postProcessing.Enabled = true;

            skybox = new(device);
            skybox.Output = await ResourceManager.GetTextureRTVAsync("LightBuffer");
            skybox.DSV = dsv;
            skybox.Env = env.ShaderResourceView;
            skybox.World = skyboxBuffer.Buffer;
            skybox.Camera = cameraBuffer.Buffer;
            skybox.Resize();

            deferredContext = device.CreateDeferredContext();

            initialized = true;
            window.Dispatcher.Invoke(() => WidgetManager.Register(new RendererWidget(this)));

            configKey.GenerateSubKeyAuto(ssao, "HBAO");

            await geometry.Initialize(device, width, height);
            await ssao.Initialize(device, width, height);
        }

        private void OnWindowResizeBegin(object? sender, EventArgs e)
        {
            postProcessing?.BeginResize();
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
            depthbuffer.Dispose();

            ResourceManager.RequireUpdate("SwapChain.RTV");
            ResourceManager.RequireUpdate("GBuffer.Color");
            ResourceManager.RequireUpdate("GBuffer.Position");
            ResourceManager.RequireUpdate("GBuffer.Normal");
            ResourceManager.RequireUpdate("SwapChain.DSV");
            ResourceManager.RequireUpdate("LightBuffer");
        }

        private void OnRendererResizeEnd(int width, int height)
        {
            if (!initialized) return;

            gbuffer = new TextureArray(device, width, height, 8, Format.RGBA32Float);
            depthStencil = new(device, width, height, Format.Depth24UNormStencil8);
            occlusionStencil = new(device, width, height, Format.Depth32Float);
            dsv = depthStencil.DSV;

            ResourceManager.UpdateTextureRTV("LightBuffer", TextureDescription.CreateTexture2DWithRTV(width, height, 1));
            ResourceManager.SetOrAddResource("GBuffer", gbuffer);
            ResourceManager.SetOrAddResource("GBuffer.Color", gbuffer.SRVs[0]);
            ResourceManager.SetOrAddResource("GBuffer.Position", gbuffer.SRVs[1]);
            ResourceManager.SetOrAddResource("GBuffer.Normal", gbuffer.SRVs[2]);
            ResourceManager.SetOrAddResource("SwapChain.DSV", depthStencil.DSV);

            depthbuffer = new(device, TextureDescription.CreateTexture2DWithRTV(width, height, 1, Format.R32Float, Usage.Default, BindFlags.ShaderResource), DepthStencilDesc.Default);
            hizBuffer.Resize(device, width, height);

            skybox.Output = ResourceManager.GetTextureRTV("LightBuffer");
            skybox.DSV = dsv;
            skybox.Resize();

            /* ssrBuffer = new(device, TextureDescription.CreateTexture2DWithRTV(width, height, 1));
             ssr.Resize(device, width, height);
             ssr.Output = ResourceManager.GetTextureRTV("DepthOfField");
             ssr.Camera = cameraBuffer.Buffer;
             ssr.Color = ResourceManager.GetTextureSRV("LightBuffer");
             ssr.Position = gbuffer.SRVs[1];
             ssr.Normal = gbuffer.SRVs[2];
             ssr.Backdepth = depthbuffer.ShaderResourceView;*/

            postProcessing.EndResize(width, height);

            var scene = SceneManager.Current;
            if (scene == null) return;

            scene.Lights.BeginResize();
            scene.Lights.EndResize(width, height);
        }

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.

        public unsafe async void LoadScene(Scene scene)
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            SceneVariables variables = scene.Variables;

            Vector4 skyColor = new(0.001f, 0.001f, 0.001f, 1);

            {
                if (variables.TryGetValue("SkyColor", out string? vl))
                {
                    if (float.TryParse(vl, out float value))
                    {
                        skyColor = new(value, value, value, 1);
                    }
                }
            }

            Vector4 ambient = new(0.1f, 0.1f, 0.1f, 1);

            {
                if (variables.TryGetValue("AmbientLight", out string? vl))
                {
                    if (float.TryParse(vl, out float value))
                    {
                        ambient = new(value);
                    }
                }
            }

            if (variables.TryGetValue("Environment", out string? envp) && FileSystem.Exists(Paths.CurrentTexturePath + envp))
            {
                env = ResourceManager.AddOrUpdateTextureFile("Environment", new TextureFileDescription(Paths.CurrentTexturePath + envp, TextureDimension.TextureCube));
                skybox.Env = env.ShaderResourceView;
            }
            else
            {
                env = ResourceManager.AddOrUpdateTextureColor("Environment", TextureDimension.TextureCube, skyColor);
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
                envfilter = ResourceManager.AddOrUpdateTextureColor("EnvironmentPrefilter", TextureDimension.TextureCube, skyColor);
            }

            scene.Lights.BeginResize();
            scene.Lights.EndResize(width, height).Wait();
        }

        public unsafe void Render(IGraphicsContext context, IRenderWindow window, Mathematics.Viewport viewport, Scene scene, Camera? camera)
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
                ResourceManager.SetOrAddResource("SwapChain.RTV", swapChain.BackbufferRTV);
                postProcessing.ResizeOutput();
            }

            postProcessing.SetViewport(viewport);

            if (!initialized) return;
            if (camera == null) return;

#if PROFILE
            profiler.Start(update);
#endif

            var types = scene.InstanceManager.Types;

            // Note the "new" doesn't apply any gc pressure, because the buffer as an array in the background that is already allocated on the unmanaged heap.
            cameraBuffer[0] = new CBCamera(camera);
            cameraBuffer.Update(context);
            skyboxBuffer[0] = new CBWorld(Matrix4x4.CreateScale(camera.Transform.Far - 0.1f) * Matrix4x4.CreateTranslation(camera.Transform.Position));
            skyboxBuffer.Update(context);

#if PROFILE
            profiler.End(update);
#endif

#if PROFILE
            profiler.Start(culling);
#endif
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
#pragma warning disable CS8604 // Possible null reference argument for parameter 'input' in 'void DepthMipChain.Generate(IGraphicsContext context, IShaderResourceView input)'.
                hizBuffer.Generate(context, occlusionStencil.SRV);
#pragma warning restore CS8604 // Possible null reference argument for parameter 'input' in 'void DepthMipChain.Generate(IGraphicsContext context, IShaderResourceView input)'.
                CullingManager.DoCulling(context, hizBuffer);
            }
            else
            {
#pragma warning disable CS8604 // Possible null reference argument for parameter 'input' in 'void DepthMipChain.Generate(IGraphicsContext context, IShaderResourceView input)'.
                hizBuffer.Generate(context, depthStencil.SRV);
#pragma warning restore CS8604 // Possible null reference argument for parameter 'input' in 'void DepthMipChain.Generate(IGraphicsContext context, IShaderResourceView input)'.
                CullingManager.DoCulling(context, hizBuffer);
            }
#if PROFILE
            profiler.End(culling);
#endif

#if PROFILE
            profiler.Start(geometry);
#endif
            context.ClearDepthStencilView(dsv, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
            var lights = scene.Lights;
            this.lights = lights;
            if (lights.Viewport == ViewportShading.Rendered)
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
#if PROFILE
            profiler.End(geometry);
#endif

#if PROFILE
            profiler.Start(ssao);
#endif
            // SSAO Pass
            ssao.Draw(context);
#if PROFILE
            profiler.End(ssao);
#endif

#if PROFILE
            profiler.Start(lights);
#endif
            bool skipPost = false;
            // Light Pass
            if (lights.Viewport == ViewportShading.Rendered)
            {
                lights.Update(context, camera);
                lights.DeferredPass(context, camera);
            }
            else
            {
                lights.ForwardPass(context, camera, swapChain.BackbufferRTV, swapChain.BackbufferDSV, viewport);
                skipPost = true;
            }
#if PROFILE
            profiler.End(lights);
#endif

            skybox.Draw(context);

            if (!skipPost)
            {
#if PROFILE
                profiler.Start(postProcessing);
#endif
                postProcessing.Draw(context);
#if PROFILE
                profiler.End(postProcessing);
#endif
            }

#if PROFILE
            profiler.Start(debug);
#endif
            context.SetRenderTarget(swapChain.BackbufferRTV, swapChain.BackbufferDSV);
            DebugDraw.Render(camera, viewport);
#if PROFILE
            profiler.End(debug);
#endif
        }

        private float zoom = 1;
        private object lights;

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

            if (ImGui.CollapsingHeader("SSAO"))
                ImGui.Image(ssao.OutputView.NativePointer, size, Vector2.One / 2 - Vector2.One / 2 * zoom, Vector2.One / 2 + Vector2.One / 2 * zoom);

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
                cameraBuffer.Dispose();
                skyboxBuffer.Dispose();
                tesselationBuffer.Dispose();
                quad.Dispose();
                geometry.Dispose();
                skybox.Dispose();
                gbuffer.Dispose();

                depthbuffer.Dispose();
                ssao.Dispose();

                pointSampler.Dispose();
                anisoSampler.Dispose();
                linearSampler.Dispose();

                postProcessing.Dispose();

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