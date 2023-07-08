namespace TestApp
{
    using HexaEngine.Shaderc;
    using HexaEngine.SPIRVCross;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using static Utils;

    public static unsafe partial class Program
    {
        public static ShadercIncludeResult* Include(void* userdata, byte* requestedSource, int type, byte* requestingSource, nuint includeDepth)
        {
            string requested = ToStringFromUTF8(requestedSource);
            string requesting = ToStringFromUTF8(requestingSource);
            string baseDir = Path.GetDirectoryName(requesting);
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

        public static void CompileHLSLToSpv()
        {
            string filename = "assets/shared/shaders/deferred/brdf/direct.hlsl";
            string entrypoint = "main";
            string source = File.ReadAllText(filename);
            var sourceSize = (nuint)source.Length;
            var compiler = Shaderc.ShadercCompilerInitialize();

            var options = Shaderc.ShadercCompileOptionsInitialize();
            ShadercIncludeResolveFn include = new((nint)(delegate*<void*, byte*, int, byte*, nuint, ShadercIncludeResult*>)&Include);
            ShadercIncludeResultReleaseFn release = new((nint)(delegate*<void*, ShadercIncludeResult*, void>)&IncludeRelease);
            options.SetIncludeCallbacks(include, release, null);
            options.SetOptimizationLevel(ShadercOptimizationLevel.Performance);
            options.SetSourceLanguage(ShadercSourceLanguage.Hlsl);
            options.SetAutoCombinedImageSampler(true);

            ShadercCompilationResult result = Shaderc.ShadercCompileIntoSpv(compiler, source, sourceSize, ShadercShaderKind.FragmentShader, filename, entrypoint, options);

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

            var fs = File.OpenWrite("vs.spirv");
            fs.Write(new Span<byte>(bytecode, (int)length));
            fs.Close();

            result.Release();
            options.Release();
            compiler.Release();
        }

        private struct VP
        {
            public int ID;

            public uint Flags;

            public Vector2 Pos;

            public Vector2 Size;

            public Vector2 WorkPos;

            public Vector2 WorkSize;

            public float DpiScale;

            public int ParentViewportId;

            public unsafe void* DrawData;

            public unsafe void* RendererUserData;

            public unsafe void* PlatformUserData;

            public unsafe void* PlatformHandle;

            public unsafe void* PlatformHandleRaw;

            public byte PlatformWindowCreated;

            public byte PlatformRequestMove;

            public byte PlatformRequestResize;

            public byte PlatformRequestClose;
        }

        [UnmanagedCallersOnly(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        private static Vector2 Test(VP* dd)
        {
            return dd->Pos;
        }

        public static void Main()
        {
            delegate* unmanaged[Cdecl]<VP*, Vector2> func = &Test;
            VP* vp = Alloc<VP>();
            Zero(vp);
            vp->ID = 0;
            vp->Flags = 0;
            vp->Pos = new Vector2(0, 0);
            vp->Pos = func(vp);
        }
    }
}