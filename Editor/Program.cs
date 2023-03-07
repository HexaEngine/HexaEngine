namespace App
{
    using HexaEngine;
    using HexaEngine.Core;
    using HexaEngine.Windows;

    public static class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        private static unsafe void Main()
        {
            Platform.Init(true);
            Application.Run(new Window() { Flags = RendererFlags.All, Title = "Editor" });
            Platform.Shutdown();
        }
    }
}