﻿namespace HexaEngine
{
    using Hexa.NET.Logging;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Windows;
    using HexaEngine.D3D11;
    using HexaEngine.D3D12;
    using HexaEngine.Editor.Plugins;
    using HexaEngine.Jobs;
    using HexaEngine.OpenAL;
    using HexaEngine.Scripts;
    using HexaEngine.Vulkan;
    using HexaEngine.Windows;

    /// <summary>
    /// Represents the platform-specific initialization and shutdown procedures.
    /// </summary>
    public static class Platform
    {
        private static bool editor;

        /// <summary>
        /// Initializes the platform-specific components based on the provided window, graphics backend, and optional editor mode.
        /// </summary>
        /// <param name="window">The window interface.</param>
        /// <param name="editor">Optional parameter to specify whether the application is running in editor mode.</param>
        public static void Init(IWindow window, bool editor = false)
        {
            Platform.editor = editor;

            switch (Application.GraphicsBackend)
            {
                case GraphicsBackend.D3D12:
                    DXGIAdapterD3D12.Init(window, Application.GraphicsDebugging);
                    break;

                case GraphicsBackend.D3D11:
                    DXGIAdapterD3D11.Init(window, Application.GraphicsDebugging);
                    break;

                case GraphicsBackend.D3D11On12:
                    DXGIAdapterD3D11On12.Init(window, Application.GraphicsDebugging);
                    break;

                case GraphicsBackend.Vulkan:
                    VulkanAdapter.Init(window, Application.GraphicsDebugging);
                    break;

                case GraphicsBackend.Metal:
                    break;
            }

            OpenALAdapter.Init();

            if (editor)
            {
                PluginManager.Load();
                Application.EditorPlayState = EditorPlayState.Edit;
                Application.InEditorMode = true;
            }
            else if (window is Window window1)
            {
                AppConfig = AppConfig.Load("app.config");
                ScriptAssemblyManager.Load(AppConfig.ScriptAssembly);
                window1.StartupScene = StartupScene;
            }
        }

        /// <summary>
        /// Initializes the platform-specific components in headless mode.
        /// </summary>
        /// <param name="editor">Optional parameter to specify whether the application is running in editor mode.</param>
        public static void Init(bool editor = false)
        {
            Platform.editor = editor;

            if (Application.GraphicsBackend != GraphicsBackend.Disabled)
            {
                throw new InvalidOperationException($"Graphics must be disabled if init in headless mode.");
            }

            if (Application.AudioBackend != Core.Audio.AudioBackend.Disabled)
            {
                throw new InvalidOperationException($"Audio must be disabled if init in headless mode.");
            }

            if (editor)
            {
                PluginManager.Load();
                Application.EditorPlayState = EditorPlayState.Edit;
                Application.InEditorMode = true;
            }
            else
            {
                AppConfig = AppConfig.Load("app.config");
                ScriptAssemblyManager.Load(AppConfig.ScriptAssembly);
            }
        }

        /// <summary>
        /// Gets the application configuration settings.
        /// </summary>
        public static AppConfig? AppConfig { get; set; }

        /// <summary>
        /// Gets the startup scene specified in the application configuration.
        /// </summary>
        public static string? StartupScene => AppConfig?.StartupScene;

        /// <summary>
        /// Shuts down the platform-specific components, especially for editor mode.
        /// </summary>
        public static void Shutdown()
        {
            if (!editor)
            {
                return;
            }

            JobScheduler.Default.Dispose();
            PluginManager.Unload();
            LoggerFactory.CloseAll();
        }
    }
}