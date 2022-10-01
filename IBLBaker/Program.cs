#nullable disable

// See https://aka.ms/new-console-template for more information
using HexaEngine.Core;
using HexaEngine.Core.Debugging;
using HexaEngine.Core.Graphics;
using HexaEngine.Graphics;
using IBLBaker;

CrashLogger.Start();

ImGuiConsole.RegisterCommand("recompile_shaders", _ =>
{
    Pipeline.ReloadShaders();
});
ImGuiConsole.RegisterCommand("clear_shader_cache", _ =>
{
    ShaderCache.Clear();
});

Application.Run(new MainWindow());