namespace Editor
{
    using HexaEngine;
    using HexaEngine.Core;
    using HexaEngine.Core.Audio;
    using HexaEngine.Core.Graphics;
    using System.Runtime.InteropServices;

    public class Program
    {
        public static void Main(string[] args)
        {
            Application.Boot(GraphicsBackend.Vulkan, AudioBackend.OpenAL);
            EditorWindow window = new();
            Platform.Init(window, true);
            Application.Run(window);
            Platform.Shutdown();
        }
    }
}