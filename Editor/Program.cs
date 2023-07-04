using HexaEngine;
using HexaEngine.Core;
using HexaEngine.Core.Effects;
using HexaEngine.Core.Graphics;
using HexaEngine.Windows;
using System.Runtime.InteropServices;

public class Program
{
    public static void Main(string[] args)
    {
        Window window = new() { Flags = RendererFlags.All, Title = "Editor" };
        Platform.Init(window, GraphicsBackend.D3D11, true);
        Application.Run(window);
        Platform.Shutdown();
    }
}