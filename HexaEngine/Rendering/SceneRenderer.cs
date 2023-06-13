#nullable disable

namespace HexaEngine.Rendering
{
    using HexaEngine.Core;
    using HexaEngine.Core.Culling;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Core.PostFx;
    using HexaEngine.Core.Renderers;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.Windows;
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.Editor;
    using HexaEngine.Editor.Widgets;
    using HexaEngine.Effects;
    using HexaEngine.Filters;
    using ImGuiNET;
    using System;
    using System.Numerics;

    public class SceneRenderer : ISceneRenderer
    {
        private ViewportShading shading = Application.InDesignMode ? ViewportShading.Solid : ViewportShading.Rendered;

        private bool initialized;
        private bool disposedValue;
        private IGraphicsDevice device;
        private IGraphicsContext context;
        private IGraphicsContext deferredContext;
        private PostProcessingManager postProcessing;
        private ISwapChain swapChain;
        private IRenderWindow window;

        private Quad quad;

        private ConstantBuffer<CBCamera> cameraBuffer;
        private ConstantBuffer<CBTessellation> tesselationBuffer;

        private DepthStencil depthStencil;
        private ResourceRef<Texture> lightBuffer;
        private IDepthStencilView dsv;
        private GBuffer gbuffer;

        private DepthMipChain hizBuffer;

        private HBAO ssao;

        private BRDFLUT brdfLUT;

        private Texture brdflut;

        private ConfigKey configKey;
        private float renderResolution;
        private int width;
        private int height;
        private int rendererWidth;
        private int rendererHeight;
        private bool windowResized;
        private bool sceneChanged;
        private readonly CPUProfiler profiler = new(10);

        public CPUProfiler Profiler => profiler;

        public PostProcessingManager PostProcessing => postProcessing;

        public ViewportShading Shading { get => shading; set => shading = value; }

        public int Width => width;

        public int Height => height;

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
            this.window = window;
            swapChain.Resizing += OnWindowResizeBegin;
            swapChain.Resized += OnWindowResizeEnd;
            ResourceManager2.Shared.SetOrAddResource("SwapChain.RTV", swapChain.BackbufferRTV);
            ResourceManager2.Shared.SetOrAddResource("SwapChain", swapChain.Backbuffer);
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

            cameraBuffer = ResourceManager2.Shared.SetOrAddConstantBuffer<CBCamera>("CBCamera", CpuAccessFlags.Write).Value;
            tesselationBuffer = new(device, CpuAccessFlags.Write);

            gbuffer = new GBuffer(device, width, height,
                Format.R16G16B16A16Float,   // BaseColor(RGB)   Material ID(A)
                Format.R8G8B8A8UNorm,   // Normal(XYZ)      Roughness(W)
                Format.R8G8B8A8UNorm,       // Metallic         Reflectance             AO      Material Data
                Format.R16G16B16A16Float    // Emission(XYZ)    Emission Strength(W)
                );

            depthStencil = new(device, width, height, Format.D32Float);
            dsv = depthStencil.DSV;
            hizBuffer = new(device, width, height);

            ResourceManager2.Shared.AddGBuffer("GBuffer", gbuffer);
            ResourceManager2.Shared.AddShaderResourceView("GBuffer.Color", gbuffer.SRVs[0]);
            ResourceManager2.Shared.AddShaderResourceView("GBuffer.Normal", gbuffer.SRVs[1]);
            ResourceManager2.Shared.AddDepthStencilView("SwapChain.DSV", depthStencil.DSV);
            ResourceManager2.Shared.AddDepthStencilView("PrePass.DSV", depthStencil.DSV);
            ResourceManager2.Shared.AddShaderResourceView("PrePass.SRV", depthStencil.SRV);
            ResourceManager2.Shared.AddShaderResourceView("SwapChain.SRV", depthStencil.SRV);
            lightBuffer = ResourceManager2.Shared.AddTexture("LightBuffer", TextureDescription.CreateTexture2DWithRTV(width, height, 1, Format.R16G16B16A16Float));

            postProcessing = new(device, width, height);

            postProcessing.Add(new VelocityBuffer());
            postProcessing.Add(new SSR());
            postProcessing.Add(new MotionBlur());
            postProcessing.Add(new DepthOfField());
            postProcessing.Add(new AutoExposure());
            postProcessing.Add(new Bloom());
            postProcessing.Add(new LensFlare());
            postProcessing.Add(new GodRays());
            postProcessing.Add(new LUT());
            postProcessing.Add(new Tonemap());
            postProcessing.Add(new TAA());
            postProcessing.Add(new FXAA());

            ssao = new();

            brdflut = ResourceManager2.Shared.AddTexture("BRDFLUT", TextureDescription.CreateTexture2DWithRTV(512, 512, 1, Format.R16G16Float)).Value;

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

            await postProcessing.InitializeAsync(width, height);
            postProcessing.Enabled = true;

            deferredContext = device.CreateDeferredContext();

            initialized = true;
            window.Dispatcher.Invoke(() => WidgetManager.Register(new RendererWidget(this)));

