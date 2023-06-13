namespace HexaEngine
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Scripts;
    using HexaEngine.D3D11;
    using HexaEngine.Editor.Plugins;
    using HexaEngine.OpenAL;
    using HexaEngine.Projects;

    public static class Platform
    {
        private static bool editor;

        public static void Init(bool editor = false)
        {
#if DEBUG
            Application.GraphicsDebugging = true;
#endif
            Platform.editor = editor;
            Logger.Initialize();
            DXGIAdapterD3D11.Init(Application.GraphicsDebugging);
            //DXGIAdapterD3D11On12.Init();
            OpenALAdapter.Init();

            if (editor)
            {
                PluginManager.Load();
                Application.InDesignMode = true;
                Application.InEditorMode = true;
            }
            else
            {
                AppConfig = AppConfig.Load();
                AssemblyManager.Load(AppConfig.ScriptAssembly);
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
        }
    }
}