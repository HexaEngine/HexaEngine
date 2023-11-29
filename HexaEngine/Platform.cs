namespace HexaEngine
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Windows;
    using HexaEngine.D3D11;
    using HexaEngine.D3D12;
    using HexaEngine.Editor.Plugins;
    using HexaEngine.OpenAL;
    using HexaEngine.OpenGL;
    using HexaEngine.Projects;
    using HexaEngine.Scripts;
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
        /// <param name="backend">The graphics backend to use.</param>
        /// <param name="editor">Optional parameter to specify whether the application is running in editor mode.</param>
        public static void Init(IWindow window, GraphicsBackend backend, bool editor = false)
        {
            Application.GraphicsBackend = backend;
#if DEBUG
            Application.GraphicsDebugging = true;
#endif
            Platform.editor = editor;

            switch (backend)
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

                    break;

                case GraphicsBackend.OpenGL:
                    OpenGLAdapter.Init(window, Application.GraphicsDebugging);
                    break;

                case GraphicsBackend.Metal:
                    break;
            }

            OpenALAdapter.Init();

            if (editor)
            {
                PluginManager.Load();
                Application.InDesignMode = true;
                Application.InEditorMode = true;
            }
            else if (window is Window window1)
            {
                AppConfig = AppConfig.Load();
                AssemblyManager.Load(AppConfig.ScriptAssembly);
                window1.StartupScene = StartupScene;
            }
        }

        /// <summary>
        /// Gets the application configuration settings.
        /// </summary>
        public static AppConfig? AppConfig { get; private set; }

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

            PluginManager.Unload();
            Logger.Close();
        }
    }
}