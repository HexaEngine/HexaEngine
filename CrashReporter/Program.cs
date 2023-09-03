using CrashReporter;
using HexaEngine;
using HexaEngine.Core;
using HexaEngine.Core.Graphics;

Application.Boot();
CrashWindow window = new();
Platform.Init(window, GraphicsBackend.D3D11, true);
Application.Run(window);
Platform.Shutdown();