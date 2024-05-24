using CrashReporter;
using HexaEngine;
using HexaEngine.Core;
using HexaEngine.Core.Graphics;

ShaderCache.DisableCache = true;
Application.Boot(GraphicsBackend.D3D11, HexaEngine.Core.Audio.AudioBackend.Auto, true);
CrashWindow window = new();
Platform.Init(window, true);
Application.Run(window);
Platform.Shutdown();