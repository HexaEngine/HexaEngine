namespace HexaEngine.Core.Graphics.Shaders
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.IO;
    using HexaEngine.Shaderc;
    using HexaEngine.SPIRVCross;
    using Silk.NET.Core.Native;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Provides functionality for compiling shaders to SPIR-V bytecode and translating SPIR-V bytecode to source code.
    /// </summary>
    public static unsafe class CrossCompiler
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(CrossCompiler));

        /// <summary>
        /// Gets the source language based on the file extension.
        /// </summary>
        /// <param name="filename">The filename of the shader.</param>
        /// <returns>The source language of the shader.</returns>
        public static SourceLanguage GetSourceLanguage(string filename)
        {
            var ex = Path.GetExtension(filename);
            return ex switch
            {
                ".hlsl" => SourceLanguage.HLSL,
                ".glsl" => SourceLanguage.GLSL,
                _ => throw new NotSupportedException()
            };
        }

        /// <summary>
        /// Callback function used during shader compilation to handle inclusion of additional source files.
        /// </summary>
        /// <param name="userdata">Pointer to user-specific data.</param>
        /// <param name="requestedSource">Pointer to the requested source file name.</param>
        /// <param name="type">Type of the inclusion (e.g., local or system).</param>
        /// <param name="requestingSource">Pointer to the source file making the inclusion request.</param>
        /// <param name="includeDepth">The depth of inclusion (how many times inclusion has occurred).</param>
        /// <returns>A pointer to a structure containing the included source content and its length.</returns>
        /// <remarks>
        /// This function is used to handle the inclusion of additional source files during shader compilation.
        /// </remarks>
        public static ShadercIncludeResult* Include(void* userdata, byte* requestedSource, int type, byte* requestingSource, nuint includeDepth)
        {
            string requested = ToStringFromUTF8(requestedSource) ?? string.Empty;
            string requesting = ToStringFromUTF8(requestingSource) ?? string.Empty;
            string baseDir = ToStringFromUTF8((byte*)userdata) ?? string.Empty;
            string path = Path.Combine(baseDir, requested);

            ShadercIncludeResult* result = AllocT<ShadercIncludeResult>();

            var blob = FileSystem.ReadBlob(path);
            result->Content = blob.Data;
            result->ContentLength = (nuint)blob.Length;
            result->SourceName = path.ToUTF8Ptr();
            result->SourceNameLength = (nuint)path.Length;
            return result;
        }

        /// <summary>
        /// Callback function used to release resources associated with included source files.
        /// </summary>
        /// <param name="userdata">Pointer to user-specific data.</param>
        /// <param name="result">Pointer to the structure containing the included source content.</param>
        /// <remarks>
        /// This function is used to release resources associated with included source files after compilation.
        /// </remarks>
        public static void IncludeRelease(void* userdata, ShadercIncludeResult* result)
        {
            Free(result->Content);
            Free(result->SourceName);
            Free(result);
        }

        private static void ErrorCallback(void* userdata, byte* error)
        {
            var message = ToStringFromUTF8(error);
            Logger.Error(message);
        }

        /// <summary>
        /// Compiles SPIR-V bytecode from shader source code.
        /// </summary>
        /// <param name="source">The source code of the shader.</param>
        /// <param name="filename">The filename of the shader.</param>
        /// <param name="entrypoint">The entry point of the shader.</param>
        /// <param name="macros">An array of shader macros.</param>
        /// <param name="kind">The kind of shader (vertex, fragment, etc.).</param>
        /// <param name="language">The source language of the shader.</param>
        /// <param name="il">The resulting SPIR-V bytecode and its length.</param>
        /// <param name="error">An optional error message if compilation fails.</param>
        /// <returns>True if the compilation is successful; otherwise, false.</returns>
        public static bool CompileSPIRVFromSource(string source, string filename, string entrypoint, ShaderMacro[] macros, ShaderKind kind, SourceLanguage language, out ShaderSpirvIL il, [NotNullWhen(false)] out string? error)
        {
            il = default;
            error = default;

            ShadercSourceLanguage sourceLanguage = language switch
            {
                SourceLanguage.HLSL => ShadercSourceLanguage.Hlsl,
                SourceLanguage.GLSL => ShadercSourceLanguage.Glsl,
                _ => throw new NotImplementedException(),
            };

            ShadercShaderKind shaderKind = kind switch
            {
                ShaderKind.VertexShader => ShadercShaderKind.VertexShader,
                ShaderKind.FragmentShader => ShadercShaderKind.FragmentShader,
                ShaderKind.ComputeShader => ShadercShaderKind.ComputeShader,
                ShaderKind.TaskShader => ShadercShaderKind.TaskShader,
                ShaderKind.MeshShader => ShadercShaderKind.MeshShader,
                ShaderKind.GeometryShader => ShadercShaderKind.GeometryShader,
                ShaderKind.IntersectionShader => ShadercShaderKind.IntersectionShader,
                ShaderKind.MissShader => ShadercShaderKind.MissShader,
                ShaderKind.AnyhitShader => ShadercShaderKind.AnyhitShader,
                ShaderKind.CallableShader => ShadercShaderKind.CallableShader,
                ShaderKind.ClosesthitShader => ShadercShaderKind.ClosesthitShader,
                ShaderKind.RaygenShader => ShadercShaderKind.RaygenShader,
                ShaderKind.SpirvAssembly => ShadercShaderKind.SpirvAssembly,
                ShaderKind.TessControlShader => ShadercShaderKind.TessControlShader,
                ShaderKind.TessEvaluationShader => ShadercShaderKind.TessEvaluationShader,
                _ => throw new NotImplementedException(),
            };

            var sourceSize = (nuint)source.Length;
            byte* pEntrypoint = (byte*)Marshal.StringToCoTaskMemUTF8(entrypoint);
            byte* pFilename = (byte*)Marshal.StringToCoTaskMemUTF8(filename);
            byte* pSource = (byte*)Marshal.StringToCoTaskMemUTF8(source);

            ShadercIncludeResolveFn include = new((nint)(delegate*<void*, byte*, int, byte*, nuint, ShadercIncludeResult*>)&Include);
            ShadercIncludeResultReleaseFn release = new((nint)(delegate*<void*, ShadercIncludeResult*, void>)&IncludeRelease);

            ShadercCompiler compiler = Shaderc.ShadercCompilerInitialize();

            ShadercCompileOptions options = Shaderc.ShadercCompileOptionsInitialize();
            Shaderc.ShadercCompileOptionsSetOptimizationLevel(options, ShadercOptimizationLevel.Performance);
            Shaderc.ShadercCompileOptionsSetSourceLanguage(options, sourceLanguage);

            for (int i = 0; i < macros.Length; i++)
            {
                var pName = macros[i].Name.ToUTF8Ptr();
                var nameLen = (nuint)macros[i].Name.Length;
                var pValue = macros[i].Definition.ToUTF8Ptr();
                var valueLen = (nuint)macros[i].Definition.Length;
                options.AddMacroDefinition(pName, nameLen, pValue, valueLen);
                Free(pName);
                Free(pValue);
            }

            var basePath = Path.GetDirectoryName(Path.Combine(Paths.CurrentShaderPath, filename)) ?? string.Empty;
            byte* pBasePath = basePath.ToUTF8Ptr();
            options.SetIncludeCallbacks(include, release, pBasePath);

            ShadercCompilationResult result = Shaderc.ShadercCompileIntoSpv(compiler, pSource, sourceSize, shaderKind, pFilename, pEntrypoint, options);

            Free(pBasePath);

            Marshal.FreeCoTaskMem((nint)pSource);
            Marshal.FreeCoTaskMem((nint)pEntrypoint);
            Marshal.FreeCoTaskMem((nint)pFilename);

            var status = result.GetCompilationStatus();

            if (status == ShadercCompilationStatus.Success)
            {
                if (Shaderc.ShadercResultGetNumWarnings(result) > 0)
                {
                    error = ToStringFromUTF8(Shaderc.ShadercResultGetErrorMessage(result));
                }

                var length = Shaderc.ShadercResultGetLength(result);
                var bytecode = Shaderc.ShadercResultGetBytes(result);

                il.Bytecode = (byte*)Alloc(length);
                il.Length = (int)length;
                Memcpy(bytecode, il.Bytecode, length, length);

                result.Release();
                options.Release();
                compiler.Release();

                return true;
            }

            error = ToStringFromUTF8(Shaderc.ShadercResultGetErrorMessage(result)) ?? string.Empty;

            result.Release();
            options.Release();
            compiler.Release();

            return false;
        }

        /// <summary>
        /// Compiles SPIR-V bytecode from a shader file.
        /// </summary>
        /// <param name="filename">The filename of the shader file.</param>
        /// <param name="entrypoint">The entry point of the shader.</param>
        /// <param name="kind">The kind of shader (vertex, fragment, etc.).</param>
        /// <param name="language">The source language of the shader.</param>
        /// <param name="il">The resulting SPIR-V bytecode and its length.</param>
        /// <param name="error">An optional error message if compilation fails.</param>
        /// <returns>True if the compilation is successful; otherwise, false.</returns>
        public static bool CompileSPIRVFromFile(string filename, string entrypoint, ShaderKind kind, SourceLanguage language, out ShaderSpirvIL il, [NotNullWhen(false)] out string? error)
        {
            var source = FileSystem.ReadAllText(Paths.CurrentShaderPath + filename);
            return CompileSPIRVFromSource(source, filename, entrypoint, Array.Empty<ShaderMacro>(), kind, language, out il, out error);
        }

        /// <summary>
        /// Compiles SPIRV code from a file with specified parameters.
        /// </summary>
        /// <param name="filename">The name of the file containing the shader source code.</param>
        /// <param name="entrypoint">The entry point function for the shader.</param>
        /// <param name="macros">An array of shader macros to be defined during compilation.</param>
        /// <param name="kind">The kind of shader (e.g., VertexShader, FragmentShader).</param>
        /// <param name="language">The source language of the shader code (e.g., HLSL, GLSL).</param>
        /// <param name="il">The output structure containing the compiled SPIRV bytecode.</param>
        /// <param name="error">If compilation fails, contains an error message; otherwise, null.</param>
        /// <returns>True if the compilation is successful; otherwise, false.</returns>
        public static bool CompileSPIRVFromFile(string filename, string entrypoint, ShaderMacro[] macros, ShaderKind kind, SourceLanguage language, out ShaderSpirvIL il, [NotNullWhen(false)] out string? error)
        {
            var source = FileSystem.ReadAllText(Paths.CurrentShaderPath + filename);
            return CompileSPIRVFromSource(source, filename, entrypoint, macros, kind, language, out il, out error);
        }

        /// <summary>
        /// Checks if the SPIRV result indicates success or failure.
        /// </summary>
        /// <param name="result">The SPIRV result to check.</param>
        /// <param name="file">The file path where the check is performed.</param>
        /// <param name="line">The line number where the check is performed.</param>
        /// <param name="member">The member name where the check is performed.</param>
        /// <returns>True if the result indicates success; otherwise, false.</returns>
        public static bool CheckResult(this SpvcResult result, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
        {
            return result == SpvcResult.Success;
            //throw new SPIRVException($"{file}:{line}, {member}: {result}");
        }

        /// <summary>
        /// Checks if the SPIRV result indicates success or failure.
        /// </summary>
        /// <param name="result">The SPIRV result to check.</param>
        /// <param name="context">The SPIRV context associated with the result.</param>
        /// <param name="file">The file path where the check is performed.</param>
        /// <param name="line">The line number where the check is performed.</param>
        /// <param name="member">The member name where the check is performed.</param>
        /// <returns>True if the result indicates success; otherwise, false.</returns>
        public static bool CheckResult(this SpvcResult result, SpvcContext context, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
        {
            return result == SpvcResult.Success;
            //throw new SPIRVException($"{file}:{line}, {member}: {SPIRV.SpvcContextGetLastErrorStringS(context)}");
        }

        /// <summary>
        /// Compiles SPIR-V bytecode to source code.
        /// </summary>
        /// <param name="il">The SPIR-V bytecode and its length.</param>
        /// <param name="language">The target source language.</param>
        /// <param name="source">The resulting source code.</param>
        /// <returns>True if the compilation is successful; otherwise, false.</returns>
        public static bool CompileSPIRVTo(ShaderSpirvIL il, SourceLanguage language, [NotNullWhen(true)] out string? source)
        {
            source = null;
            uint word_count = (uint)il.Length / 4;

            SpvcContext context = default;
            SpvcParsedIr ir;
            SpvcCompiler compiler;
            SpvcCompilerOptions options;
            SpvcErrorCallback errorCallback = new((nint)(delegate*<void*, byte*, void>)&ErrorCallback);

            // Create context.
            if (!SPIRV.SpvcContextCreate(&context).CheckResult())
            {
                return false;
            }

            // Set debug callback.
            context.SetErrorCallback(errorCallback, null);

            // Parse the SPIR-V.
            if (!context.ParseSpirv((SpvId*)il.Bytecode, word_count, &ir).CheckResult(context))
            {
                context.Destroy();
                return false;
            }

            SpvcBackend backend = language switch
            {
                SourceLanguage.GLSL => SpvcBackend.Glsl,
                SourceLanguage.HLSL => SpvcBackend.Hlsl,
                SourceLanguage.MSL => SpvcBackend.Msl,
                _ => throw new NotImplementedException(),
            };

            if (!context.CreateCompiler(backend, ir, SpvcCaptureMode.TakeOwnership, &compiler).CheckResult(context))
            {
                context.Destroy();
                return false;
            }

            // Modify options.
            if (!compiler.CreateCompilerOptions(&options).CheckResult())
            {
                context.Destroy();
                return false;
            }

            if (!options.SetUint(SpvcCompilerOption.GlslVersion, 450).CheckResult(context))
            {
                context.Destroy();
                return false;
            }
            if (!options.SetBool(SpvcCompilerOption.GlslEs, false).CheckResult(context))
            {
                context.Destroy();
                return false;
            }
            if (!compiler.InstallCompilerOptions(options).CheckResult(context))
            {
                context.Destroy();
                return false;
            }
            compiler.BuildCombinedImageSamplers();
            byte* result = default;
            if (!compiler.Compile(&result).CheckResult(context))
            {
                context.Destroy();
                return false;
            }
            source = ToStringFromUTF8(result) ?? throw new Exception();

            // Frees all memory we allocated so far.
            context.Destroy();

            return true;
        }
    }
}