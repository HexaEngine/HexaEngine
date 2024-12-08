﻿namespace Editor
{
    using HexaEngine;
    using HexaEngine.Core;
    using HexaEngine.Core.Audio;
    using HexaEngine.Core.Graphics;

    public class Program
    {
        public static void Main(string[] args)
        {
            Application.Boot(GraphicsBackend.D3D11, AudioBackend.OpenAL);
            EditorWindow window = new();

            Platform.Init(window, true);
            Application.Run(window);
            Platform.Shutdown();
        }
    }
}