using HexaEngine.Core.Graphics;

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
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.Scenes.Managers;
    using HexaEngine.Core.Windows;
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.Editor;
    using HexaEngine.Editor.Widgets;
    using HexaEngine.Effects;
    using ImGuiNET;
    using System;
    using System.Numerics;
    using Texture = Texture;

    // TODO: Cleanup and specialization
    public class SceneRenderer : ISceneRenderer
    {
        private ViewportShading shading = Application.InDesignMode ? ViewportShading.Solid : ViewportShading.Rendered;
        private readonly object update = new();
        private readonly object culling = new();
        private readonly object shadows = new();
        private readonly object debug = new();
        private readonly object geometry = new();
#nullable disable
        private bool initialized;
        private bool disposedValue;
        private IGraphicsDevice device;
        private IGraphicsContext context;
        private IGraphicsContext deferredContext;
        private PostProcessManager postProcessing;
        private ISwapChain swapChain;
        private IRenderWindow window;

        private Quad quad;

        private ConstantBuffer<CBCamera> cameraBuffer;
        private ConstantBuffer<CBTessellation> tesselationBuffer;

        private DepthStencil depthStencil;
        private DepthStencil occlusionStencil;
        private ResourceRef<Texture> lightBuffer;
        private IDepthStencilView dsv;
        private TextureArray gbuffer;

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
        private readonly RendererProfiler profiler = new(10);

        public RendererProfiler Profiler => profiler;
        public object Update => update;

        public object Culling => culling;

        public object Geometry => geometry;

        public object SSAO => ssao;

        public object Lights => lights;

        public PostProcessManager PostProcess => postProcessing;

        public object Debug => debug;

        public ViewportShading Shading { get => shading; set => shading = value; }

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
            ResourceManager2.Shared.SetOrAddResource("SwapChain.RTV", swapChain.BackbufferRTV);
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

            gbuffer = new TextureArray(device, width, height, 8, Format.R32G32B32A32Float);
            depthStencil = new(device, width, height, Format.D32FloatS8X24UInt);
            occlusionStencil = new(device, width, height, Format.D32Float);
            dsv = depthStencil.DSV;
            hizBuffer = new(device, width, height);

            ResourceManager2.Shared.AddTextureArray("GBuffer", gbuffer);
            ResourceManager2.Shared.AddShaderResourceView("GBuffer.Color", gbuffer.SRVs[0]);
            ResourceManager2.Shared.AddShaderResourceView("GBuffer.Position", gbuffer.SRVs[1]);
            ResourceManager2.Shared.AddShaderResourceView("GBuffer.Normal", gbuffer.SRVs[2]);
            ResourceManager2.Shared.AddDepthStencilView("SwapChain.DSV", depthStencil.DSV);
#pragma warning disable CS8604 // Possible null reference argument for parameter 'srv' in 'void ResourceManager.AddShaderResourceView(string name, IShaderResourceView srv)'.
            ResourceManager2.Shared.AddShaderResourceView("SwapChain.SRV", depthStencil.SRV);
#pragma warning restore CS8604 // Possible null reference argument for parameter 'srv' in 'void ResourceManager.AddShaderResourceView(string name, IShaderResourceView srv)'.
            lightBuffer = ResourceManager2.Shared.AddTexture("LightBuffer", TextureDescription.CreateTexture2DWithRTV(width, height, 1));

            postProcessing = new(device, width, height);

            postProcessing.Add(new DepthOfField());
            postProcessing.Add(new AutoExposure());
            postProcessing.Add(new Bloom());
            postProcessing.Add(new Tonemap());
            postProcessing.Add(new FXAA());

            ssao = new();

            brdflut = ResourceManager2.Shared.AddTexture("BRDFLUT", TextureDescription.CreateTexture2DWithRTV(512, 512, 1, Format.R32G32B32A32Float)).Value;

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

            deferredContext = device.CreateDeferredContext();

            initialized = true;
            window.Dispatcher.Invoke(() => WidgetManager.Register(new RendererWidget(this)));

            configKey.GenerateSubKeyAuto(ssao, "HBAO");

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
            occlusionStencil.Dispose();
            gbuffer.Dispose();
        }

        private void OnRendererResizeEnd(int width, int height)
        {
            if (!initialized)
            {
                return;
            }

            gbuffer = new TextureArray(device, width, height, 8, Format.R32G32B32A32Float);
            depthStencil = new(device, width, height, Format.D32FloatS8X24UInt);
            occlusionStencil = new(device, width, height, Format.D32Float);
            dsv = depthStencil.DSV;

            lightBuffer = ResourceManager2.Shared.UpdateTexture("LightBuffer", TextureDescription.CreateTexture2DWithRTV(width, height, 1));
            ResourceManager2.Shared.SetOrAddResource("GBuffer", gbuffer);
            ResourceManager2.Shared.SetOrAddResource("GBuffer.Color", gbuffer.SRVs[0]);
            ResourceManager2.Shared.SetOrAddResource("GBuffer.Position", gbuffer.SRVs[1]);
            ResourceManager2.Shared.SetOrAddResource("GBuffer.Normal", gbuffer.SRVs[2]);
            ResourceManager2.Shared.SetOrAddResource("SwapChain.DSV", depthStencil.DSV);

            hizBuffer.Resize(device, width, height);

            postProcessing.EndResize(width, height);

            var scene = SceneManager.Current;
            if (scene == null)
            {
                return;
            }

            scene.Lights.BeginResize();
            scene.Lights.EndResize(width, height);
        }

        public event Action? OnMainPassEnd;

        public unsafe void LoadScene(Scene scene)
        {
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
                windowResized = false;
                ResourceManager2.Shared.SetOrAddResource("SwapChain.RTV", swapChain.BackbufferRTV);
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

#if PROFILE
            profiler.Start(update);
#endif

            cameraBuffer[0] = new CBCamera(camera);
            cameraBuffer.Update(context);

#if PROFILE
            profiler.End(update);
#endif

#if PROFILE
            profiler.Start(culling);
#endif
            CullingManager.UpdateCamera(context);

            scene.RenderManager.Update(context);

            context.ClearDepthStencilView(occlusionStencil.DSV, DepthStencilClearFlags.Depth, 1, 0);
            context.SetRenderTargets(null, 0, occlusionStencil.DSV);
            context.SetViewport(gbuffer.Viewport);
            scene.RenderManager.VisibilityTest(context, RenderQueueIndex.Geometry);
            context.ClearState();
            hizBuffer.Generate(context, occlusionStencil.SRV);
            CullingManager.DoCulling(context, hizBuffer);

#if PROFILE
            profiler.End(culling);
#endif
            context.ClearDepthStencilView(dsv, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
            context.ClearRenderTargetView(lightBuffer.Value.RenderTargetView, default);
#if PROFILE
            profiler.Start(shadows);
#endif
            scene.RenderManager.UpdateShadows(context, camera);
#if PROFILE
            profiler.End(shadows);
#endif

#if PROFILE
            profiler.Start(geometry);
#endif

            var lights = scene.Lights;
            this.lights = lights;
            if (shading == ViewportShading.Rendered)
            {
                // Fill Geometry Buffer
                context.ClearRenderTargetViews(gbuffer.PRTVs, gbuffer.Count, Vector4.Zero);
                context.SetRenderTargets(gbuffer.PRTVs, gbuffer.Count, depthStencil.DSV);
                context.SetViewport(gbuffer.Viewport);
                scene.RenderManager.Draw(context, RenderQueueIndex.Geometry);
                context.ClearState();
            }
#if PROFILE
            profiler.End(geometry);
#endif

#if PROFILE
            profiler.Start(lights);
#endif

            bool skipPost = false;
            // Light Pass
            if (shading == ViewportShading.Rendered)
            {
                lights.Update(context, camera);
                lights.DeferredPass(context, shading, camera);
                lights.ForwardPass(context, shading, camera);
            }
            else
            {
                lights.ForwardPass(context, shading, camera, swapChain.BackbufferRTV, swapChain.BackbufferDSV, viewport);
                skipPost = true;
            }
#if PROFILE
            profiler.End(lights);
#endif

#if PROFILE
            profiler.Start(ssao);
#endif
            // SSAO Pass
            ssao.Draw(context);
#if PROFILE
            profiler.End(ssao);
#endif

            if (shading == ViewportShading.Rendered)
            {
                OnMainPassEnd?.Invoke();
                context.SetRenderTarget(lightBuffer.Value.RenderTargetView, dsv);
                context.SetViewport(lightBuffer.Value.RenderTargetView.Viewport);
                scene.RenderManager.Draw(context, RenderQueueIndex.Background);
            }
            else
            {
                context.SetRenderTarget(swapChain.BackbufferRTV, swapChain.BackbufferDSV);
                context.SetViewport(viewport);
                scene.RenderManager.Draw(context, RenderQueueIndex.Background);
            }

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
        }

        private float zoom = 1;
        private object lights;

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
                occlusionStencil.Dispose();

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