            configKey.GenerateSubKeyAuto(ssao, "HBAO");

            await ssao.Initialize(device, width, height);
#if PROFILE
            device.Profiler.CreateBlock("Update");
            device.Profiler.CreateBlock("PrePass");
            device.Profiler.CreateBlock("ObjectCulling");
            device.Profiler.CreateBlock("LightCulling");
            device.Profiler.CreateBlock("Shadows");
            device.Profiler.CreateBlock("Geometry");
            device.Profiler.CreateBlock("Lights");
            device.Profiler.CreateBlock("SSAO");
            device.Profiler.CreateBlock("PostProcessing");
#endif
        }

        private void OnWindowResizeBegin(object? sender, EventArgs e)
        {
            postProcessing?.BeginResize();
        }

        private void SceneChanged(object? sender, SceneChangedEventArgs e)
        {
            sceneChanged = true;
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
                {
                    CullingManager.CullingFlags |= CullingFlags.Frustum;
                }

                val.ValueChanged += (ss, ee) =>
                {
                    if (val.GetBool())
                    {
                        CullingManager.CullingFlags |= CullingFlags.Frustum;
                    }
                    else
                    {
                        CullingManager.CullingFlags &= ~CullingFlags.Frustum;
                    }

                    Config.Global.Save();
                };
            }
            {
                configKey.TryGetOrAddKeyValue("Occlusion culling", true.ToString(), DataType.Bool, false, out var val);
                if (val.GetBool())
                {
                    CullingManager.CullingFlags |= CullingFlags.Occlusion;
                }

                val.ValueChanged += (ss, ee) =>
                {
                    if (val.GetBool())
                    {
                        CullingManager.CullingFlags |= CullingFlags.Occlusion;
                    }
                    else
                    {
                        CullingManager.CullingFlags &= ~CullingFlags.Occlusion;
                    }

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
            if (!initialized)
            {
                return;
            }

            depthStencil.Dispose();
        }

        private void OnRendererResizeEnd(int width, int height)
        {
            if (!initialized)
            {
                return;
            }

            gbuffer.Resize(width, height);
            depthStencil = new(device, width, height, Format.D32Float);
            dsv = depthStencil.DSV;

            lightBuffer = ResourceManager2.Shared.UpdateTexture("LightBuffer", TextureDescription.CreateTexture2DWithRTV(width, height, 1, Format.R16G16B16A16Float));
            ResourceManager2.Shared.SetOrAddResource("GBuffer.Color", gbuffer.SRVs[0]);
            ResourceManager2.Shared.SetOrAddResource("GBuffer.Position", gbuffer.SRVs[1]);
            ResourceManager2.Shared.SetOrAddResource("GBuffer.Normal", gbuffer.SRVs[2]);
            ResourceManager2.Shared.SetOrAddResource("SwapChain.DSV", depthStencil.DSV);
            ResourceManager2.Shared.SetOrAddResource("PrePass.DSV", depthStencil.DSV);
            ResourceManager2.Shared.SetOrAddResource("PrePass.SRV", depthStencil.SRV);

            hizBuffer.Resize(device, width, height);

            postProcessing.EndResize(width, height);

            var scene = SceneManager.Current;
            if (scene == null)
            {
                return;
            }

            scene.LightManager.BeginResize();
            scene.LightManager.EndResize(width, height);
        }

        public unsafe void LoadScene(Scene scene)
        {
            scene.LightManager.BeginResize();
            scene.LightManager.EndResize(width, height).Wait();
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
                windowResized = false;
                ResourceManager2.Shared.SetOrAddResource("SwapChain.RTV", swapChain.BackbufferRTV);
                ResourceManager2.Shared.SetOrAddResource("SwapChain", swapChain.Backbuffer);
                postProcessing.ResizeOutput();
            }

            postProcessing.SetViewport(viewport);

            if (!initialized)
            {
                return;
            }

            if (camera == null)
            {
                return;
            }

            var lights = scene.LightManager;
            var renderers = scene.RenderManager;

#if PROFILE
            profiler.Begin("Update");
            device.Profiler.Begin(context, "Update");
#endif

            cameraBuffer[0] = new CBCamera(camera, new(width, height), cameraBuffer[0]);
            cameraBuffer.Update(context);

            renderers.Update(context);
            lights.Update(context, camera);
            CullingManager.UpdateCamera(context);

#if PROFILE
            device.Profiler.End(context, "Update");
            profiler.End("Update");
#endif
            if (prepassEnabled)
            {
#if PROFILE
                profiler.Begin("PrePass");
                device.Profiler.Begin(context, "PrePass");
#endif
                context.ClearDepthStencilView(dsv, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
                context.SetRenderTargets(null, 0, dsv);
                context.SetViewport(gbuffer.Viewport);
                renderers.DrawDepth(context, RenderQueueIndex.Geometry | RenderQueueIndex.Transparency);
                context.ClearState();
#if PROFILE
                device.Profiler.End(context, "PrePass");
                profiler.End("PrePass");
#endif
            }
#if PROFILE
            profiler.Begin("ObjectCulling");
            device.Profiler.Begin(context, "ObjectCulling");
#endif

            hizBuffer.Generate(context, depthStencil.SRV);
            CullingManager.DoCulling(context, hizBuffer);

#if PROFILE
            device.Profiler.End(context, "ObjectCulling");
            profiler.End("ObjectCulling");
#endif

#if PROFILE
            profiler.Begin("LightCulling");
            device.Profiler.Begin(context, "LightCulling");
#endif
            lights.CullLights(context);
#if PROFILE
            device.Profiler.End(context, "LightCulling");
            profiler.End("LightCulling");
#endif

            if (!prepassEnabled)
            {
                context.ClearDepthStencilView(dsv, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
            }

            context.ClearRenderTargetView(lightBuffer.Value.RenderTargetView, default);
#if PROFILE
            profiler.Begin("Shadows");
            device.Profiler.Begin(context, "Shadows");
#endif
            renderers.UpdateShadows(context, camera);
#if PROFILE
            device.Profiler.End(context, "Shadows");
            profiler.End("Shadows");
#endif

#if PROFILE
            profiler.Begin("Geometry");
            device.Profiler.Begin(context, "Geometry");
#endif

            // Fill Geometry Buffer
            context.ClearRenderTargetViews(gbuffer.PRTVs, gbuffer.Count, Vector4.Zero);
            context.SetRenderTargets(gbuffer.PRTVs, gbuffer.Count, depthStencil.DSV);
            context.SetViewport(gbuffer.Viewport);
            renderers.Draw(context, RenderQueueIndex.Geometry);
            context.ClearState();

#if PROFILE
            device.Profiler.End(context, "Geometry");
            profiler.End("Geometry");
#endif

#if PROFILE
            profiler.Begin("Lights");
            device.Profiler.Begin(context, "Lights");
#endif

            // Light Pass
            lights.UpdateBuffers(context);

            context.SetRenderTarget(lightBuffer.Value.RenderTargetView, default);
            context.SetViewport(gbuffer.Viewport);

            lights.DeferredPassIndirect(context);
            lights.DeferredPassDirect(context);
            lights.DeferredPassDirectShadows(context);
            var alphaTest = renderers.AlphaTestQueue;
            for (int i = 0; i < alphaTest.Count; i++)
            {
                lights.ForwardPass(context, alphaTest[i], camera);
            }

#if PROFILE
            device.Profiler.End(context, "Lights");
            profiler.End("Lights");
#endif

#if PROFILE
            profiler.Begin("SSAO");
            device.Profiler.Begin(context, "SSAO");
#endif
            // SSAO Pass
            ssao.Draw(context);
#if PROFILE
            device.Profiler.End(context, "SSAO");
            profiler.End("SSAO");
#endif

            context.SetRenderTarget(lightBuffer.Value.RenderTargetView, dsv);
            context.SetViewport(lightBuffer.Value.RenderTargetView.Viewport);
            renderers.Draw(context, RenderQueueIndex.Background);

#if PROFILE
            profiler.Begin("PostProcessing");
            device.Profiler.Begin(context, "PostProcessing");
#endif
            postProcessing.Draw(context);
#if PROFILE
            device.Profiler.End(context, "PostProcessing");
            profiler.End("PostProcessing");
#endif
        }

        private float zoom = 1;
        private bool prepassEnabled = true;

        public void DrawSettings()
        {
            if (!initialized)
            {
                return;
            }

            var size = ImGui.GetWindowContentRegionMax();
            size.Y = size.X / 16 * 9;
            ImGui.DragFloat("Zoom", ref zoom, 0.01f, 0, 1);
            if (ImGui.CollapsingHeader("Color"))
            {
                ImGui.Image(gbuffer.SRVs[0].NativePointer, size, Vector2.One / 2 - Vector2.One / 2 * zoom, Vector2.One / 2 + Vector2.One / 2 * zoom);
            }

            if (ImGui.CollapsingHeader("Position"))
            {
                ImGui.Image(gbuffer.SRVs[1].NativePointer, size, Vector2.One / 2 - Vector2.One / 2 * zoom, Vector2.One / 2 + Vector2.One / 2 * zoom);
            }

            if (ImGui.CollapsingHeader("Normals"))
            {
                ImGui.Image(gbuffer.SRVs[2].NativePointer, size, Vector2.One / 2 - Vector2.One / 2 * zoom, Vector2.One / 2 + Vector2.One / 2 * zoom);
            }

            if (ImGui.CollapsingHeader("SSAO"))
            {
                ImGui.Image(ssao.Output.Value.ShaderResourceView.NativePointer, size, Vector2.One / 2 - Vector2.One / 2 * zoom, Vector2.One / 2 + Vector2.One / 2 * zoom);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (!initialized)
                {
                    return;
                }

                dsv.Dispose();
                depthStencil.Dispose();

                hizBuffer.Dispose();
                deferredContext.Dispose();
                cameraBuffer.Dispose();
                tesselationBuffer.Dispose();
                quad.Dispose();

                gbuffer.Dispose();

                ssao.Dispose();

                postProcessing.Dispose();

                brdflut.Dispose();
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