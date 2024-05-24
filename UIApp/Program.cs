namespace UIApp
{
    using HexaEngine;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;

    public class Program
    {
        private static void Main(string[] args)
        {
            Application.Boot(GraphicsBackend.D3D11, HexaEngine.Core.Audio.AudioBackend.Auto);
            TestWindow window = new();
            Platform.Init(window);
            Application.Run(window);
            Platform.Shutdown();
        }
    }
}