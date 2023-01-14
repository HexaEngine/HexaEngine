namespace App
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Editor;
    using HexaEngine.Plugins;
    using Window = HexaEngine.Windows.Window;

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

            var window = new Window() { Flags = HexaEngine.Windows.RendererFlags.All };

            Application.Run(window);

            PluginManager.Unload();
        }
    }
}