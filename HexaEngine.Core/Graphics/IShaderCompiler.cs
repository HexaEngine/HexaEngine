namespace HexaEngine.Core.Graphics
{
    using System.Diagnostics.CodeAnalysis;

    public interface IShaderCompiler
    {
        bool Compile(string source, ShaderMacro[] macros, string entryPoint, string sourceName, string profile, out Blob? shaderBlob, [NotNullWhen(true)] out string? error);

        unsafe bool Compile(string code, ShaderMacro[] macros, string entry, string sourceName, string profile, Shader** shader, [NotNullWhen(true)] out string? error);

        unsafe bool Compile(string code, string entry, string sourceName, string profile, Shader** shader, [NotNullWhen(true)] out string? error);

        unsafe void CompileFromFile(string path, ShaderMacro[] macros, string entry, string profile, Shader** shader);

        unsafe bool CompileFromFile(string path, ShaderMacro[] macros, string entry, string profile, Shader** shader, [NotNullWhen(true)] out string? error);

        unsafe void CompileFromFile(string path, string entry, string profile, Shader** shader);

        unsafe bool CompileFromFile(string path, string entry, string profile, Shader** shader, [NotNullWhen(true)] out string? error);
    }
}