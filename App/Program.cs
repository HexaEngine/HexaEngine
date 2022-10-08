namespace App
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Logging;
    using HexaEngine.Editor;
    using HexaEngine.Graphics;
    using HexaEngine.Scenes;
    using Silk.NET.SDL;
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
            ShaderCache.DisableCache = false;

            Config config = new();
            config.Save();

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
            Keyboard.OnKeyUp += (s, e) =>
            {
                if (e.KeyCode == KeyCode.KEscape)
                {
                    window.Close();
                }
                if (e.KeyCode == KeyCode.KF5)
                {
                    SceneManager.Current.Dispatcher.Invoke(() => Pipeline.ReloadShaders());
                }
                if (e.KeyCode == KeyCode.KF10)
                {
                    window.LockCursor = !window.LockCursor;
                }
                if (e.KeyCode == KeyCode.KF11)
                {
                    // TODO: Reimplement BorderlessFullscreen
                    //window.BorderlessFullscreen = !window.BorderlessFullscreen;
                }
                if (e.KeyCode == KeyCode.KF9)
                {
                    SceneManager.Current.Dispatcher.Invoke(() => Designer.InDesignMode = !Designer.InDesignMode);
                }
                if (e.KeyCode == KeyCode.KF1)
                {
                    SceneManager.Current.IsSimulating = false;
                    if (Designer.InDesignMode)
                    {
                        SceneManager.Current?.SaveState();
                        window.LockCursor = true;
                        Designer.InDesignMode = false;
                    }
                    else
                    {
                        SceneManager.Current?.RestoreState();
                        window.LockCursor = false;
                        Designer.InDesignMode = true;
                    }
                    SceneManager.Current.IsSimulating = true;
                    SceneManager.Reload();
                }
            };

            Application.Run(window);
        }
    }
}