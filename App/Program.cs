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
        public static void Main()
        {
            Platform.Init();
            Application.Run(new Window() { StartupScene = Platform.StartupScene });
            Platform.Shutdown();
        }
    }
}