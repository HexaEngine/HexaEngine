namespace Editor
{
    using HexaEngine;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;

    public class Program
    {
        public static void Main(string[] args)
        {
            Application.Boot();
            EditorWindow window = new();
            Platform.Init(window, GraphicsBackend.D3D11, true);
            Application.Run(window);
            Platform.Shutdown();
        }
    }
}