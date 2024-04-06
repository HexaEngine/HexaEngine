namespace HexaEngine.Vulkan
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Shaderc;
    using Silk.NET.Core.Native;
    using System.Runtime.InteropServices;
    using System.Text;

    public unsafe class ShaderCompiler
    {
        public static bool Compile(string source, ShaderMacro[] macros, string entryPoint, string sourceName, ShadercShaderKind shaderKind, out Blob? shaderBlob, out string? error)
        {
            ShadercCompiler compiler = Shaderc.ShadercCompilerInitialize();

            ShadercSourceLanguage sourceLanguage = ShadercSourceLanguage.Hlsl;

            var sourceSize = (nuint)source.Length;
            byte* pEntrypoint = (byte*)Marshal.StringToCoTaskMemUTF8(entryPoint);
            byte* pFilename = (byte*)Marshal.StringToCoTaskMemUTF8(sourceName);
            byte* pSource = (byte*)Marshal.StringToCoTaskMemUTF8(source);

            ShadercCompileOptions options = Shaderc.ShadercCompileOptionsInitialize();
            options.SetOptimizationLevel(ShadercOptimizationLevel.Performance);
            options.SetSourceLanguage(sourceLanguage);
            options.SetTargetSpirv(ShadercSpirvVersion.Version16);

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

            IncludeHandler handler = new(Path.GetDirectoryName(Path.Combine(Paths.CurrentShaderPath, sourceName)) ?? string.Empty);
            ShadercIncludeResolveFn include = Marshal.GetFunctionPointerForDelegate(handler.Include);
            ShadercIncludeResultReleaseFn release = Marshal.GetFunctionPointerForDelegate(handler.IncludeRelease);
            options.SetIncludeCallbacks(include, release, null);

            ShadercCompilationResult result = Shaderc.ShadercCompileIntoSpv(compiler, pSource, sourceSize, shaderKind, pFilename, pEntrypoint, options);

            Marshal.FreeCoTaskMem((nint)pSource);
            Marshal.FreeCoTaskMem((nint)pEntrypoint);
            Marshal.FreeCoTaskMem((nint)pFilename);

            var status = result.GetCompilationStatus();

            if (status == ShadercCompilationStatus.Success)
            {
                if (result.GetNumWarnings() > 0)
                {
                    error = ToStringFromUTF8(result.GetErrorMessage());
                }
                else
                {
                    error = null;
                }

                var length = result.GetLength();
                var bytecode = result.GetBytes();
                Blob blob = new((byte*)Alloc(length), length);

                Memcpy(bytecode, (void*)blob.BufferPointer, length, length);

                result.Release();
                options.Release();
                compiler.Release();
                shaderBlob = blob;

                return true;
            }

            error = ToStringFromUTF8(result.GetErrorMessage());

            result.Release();
            options.Release();
            compiler.Release();

            shaderBlob = null;
            return false;
        }
    }
}