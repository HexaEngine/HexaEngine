namespace VkTesting.Graphics
{
    using HexaEngine.Shaderc;
    using System;
    using System.Text;
    using VkTesting;

    public unsafe struct ShaderBlob
    {
        public ShaderBlob(void* data, uint length)
        {
            Data = data;
            Length = length;
        }

        public void* Data;
        public uint Length;

        public void Release()
        {
            Free(Data);
            Data = null;
            Length = 0;
        }
    }

    internal enum TargetEnvVersion : uint
    {
        Default = 0,  // Default for the corresponding target environment

        // For Vulkan, use numbering scheme from vulkan.h
        Vulkan_1_0 = ((1 << 22)),              // Vulkan 1.0

        Vulkan_1_1 = ((1 << 22) | (1 << 12)),  // Vulkan 1.1
        Vulkan_1_2 = ((1 << 22) | (2 << 12)),  // Vulkan 1.2
        Vulkan_1_3 = ((1 << 22) | (3 << 12)),  // Vulkan 1.2

        // For OpenGL, use the numbering from #version in shaders.
        OpenGL_4_5 = 450,
    };

    public unsafe class ShaderCompiler
    {
        public static ShadercIncludeResult* Include(void* userdata, byte* requestedSource, int type, byte* requestingSource, nuint includeDepth)
        {
            string requested = ToStringFromUTF8(requestedSource) ?? throw new();
            string requesting = ToStringFromUTF8(requestingSource) ?? throw new();
            string baseDir = Path.GetDirectoryName(requesting) ?? throw new();
            string path = Path.Combine(baseDir, requested);

            ShadercIncludeResult* result = Alloc<ShadercIncludeResult>();

            var fs = File.OpenRead(path);
            uint length = (uint)fs.Length;
            byte* buffer = Alloc<byte>(length);
            fs.Read(new Span<byte>(buffer, (int)length));
            fs.Close();
            result->Content = buffer;
            result->ContentLength = length;
            result->SourceName = path.ToUTF8();
            result->SourceNameLength = (nuint)path.Length;
            return result;
        }

        public static void IncludeRelease(void* userdata, ShadercIncludeResult* result)
        {
            Free(result->Content);
            Free(result->SourceName);
            Free(result);
        }

        public static ShaderBlob CompileHLSLFile(string filename, string entrypoint, ShadercShaderKind kind)
        {
            var source = File.ReadAllText(filename);
            return CompileHLSL(source, entrypoint, filename, kind);
        }

        public static ShaderBlob CompileHLSL(string source, string entrypoint, string filename, ShadercShaderKind kind)
        {
            var sourceSize = (nuint)source.Length;
            var compiler = Shaderc.ShadercCompilerInitialize();

            var options = Shaderc.ShadercCompileOptionsInitialize();
            ShadercIncludeResolveFn include = new((nint)(delegate*<void*, byte*, int, byte*, nuint, ShadercIncludeResult*>)&Include);
            ShadercIncludeResultReleaseFn release = new((nint)(delegate*<void*, ShadercIncludeResult*, void>)&IncludeRelease);
            options.SetIncludeCallbacks(include, release, null);
            options.SetOptimizationLevel(ShadercOptimizationLevel.Performance);
            options.SetSourceLanguage(ShadercSourceLanguage.Hlsl);
            options.SetTargetSpirv(ShadercSpirvVersion.Version13);
            options.SetTargetEnv(ShadercTargetEnv.Vulkan, (uint)TargetEnvVersion.Vulkan_1_3);

            ShadercCompilationResult result = Shaderc.ShadercCompileIntoSpv(compiler, source, sourceSize, kind, filename, entrypoint, options);

            var status = result.GetCompilationStatus();
            if (status != ShadercCompilationStatus.Success)
            {
                string? message = result.GetErrorMessageS();
                throw new Exception(message);
            }

            if (Shaderc.ShadercResultGetNumWarnings(result) > 0)
            {
                string message = result.GetErrorMessageS();
                Console.WriteLine(message);
            }

            var length = result.GetLength();
            var bytecode = result.GetBytes();

            var data = Malloc(length);

            Memcpy(bytecode, data, length, length);

            ShaderBlob blob = new(data, (uint)length);

            result.Release();
            options.Release();
            compiler.Release();

            return blob;
        }

        public static ShaderBlob CompileGLSLFile(string filename, string entrypoint, ShadercShaderKind kind)
        {
            var source = File.ReadAllText(filename);
            return CompileGLSL(source, entrypoint, filename, kind);
        }

        public static ShaderBlob CompileGLSL(string source, string entrypoint, string filename, ShadercShaderKind kind)
        {
            var sourceSize = (nuint)source.Length;
            var compiler = Shaderc.ShadercCompilerInitialize();

            var options = Shaderc.ShadercCompileOptionsInitialize();
            ShadercIncludeResolveFn include = new((nint)(delegate*<void*, byte*, int, byte*, nuint, ShadercIncludeResult*>)&Include);
            ShadercIncludeResultReleaseFn release = new((nint)(delegate*<void*, ShadercIncludeResult*, void>)&IncludeRelease);
            options.SetIncludeCallbacks(include, release, null);
            options.SetOptimizationLevel(ShadercOptimizationLevel.Performance);
            options.SetSourceLanguage(ShadercSourceLanguage.Glsl);
            options.SetTargetSpirv(ShadercSpirvVersion.Version13);
            options.SetTargetEnv(ShadercTargetEnv.Vulkan, (uint)TargetEnvVersion.Vulkan_1_3);

            ShadercCompilationResult result = Shaderc.ShadercCompileIntoSpv(compiler, source, sourceSize, kind, filename, entrypoint, options);

            var status = result.GetCompilationStatus();
            if (status != ShadercCompilationStatus.Success)
            {
                string? message = result.GetErrorMessageS();
                throw new Exception(message);
            }

            if (Shaderc.ShadercResultGetNumWarnings(result) > 0)
            {
                string message = result.GetErrorMessageS();
                Console.WriteLine(message);
            }

            var length = result.GetLength();
            var bytecode = result.GetBytes();

            var data = Malloc(length);

            Memcpy(bytecode, data, length, length);

            ShaderBlob blob = new(data, (uint)length);

            result.Release();
            options.Release();
            compiler.Release();

            return blob;
        }
    }
}