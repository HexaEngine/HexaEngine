namespace App
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Editor;
    using HexaEngine.Graphics;
    using HexaEngine.IO;
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
            PluginManager.Load();

            Designer.InDesignMode = true;
            FileSystem.Initialize();

            Application.Run(new Window() { Flags = RendererFlags.All });

            PluginManager.Unload();
        }
    }
}