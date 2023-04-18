using HexaEngine;
using HexaEngine.Core;
using HexaEngine.Windows;

public class Program
{
    public static void Main(string[] args)
    {
        Platform.Init(true);
        Application.Run(new Window() { Flags = RendererFlags.All, Title = "Editor" });
        Platform.Shutdown();
    }
}