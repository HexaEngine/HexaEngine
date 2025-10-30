namespace UIApp
{
    using HexaEngine;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.D3D11;
    using HexaEngine.OpenAL;

    public class Program
    {
        private static void Main(string[] args)
        {
            Application.Boot(GraphicsBackend.D3D11, HexaEngine.Core.Audio.AudioBackend.Auto);
            TestWindow window = new();
            DXGIAdapterD3D11.Init(window, Application.GraphicsDebugging);
            OpenALAdapter.Init();
            Application.Run(window);
        }
    }
}