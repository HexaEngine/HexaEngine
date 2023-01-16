namespace App
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.IO;
    using HexaEngine.D3D11;
    using HexaEngine.Plugins;
    using HexaEngine.Windows;

    public class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        private static unsafe void Main()
        {
            CrashLogger.Initialize();
            DXGIAdapter.Init();
            PluginManager.Load();

            Application.InDesignMode = true;
            Application.InEditorMode = true;
            FileSystem.Initialize();

            Application.Run(new Window() { Flags = RendererFlags.All, Title = "Editor" });

            PluginManager.Unload();
        }
    }
}