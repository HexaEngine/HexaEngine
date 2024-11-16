namespace HexaEngine.D3D12
{
    using Hexa.NET.DXC;
    using Hexa.NET.Logging;
    using Hexa.NET.Utilities;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaGen.Runtime;
    using System;
    using System.Runtime.InteropServices;
    using Buffer = Hexa.NET.DXC.Buffer;

    public unsafe class ShaderCompiler
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(ShaderCompiler));

        private static Guid CLSID_DxcUtils = new(0x6245d6af, 0x66e0, 0x48fd, 0x80, 0xb4, 0x4d, 0x27, 0x17, 0x96, 0x74, 0x8c);
        private static Guid CLSID_DxcCompiler = new(0x73e22d93, 0xe6ce, 0x47f3, 0xb5, 0xbf, 0xf0, 0x66, 0x4f, 0x39, 0xc1, 0xb0);

        public bool Compile(string source, ShaderMacro[] macros, string entryPoint, string sourceName, string profile, out ComPtr<IDxcResult>? shaderBlob, out string? error)
        {
            Logger.Info($"Compiling: {sourceName}");
            shaderBlob = null;
            error = null;
            ComPtr<IDxcUtils> utils = default;
            DXC.CreateInstance(ref CLSID_DxcUtils, ComUtils.GuidPtrOf<IDxcUtils>(), (void**)utils.GetAddressOf()).ThrowIf();
            ComPtr<IDxcCompiler3> compiler = default;
            DXC.CreateInstance(ref CLSID_DxcCompiler, ComUtils.GuidPtrOf<IDxcCompiler3>(), (void**)compiler.GetAddressOf()).ThrowIf();

            StdString str = new(source);
            ComPtr<IDxcBlobEncoding> pSource = default;
            utils.CreateBlob(str.Data, (uint)str.Size, DXC.DXC_CP_UTF8, out pSource).ThrowIf();
            str.Release();

            UnsafeList<StdWString> stringArgs = [];

            // -E for the entry point (eg. 'main')
            stringArgs.PushBack("-E");
            stringArgs.PushBack(entryPoint);

            // -T for the target profile (eg. 'ps_6_6')
            stringArgs.PushBack("-T");
            stringArgs.PushBack(profile);

            // Strip reflection data and pdbs (see later)
            stringArgs.PushBack("-Qstrip_debug");
            stringArgs.PushBack("-Qstrip_reflect");

#if DEBUG && !RELEASE && !SHADER_FORCE_OPTIMIZE
            stringArgs.PushBack(DXC.DXC_ARG_DEBUG);
            stringArgs.PushBack(DXC.DXC_ARG_SKIP_OPTIMIZATIONS);
            stringArgs.PushBack(DXC.DXC_ARG_DEBUG_NAME_FOR_SOURCE);
#else
            stringArgs.PushBack(DXC.DXC_ARG_OPTIMIZATION_LEVEL3);
#endif

            UnsafeList<Hexa.NET.Utilities.Pointer<char>> arguments = [];
            for (int i = 0; i < stringArgs.Count; i++)
            {
                arguments.PushBack(stringArgs[i].Data);
            }

            Buffer sourceBuffer;
            sourceBuffer.Ptr = pSource.GetBufferPointer();
            sourceBuffer.Size = pSource.GetBufferSize();
            sourceBuffer.Encoding = 0;

            IncludeHandler handler = new(Path.GetDirectoryName(Path.Combine(Paths.CurrentShaderPath, sourceName)) ?? string.Empty);
            ComPtr<IDxcIncludeHandler> pInclude;
            IDxcIncludeHandler* include = (IDxcIncludeHandler*)Alloc(sizeof(IDxcIncludeHandler) + sizeof(nint));
            pInclude.Handle = include;
            include->LpVtbl = (void**)Alloc(sizeof(nint) * 4);
            include->LpVtbl[0] = (void*)Marshal.GetFunctionPointerForDelegate(handler.QueryInterface);
            include->LpVtbl[1] = (void*)Marshal.GetFunctionPointerForDelegate(handler.AddRef);
            include->LpVtbl[2] = (void*)Marshal.GetFunctionPointerForDelegate(handler.Release);
            include->LpVtbl[3] = (void*)Marshal.GetFunctionPointerForDelegate(handler.LoadSource);

            ComPtr<IDxcResult> pCompileResult = default;
            compiler.Compile(&sourceBuffer, (char**)arguments.Data, (uint)arguments.Size, pInclude, ComUtils.GuidPtrOf<IDxcResult>(), (void**)pCompileResult.GetAddressOf()).ThrowIf();

            stringArgs.Release();
            arguments.Release();
            Free(include->LpVtbl);
            Free(include);

            pSource.Release();

            utils.Release();

            compiler.Release();

            for (int i = 0; i < stringArgs.Count; i++)
            {
                stringArgs[i].Release();
            }

            ComPtr<IDxcBlobUtf8> pErrors = default;
            ComPtr<IDxcBlobUtf16> pOutputName = default;
            pCompileResult.GetOutput(OutKind.Errors, ref pErrors, out pOutputName);
            if (pErrors.Handle != null)
            {
                error = pErrors.GetStringPointerS();
                pErrors.Release();
            }

            HResult status;
            pCompileResult.GetStatus(&status);
            if (HResult.IndicatesFailure(status))
            {
                pCompileResult.Release();
                Logger.Error($"Error: {sourceName}");
                return false;
            }

            shaderBlob = pCompileResult;

            Logger.Info($"Done: {sourceName}");

            return true;
        }
    }
}