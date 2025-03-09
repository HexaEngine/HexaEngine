﻿#define PROFILE

namespace HexaEngine.Windows
{
    using Hexa.NET.ImGui;
    using Hexa.NET.Logging;
    using Hexa.NET.Mathematics;
    using Hexa.NET.Utilities.Threading;
    using HexaEngine.Core;
    using HexaEngine.Core.Audio;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Windows;
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Profiling;
    using HexaEngine.Resources;
    using HexaEngine.Resources.Factories;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using Microsoft.Extensions.DependencyInjection;
    using System.Numerics;

    /// <summary>
    /// Represents the application window that implements the <see cref="ICoreWindow"/> interface.
    /// </summary>
    public class Window : CoreWindow
    {
        protected ThreadDispatcher renderDispatcher;
        protected bool running;
        protected bool resetTime;
        protected IAudioDevice audioDevice;
        protected IGraphicsDevice graphicsDevice;
        protected IGraphicsContext graphicsContext;
        protected ISwapChain swapChain;
        protected SceneRenderer sceneRenderer;
        protected ConstantBuffer<CBDisplay> displayBuffer;
        protected Task initTask;
        protected bool rendererInitialized;
        protected bool sizeChanged = false;
        protected bool hdrStateChanged = false;

        protected Thread updateThread;

        protected readonly Barrier syncBarrier = new(2);
        protected readonly AutoResetEvent signal = new(false);

        protected Viewport windowViewport;
        protected Viewport renderViewport;

        /// <summary>
        /// Gets or sets the rendering flags for the window.
        /// </summary>
        public RendererFlags Flags;

        /// <summary>
        /// Gets or sets the startup scene to be loaded.
        /// </summary>
        public string? StartupScene;

        private bool? forceHDR;

        /// <summary>
        /// Gets the scene renderer associated with the window.
        /// </summary>
        public ISceneRenderer Renderer => sceneRenderer;

        /// <summary>
        /// Gets the viewport of the window.
        /// </summary>
        public override sealed Viewport WindowViewport => windowViewport;

        /// <summary>
        /// Gets the viewport used for rendering.
        /// </summary>
        public override sealed Viewport RenderViewport => renderViewport;

        public bool? ForceHDR
        {
            get => forceHDR;
            set
            {
                forceHDR = value;
                hdrStateChanged = true;
            }
        }

#nullable disable

        /// <summary>
        /// Initializes a new instance of the <see cref="Window"/> class.
        /// </summary>
        public Window()
        {
        }

#nullable restore

