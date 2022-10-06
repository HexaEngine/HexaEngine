namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Unsafes;
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D.Compilers;
    using System.Collections.Concurrent;
    using System.Runtime.InteropServices;
    using System.Text;

    public static unsafe class ShaderCompiler
    {
        private static readonly D3DCompiler D3DCompiler = D3DCompiler.GetApi();
        private static readonly ConcurrentDictionary<Pointer<ID3DInclude>, string> paths = new();

        public static bool Compile(string source, ShaderMacro[] macros, string entryPoint, string sourceName, string profile, out Blob? shaderBlob, out Blob? errorBlob)
        {
            shaderBlob = null;
            errorBlob = null;
            ShaderFlags flags = (ShaderFlags)(1 << 21);
#if DEBUG && !RELEASE && !SHADER_FORCE_OPTIMIZE
            flags |= ShaderFlags.Debug | ShaderFlags.SkipOptimization | ShaderFlags.DebugNameForSource;
#else
            flags |= ShaderFlags.OptimizationLevel3;
#endif

#nullable disable
            var vmacros = macros.Select(x => new D3DShaderMacro(x.Name.ToBytes(), x.Definition.ToBytes())).Union(new D3DShaderMacro[] { new(null, null) }).ToArray();
            var pMacros = Utils.AsPointer(vmacros);
#nullable enable

            ID3D10Blob* vBlob;
            ID3D10Blob* vError;
            byte* pSource = Utils.ToBytes(source);

            // For more about that black magic visit https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/function-pointers
            delegate*<ID3DInclude*, D3DIncludeType, byte*, void*, void**, uint*, int> pOpen = &Open;
            delegate*<ID3DInclude*, void*, int> pClose = &Close;

            ID3DInclude include;

            void*[] callbacks = new void*[] { pOpen, pClose };

            fixed (void** pCallbacks = callbacks)
            {
                include = new ID3DInclude(pCallbacks);
            }

            ID3DInclude* pInclude = &include;

            paths.TryAdd(pInclude, Path.GetDirectoryName(Path.Combine(Paths.CurrentShaderPath, sourceName)) ?? string.Empty);
            D3DCompiler.Compile(pSource, (nuint)source.Length, sourceName.ToBytes(), pMacros, pInclude, entryPoint.ToBytes(), profile.ToBytes(), (uint)flags, 0, &vBlob, &vError);
            paths.Remove(pInclude, out _);

            if (vError != null)
            {
                errorBlob = new(vError->Buffer.ToArray());
                vError->Release();
            }

            if (vBlob == null)
            {
                return false;
            }

            shaderBlob = new(vBlob->Buffer.ToArray());
            vBlob->Release();

            return true;
        }

        private static unsafe int Open(ID3DInclude* pInclude, D3DIncludeType IncludeType, byte* pFileName, void* pParentData, void** ppData, uint* pBytes)
        {
            string basePath = paths[pInclude];
            string fileName = Encoding.UTF8.GetString(MemoryMarshal.CreateReadOnlySpanFromNullTerminated(pFileName));
            string path = Path.Combine(basePath, fileName);
            byte[] data = FileSystem.ReadAllBytes(path);
            *ppData = Utils.AsPointer(data);
            *pBytes = (uint)data.Length;
            return 0;
        }

        private static unsafe int Close(ID3DInclude* pInclude, void* pData)
        {
            return 0;
        }

        public static Blob GetInputSignature(Blob shader)
        {
            ID3D10Blob* signature;
            D3DCompiler.GetInputSignatureBlob((void*)shader.BufferPointer, (nuint)(int)shader.PointerSize, &signature);
            Blob output = new(signature->Buffer.ToArray());
            signature->Release();
            return output;
        }

        public static void Reflect(Blob blob, Guid guid, void** reflector)
        {
            D3DCompiler.Reflect((void*)blob.BufferPointer, (nuint)(int)blob.PointerSize, Utils.Guid(guid), reflector);
        }
    }
}