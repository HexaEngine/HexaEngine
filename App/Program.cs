namespace App
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Editor;
    using HexaEngine.Windows;
    using Window = HexaEngine.Windows.Window;

    public class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        private static void Main()
        {
            CrashLogger.Start();
            Designer.InDesignMode = false;
            Application.Run(new Window() { Flags = RendererFlags.SceneGraph });
        }
    }
}