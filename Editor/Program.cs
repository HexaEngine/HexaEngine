namespace App
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Logging;
    using HexaEngine.Editor;
    using HexaEngine.Graphics;
    using HexaEngine.Plugins;
    using HexaEngine.Scenes;
    using System.Diagnostics;
    using Window = HexaEngine.Windows.Window;

    public class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        private static void Main()
        {
            CrashLogger.Start();
            PluginManager.Load();
            ShaderCache.DisableCache = false;

            Trace.Listeners.Add(new DebugListener("output.log"));

            Designer.InDesignMode = true;

            var window = new Window() { Flags = HexaEngine.Windows.RendererFlags.All };

            ImGuiConsole.RegisterCommand("recompile_shaders", _ =>
            {
                SceneManager.Current.Dispatcher.Invoke(() => { ShaderCache.Clear(); Pipeline.ReloadShaders(); });
            });
            ImGuiConsole.RegisterCommand("gc_force", _ =>
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            });

            Application.Run(window);

            PluginManager.Unload();
        }
    }
}