using HexaEngine;
using HexaEngine.Core;
using HexaEngine.Windows;

Platform.Init(true);
Application.Run(new Window() { Flags = RendererFlags.All, Title = "Editor" });
Platform.Shutdown();