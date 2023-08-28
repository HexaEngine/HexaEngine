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

    public static class Platform
    {
        private static bool editor;

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

            FileSystem.Initialize();
        }

        public static AppConfig AppConfig { get; private set; }

        public static string StartupScene => AppConfig.StartupScene;

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