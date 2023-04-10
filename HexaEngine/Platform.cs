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
            Platform.editor = editor;
            CrashLogger.Initialize();
            DXGIAdapter.Init();
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

#if DEBUG
            Application.GraphicsDebugging = true;
#endif

            FileSystem.Initialize();
        }

        public static AppConfig AppConfig { get; private set; }

        public static string StartupScene => AppConfig.StartupScene;

        public static void Shutdown()
        {
            if (!editor)
                return;
            PluginManager.Unload();
        }
    }
}