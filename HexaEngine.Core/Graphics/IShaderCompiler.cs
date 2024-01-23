namespace HexaEngine.Core.Graphics
{
    using System.Diagnostics.CodeAnalysis;
    using HexaEngine.Core.IO;

    /// <summary>
    /// Represents a compiler for shader programs that can compile shader source code into shader objects.
    /// </summary>
    public interface IShaderCompiler
    {
        /// <summary>
        /// Compiles shader source code into a shader object.
        /// </summary>
        /// <param name="source">The source code of the shader.</param>
        /// <param name="macros">An array of shader macros to use during compilation.</param>
        /// <param name="entryPoint">The entry point for the shader.</param>
        /// <param name="sourceName">The name of the shader source.</param>
        /// <param name="profile">The shader profile to compile for.</param>
        /// <param name="shaderBlob">When the method returns true, contains the compiled shader blob; otherwise, null.</param>
        /// <param name="error">When the method returns false, contains the error message; otherwise, null.</param>
        /// <returns>True if the compilation is successful; otherwise, false.</returns>
        bool Compile(string source, ShaderMacro[] macros, string entryPoint, string sourceName, string profile, out Blob? shaderBlob, [NotNullWhen(true)] out string? error);

        /// <summary>
        /// Compiles shader source code into a shader object.
        /// </summary>
        /// <param name="code">The source code of the shader.</param>
        /// <param name="macros">An array of shader macros to use during compilation.</param>
        /// <param name="entry">The entry point for the shader.</param>
        /// <param name="sourceName">The name of the shader source.</param>
        /// <param name="profile">The shader profile to compile for.</param>
        /// <param name="shader">When the method returns true, contains a pointer to the compiled shader; otherwise, null.</param>
        /// <param name="error">When the method returns false, contains the error message; otherwise, null.</param>
        /// <returns>True if the compilation is successful; otherwise, false.</returns>
        unsafe bool Compile(string code, ShaderMacro[] macros, string entry, string sourceName, string profile, Shader** shader, [NotNullWhen(true)] out string? error);

        /// <summary>
        /// Compiles shader source code into a shader object.
        /// </summary>
        /// <param name="code">The source code of the shader.</param>
        /// <param name="entry">The entry point for the shader.</param>
        /// <param name="sourceName">The name of the shader source.</param>
        /// <param name="profile">The shader profile to compile for.</param>
        /// <param name="shader">When the method returns true, contains a pointer to the compiled shader; otherwise, null.</param>
        /// <param name="error">When the method returns false, contains the error message; otherwise, null.</param>
        /// <returns>True if the compilation is successful; otherwise, false.</returns>
        unsafe bool Compile(string code, string entry, string sourceName, string profile, Shader** shader, [NotNullWhen(true)] out string? error);

        /// <summary>
        /// Compiles shader source code from a file into a shader object.
        /// </summary>
        /// <param name="path">The path to the shader source file.</param>
        /// <param name="macros">An array of shader macros to use during compilation.</param>
        /// <param name="entry">The entry point for the shader.</param>
        /// <param name="profile">The shader profile to compile for.</param>
        /// <param name="shader">When the method returns true, contains a pointer to the compiled shader; otherwise, null.</param>
        unsafe void CompileFromFile(string path, ShaderMacro[] macros, string entry, string profile, Shader** shader);

        /// <summary>
        /// Compiles shader source code from a file into a shader object.
        /// </summary>
        /// <param name="path">The path to the shader source file.</param>
        /// <param name="macros">An array of shader macros to use during compilation.</param>
        /// <param name="entry">The entry point for the shader.</param>
        /// <param name="profile">The shader profile to compile for.</param>
        /// <param name="shader">When the method returns true, contains a pointer to the compiled shader; otherwise, null.</param>
        /// <param name="error">When the method returns false, contains the error message; otherwise, null.</param>
        /// <returns>True if the compilation is successful; otherwise, false.</returns>
        unsafe bool CompileFromFile(string path, ShaderMacro[] macros, string entry, string profile, Shader** shader, [NotNullWhen(true)] out string? error);

        /// <summary>
        /// Compiles shader source code from a file into a shader object.
        /// </summary>
        /// <param name="path">The path to the shader source file.</param>
        /// <param name="entry">The entry point for the shader.</param>
        /// <param name="profile">The shader profile to compile for.</param>
        /// <param name="shader">When the method returns true, contains a pointer to the compiled shader; otherwise, null.</param>
        unsafe void CompileFromFile(string path, string entry, string profile, Shader** shader);

        /// <summary>
        /// Compiles shader source code from a file into a shader object.
        /// </summary>
        /// <param name="path">The path to the shader source file.</param>
        /// <param name="entry">The entry point for the shader.</param>
        /// <param name="profile">The shader profile to compile for.</param>
        /// <param name="shader">When the method returns true, contains a pointer to the compiled shader; otherwise, null.</param>
        /// <param name="error">When the method returns false, contains the error message; otherwise, null.</param>
        /// <returns>True if the compilation is successful; otherwise, false.</returns>
        unsafe bool CompileFromFile(string path, string entry, string profile, Shader** shader, [NotNullWhen(true)] out string? error);
    }
}