using CrashReporter;
using HexaEngine;
using HexaEngine.Core;
using HexaEngine.Core.Graphics;

Application.Boot(GraphicsBackend.D3D11);
CrashWindow window = new();
Platform.Init(window, true);
Application.Run(window);
Platform.Shutdown();