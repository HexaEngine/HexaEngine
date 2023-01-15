namespace App
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Editor;
    using HexaEngine.IO;
    using HexaEngine.Projects;
    using HexaEngine.Scripting;
    using HexaEngine.Windows;

    public static class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        public static void Main()
        {
            CrashLogger.Initialize();
            AppConfig config = AppConfig.Load();
            Designer.InDesignMode = false;
            Designer.IsShown = false;
            FileSystem.Initialize();
            AssemblyManager.Load(config.ScriptAssembly);
            Application.Run(new Window() { Flags = RendererFlags.SceneGraph, StartupScene = config.StartupScene });
        }
    }
}