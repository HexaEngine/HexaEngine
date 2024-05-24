namespace App
{
    using HexaEngine;
    using HexaEngine.Core;
    using HexaEngine.Core.Audio;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Windows;

    public static class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        public static void Main()
        {
            Application.Boot(GraphicsBackend.D3D11, AudioBackend.Auto);
            Window window = new();
            Platform.Init(window);
            Application.Run(window);
            Platform.Shutdown();
        }
    }
}