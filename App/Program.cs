namespace App
{
    using HexaEngine.Core;
    using HexaEngine.Core.Logging;
    using HexaEngine.Scenes;
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
            Trace.Listeners.Add(new DebugListener("output.log"));
            AssimpSceneLoader.Load("untitled.dae");
            Game game = new();
            game.Initialize();
            Application.Run(new GameWindow(game));
            game.Uninitialize();
        }
    }
}