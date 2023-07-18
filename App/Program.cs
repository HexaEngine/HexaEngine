namespace App
{
    using HexaEngine;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Windows;

    public static class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        public static void Main()
        {
            Window window = new();
            Platform.Init(window, GraphicsBackend.D3D11);
            Application.Run(window);
            Platform.Shutdown();
        }
    }
}