        /// <summary>
        /// Initializes the window, including setting up rendering resources, audio, and other necessary components.
        /// </summary>
        /// <param name="audioDevice">The audio device to use for audio processing.</param>
        /// <param name="graphicsDevice">The graphics device to use for rendering.</param>
        public override void Initialize(IAudioDevice audioDevice, IGraphicsDevice graphicsDevice)
        {
            base.Initialize(audioDevice, graphicsDevice);
            running = true;
            this.audioDevice = audioDevice;
            this.graphicsDevice = graphicsDevice;
            graphicsContext = graphicsDevice.Context;
            swapChain = SwapChain;
            renderDispatcher = (ThreadDispatcher)Dispatcher;

            displayBuffer = new(CpuAccessFlags.Write);
            graphicsDevice.SetGlobalCBV("DisplayBuffer", displayBuffer);
            UpdateHDRState(graphicsContext);

            if (Application.MainWindow == this)
            {
                // Initialize AudioManager if this is the main window
                AudioManager.Initialize(audioDevice);

                // Setup shared resource manager and its descriptors
                ServiceCollection descriptors = new();
                descriptors.AddSingleton<IResourceFactory, GraphicsPipelineResourceFactory>();
                descriptors.AddSingleton<IResourceFactory, MaterialResourceFactory>();
                descriptors.AddSingleton<IResourceFactory, MaterialShaderResourceFactory>();
                descriptors.AddSingleton<IResourceFactory, MaterialTextureResourceFactory>();
                descriptors.AddSingleton<IResourceFactory, MeshResourceFactory>();

                ResourceManager.Shared = new("Shared", graphicsDevice, audioDevice, descriptors);
                PipelineManager.Initialize(graphicsDevice);
            }

            // Subscribe to the SceneChanged event
            SceneManager.SceneChanged += SceneChanged;

            // Invoke virtual method for additional renderer-specific initialization
            OnRendererInitialize(graphicsDevice);

            // Create and initialize the scene renderer
            sceneRenderer = new(Flags);
            initTask = sceneRenderer.Initialize(graphicsDevice, swapChain, this);
            initTask.ContinueWith(x =>
            {
                if (x.IsCompletedSuccessfully)
                {
                    LoggerFactory.General.Info("Renderer: Initialized");
                }
                if (x.IsFaulted)
                {
                    LoggerFactory.General.Error("Renderer: Failed Initialize");
                    LoggerFactory.General.Log(x.Exception);
                }

                // Set the render viewport based on the initialized scene renderer
                renderViewport = new(sceneRenderer.Width, sceneRenderer.Height);

                rendererInitialized = true;
            });

            // Load the specified startup scene if provided
            if (!string.IsNullOrEmpty(StartupScene))
            {
#nullable disable // Scene cant be null here, unless something bad happend like corrupted files, but I dont care.
                SceneManager.Load(StartupScene, SceneInitFlags.None);
                SceneManager.Current.IsSimulating = true;
#nullable restore
            }

            // Create and start the scene update worker thread
            updateThread = new(UpdateScene);
            updateThread.Name = "Scene Update Worker";
            updateThread.Start();

            Show();
        }

        /// <summary>
        /// Continuously updates the current scene while the application is running.
        /// </summary>
        protected virtual void UpdateScene()
        {
            while (running)
            {
                signal.WaitOne();

                IScene? scene = SceneManager.Current;

                if (scene != null)
                {
                    var profiler = scene.Profiler;

                    profiler.Start(Scene.ProfileObject);

                    // Update the current scene
                    scene.Update();

                    profiler.End(Scene.ProfileObject);

                    profiler.Start(Time.ProfileObject);

                    // Do fixed update tick if necessary.
                    Time.FixedUpdateTick();

                    profiler.End(Time.ProfileObject);
                }

                // Signal and wait for synchronization with the main thread.
                syncBarrier.SignalAndWait();
            }
        }

        /// <summary>
        /// Event handler called when the active scene in the <see cref="SceneManager"/> changes.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments containing information about the scene change.</param>
        private void SceneChanged(object? sender, SceneChangedEventArgs e)
        {
            resetTime = true;
        }

