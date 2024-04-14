using CrashReporter;
using HexaEngine;
using HexaEngine.Core;
using HexaEngine.Core.Graphics;

ShaderCache.DisableCache = true;
Application.Boot(GraphicsBackend.D3D11, true);
CrashWindow window = new();
Platform.Init(window, true);
Application.Run(window);
Platform.Shutdown();