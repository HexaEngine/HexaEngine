using HexaEngine.Core.Debugging;
using HexaEngine.Core.Graphics;
using HexaEngine.IO;
using System.Runtime.CompilerServices;

namespace HexaEngine.D3D11
{
    public partial class D3D11GraphicsDevice
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<(Blob?, Blob?)> CompileAsync(string code, ShaderMacro[] macros, string entry, string sourceName, string profile)
        {
            var result = await ShaderCompiler.CompileAsync(code, macros, entry, sourceName, profile);
            var errorBlob = result.Item3;

            if (errorBlob != null)
                ImGuiConsole.Log(errorBlob.AsString());
            return (result.Item2, result.Item3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<(Blob?, Blob?)> CompileAsync(string code, string entry, string sourceName, string profile)
        {
            return CompileAsync(code, Array.Empty<ShaderMacro>(), entry, sourceName, profile);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<(Blob?, Blob?)> CompileFromFileAsync(string path, ShaderMacro[] macros, string entry, string profile)
        {
            var result = await ShaderCompiler.CompileAsync(FileSystem.ReadAllText(Paths.CurrentShaderPath + path), macros, entry, path, profile);
            var errorBlob = result.Item3;

            if (errorBlob != null)
                ImGuiConsole.Log(errorBlob.AsString());
            return (result.Item2, result.Item3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<(Blob?, Blob?)> CompileFromFileAsync(string path, string entry, string profile)
        {
            return CompileFromFileAsync(path, Array.Empty<ShaderMacro>(), entry, profile);
        }
    }
}