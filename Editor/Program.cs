﻿namespace App
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Logging;
    using HexaEngine.Editor;
    using HexaEngine.IO;
    using HexaEngine.OpenAL;
    using HexaEngine.Plugins;
    using System.Diagnostics;
    using Window = HexaEngine.Windows.Window;

    public class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        private static unsafe void Main()
        {
            CrashLogger.Start();
            PluginManager.Load();
            ShaderCache.DisableCache = false;

            Trace.Listeners.Add(new DebugListener("output.log"));

            Designer.InDesignMode = true;

            var window = new Window() { Flags = HexaEngine.Windows.RendererFlags.All };

            ImGuiConsole.RegisterCommand("gc_force", _ =>
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            });

            Application.Run(window);

            PluginManager.Unload();
        }
    }
}