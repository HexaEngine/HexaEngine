namespace UIApp
{
    using HexaEngine;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;

    public class Program
    {
        private static void Main(string[] args)
        {
            Application.Boot();
            TestWindow window = new();
            Platform.Init(window, GraphicsBackend.D3D11);
            Application.Run(window);
            Platform.Shutdown();
        }
    }
}