        /// <summary>
        /// Renders the content of the window using the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context.</param>
        public override void Render(IGraphicsContext context)
        {
            signal.Set();
#if PROFILE
            // Begin profiling frame and total time if profiling is enabled.
            graphicsDevice.Profiler.BeginFrame();
            graphicsDevice.Profiler.Begin(context, "Total");
            CPUProfiler.Global.BeginFrame();
            CPUProfiler.Global.Begin("Total");
#endif

            // Resize the swap chain if necessary.
            if (sizeChanged)
            {
                swapChain.Resize(Width, Height);
                sizeChanged = false;
            }

            if (hdrStateChanged)
            {
                UpdateHDRState(context);
                hdrStateChanged = false;
            }

            // Initialize time if requested.
            if (resetTime)
            {
                Time.ResetTime();
                resetTime = false;
            }

            // Clear depth-stencil and render target views.
            context.ClearDepthStencilView(swapChain.BackbufferDSV, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
            context.ClearRenderTargetView(swapChain.BackbufferRTV, Vector4.Zero);

            // Execute rendering commands from the render dispatcher.
            renderDispatcher.ExecuteQueue();

            // Invoke virtual method for pre-render operations.
            OnRenderBegin(context);

            // Determine if rendering should occur based on initialization status.
            var drawing = rendererInitialized;

            // Wait for swap chain presentation.
            //swapChain.WaitForPresent();

            IScene? scene = SceneManager.Current;

            // Render the scene if drawing is enabled.
            if (drawing && scene is not null)
            {
                scene.GraphicsUpdate(context);

                sceneRenderer.OutputViewport = windowViewport;
                sceneRenderer.Render(context, scene, CameraManager.Current);
            }

            // Invoke virtual method for post-render operations.
            OnRender(context);

            // Invoke virtual method for post-render operations.
            OnRenderEnd(context);

            // Present and swap buffers.
            swapChain.Present();

            // Wait for swap chain presentation to complete.
            swapChain.Wait();

            // Signal and wait for synchronization with the update thread.
            syncBarrier.SignalAndWait();

#if PROFILE
            // End profiling frame and total time if profiling is enabled.
            CPUProfiler.Global.End("Total");
            graphicsDevice.Profiler.End(context, "Total");
            graphicsDevice.Profiler.EndFrame(context);
#endif
        }

        protected void UpdateHDRState(IGraphicsContext context)
        {
            var enabled = HDREnabled;
            if (ForceHDR.HasValue)
            {
                enabled = ForceHDR.Value;
            }
            ColorSpace colorSpace = enabled ? ColorSpace.RGBFullG2084NoneP2020 : ColorSpace.RGBFullG22NoneP709;
            swapChain.SetColorSpace(colorSpace);
            displayBuffer.Update(context, new(SDRWhiteLevel, 400, colorSpace));
        }

        /// <summary>
        /// Called when [renderer initialize].
        /// </summary>
        /// <param name="device">The device.</param>
        protected virtual void OnRendererInitialize(IGraphicsDevice device)
        {
        }

        /// <summary>
        /// Called when [render begin].
        /// </summary>
        /// <param name="context">The context.</param>
        [Profile]
        protected virtual void OnRenderBegin(IGraphicsContext context)
        {
        }

        /// <summary>
        /// Called when [render].
        /// </summary>
        /// <param name="context">The context.</param>
        [Profile]
        protected virtual void OnRender(IGraphicsContext context)
        {
        }

        /// <summary>
        /// Called when [render end].
        /// </summary>
        /// <param name="context">The context.</param>
        [Profile]
        protected virtual void OnRenderEnd(IGraphicsContext context)
        {
        }

        /// <summary>
        /// Called when [renderer dispose].
        /// </summary>
        protected virtual void OnRendererDispose()
        {
        }

        /// <summary>
        /// Raises the <see cref="E:HexaEngine.Core.Windows.SdlWindow.Resized" /> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected override void OnResized(ResizedEventArgs args)
        {
            sizeChanged = true;
            base.OnResized(args);
        }

        protected override void OnHDRStateChanged(HDRStateChangedArgs args)
        {
            hdrStateChanged = true;
            base.OnHDRStateChanged(args);
        }

        protected override void DisposeCore()
        {
            // Set the running flag to false.
            running = false;

            // Remove the participant from the synchronization barrier and join the update thread.
            syncBarrier.RemoveParticipant();
            signal.Set();
            updateThread.Join();

            syncBarrier.Dispose();
            signal.Dispose();

            // Invoke virtual method for disposing renderer-specific resources.
            OnRendererDispose();

            // Dispose the profiler associated with the graphics device.
            GraphicsDevice.Profiler.Dispose();

            // Unload the scene manager and wait for initialization task completion if not already completed.
            SceneManager.Unload();
            if (!initTask.IsCompleted)
            {
                initTask.Wait();
            }

            SceneManager.Shutdown();

            // Dispose of the scene renderer, render dispatcher, shared resource manager, audio manager, swap chain,
            // graphics context, and graphics device.
            sceneRenderer.Dispose();
            renderDispatcher.Dispose();
            ResourceManager.Shared.Dispose();
            AudioManager.Release();
        }
    }
}