namespace App
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Logging;
    using HexaEngine.Windows;
    using System.Diagnostics;
    using TestGame;

    public class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        private static void Main()
        {
            CrashLogger.Start();
            ShaderCache.DisableCache = false;
            Trace.Listeners.Add(new DebugListener("output.log"));
            Game game = new();
            game.Initialize();
            Application.Run(new GameWindow(game));
            game.Uninitialize();
        }
    }
}