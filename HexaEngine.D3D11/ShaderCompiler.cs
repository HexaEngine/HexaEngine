namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.IO;
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D.Compilers;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class ShaderCompiler
    {
        private static readonly SemaphoreSlim semaphore = new(1);
        private static readonly D3DCompiler D3DCompiler = D3DCompiler.GetApi();
        private static readonly ConcurrentDictionary<Pointer<ID3DInclude>, string> paths = new();

        public static async Task<(bool, Blob?, Blob?)> CompileAsync(string source, ShaderMacro[] macros, string entryPoint, string sourceName, string profile)
        {
            await semaphore.WaitAsync();
            var result = CompileInternal(source, macros, entryPoint, sourceName, profile, out var shaderBlob, out var errorBlob);
            semaphore.Release();
            return (result, shaderBlob, errorBlob);
        }

        public static bool Compile(string source, ShaderMacro[] macros, string entryPoint, string sourceName, string profile, out Blob? shaderBlob, out Blob? errorBlob)
        {
            semaphore.Wait();
            var result = CompileInternal(source, macros, entryPoint, sourceName, profile, out shaderBlob, out errorBlob);
            semaphore.Release();
            return result;
        }

        private static unsafe bool CompileInternal(string source, ShaderMacro[] macros, string entryPoint, string sourceName, string profile, out Blob? shaderBlob, out Blob? errorBlob)
        {
            Debug.WriteLine($"Compiling: {sourceName}");
            shaderBlob = null;
            errorBlob = null;
            ShaderFlags flags = (ShaderFlags)(1 << 21);
#if DEBUG && !RELEASE && !SHADER_FORCE_OPTIMIZE
            flags |= ShaderFlags.Debug | ShaderFlags.SkipOptimization | ShaderFlags.DebugNameForSource;
#else
            flags |= ShaderFlags.OptimizationLevel3;
#endif
            byte* pSource = (byte*)Marshal.StringToCoTaskMemUTF8(source);

            var pMacros = macros.Length > 0 ? Alloc<D3DShaderMacro>(macros.Length) : null;
            for (int i = 0; i < macros.Length; i++)
            {
                var macro = macros[i];
                var pName = Marshal.StringToCoTaskMemUTF8(macro.Name);
                var pDef = Marshal.StringToCoTaskMemUTF8(macro.Definition);
                pMacros[i] = new((byte*)pName, (byte*)pDef);
            }

            byte* pEntryPoint = (byte*)Marshal.StringToCoTaskMemUTF8(entryPoint);
            byte* pSourceName = (byte*)Marshal.StringToCoTaskMemUTF8(sourceName);
            byte* pProfile = (byte*)Marshal.StringToCoTaskMemUTF8(profile);

            ID3D10Blob* vBlob;
            ID3D10Blob* vError;

            // For more about that black magic visit https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/function-pointers
            delegate*<ID3DInclude*, D3DIncludeType, byte*, void*, void**, uint*, int> pOpen = &Open;
            delegate*<ID3DInclude*, void*, int> pClose = &Close;

            void** callbacks = AllocArray(2);

            callbacks[0] = pOpen;
            callbacks[1] = pClose;

            ID3DInclude* include = Alloc<ID3DInclude>();

            include->LpVtbl = callbacks;

            lock (D3DCompiler)
            {
                paths.TryAdd(include, Path.GetDirectoryName(Path.Combine(Paths.CurrentShaderPath, sourceName)) ?? string.Empty);
                D3DCompiler.Compile(pSource, (nuint)source.Length, pSourceName, pMacros, include, pEntryPoint, pProfile, (uint)flags, 0, &vBlob, &vError);
                paths.Remove(include, out _);
            }

            Free(callbacks);
            Free(include);

            Marshal.FreeCoTaskMem((nint)pSource);
            Marshal.FreeCoTaskMem((nint)pEntryPoint);
            Marshal.FreeCoTaskMem((nint)pSourceName);
            Marshal.FreeCoTaskMem((nint)pProfile);

            for (int i = 0; i < macros.Length; i++)
            {
                var macro = pMacros[i];
                Marshal.FreeCoTaskMem((nint)macro.Name);
                Marshal.FreeCoTaskMem((nint)macro.Definition);
            }

            Free(pMacros);

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
            var data = FileSystem.ReadAllBytes(path);

            *ppData = AllocCopy(data);
            *pBytes = (uint)data.Length;
            return 0;
        }

        private static unsafe int Close(ID3DInclude* pInclude, void* pData)
        {
            Free(pData);
            return 0;
        }

        public static unsafe Blob GetInputSignature(Blob shader)
        {
            lock (D3DCompiler)
            {
                ID3D10Blob* signature;
                D3DCompiler.GetInputSignatureBlob((void*)shader.BufferPointer, (nuint)(int)shader.PointerSize, &signature);
                Blob output = new(signature->Buffer.ToArray());
                signature->Release();
                return output;
            }
        }

        public static unsafe Blob GetInputSignature(Shader* shader)
        {
            lock (D3DCompiler)
            {
                ID3D10Blob* signature;
                D3DCompiler.GetInputSignatureBlob(shader->Bytecode, shader->Length, &signature);
                Blob output = new(signature->Buffer.ToArray());
                signature->Release();
                return output;
            }
        }

        public static unsafe void Reflect(Blob blob, Guid guid, void** reflector)
        {
            lock (D3DCompiler)
            {
                D3DCompiler.Reflect((void*)blob.BufferPointer, (nuint)(int)blob.PointerSize, Utils.Guid(guid), reflector);
            }
        }

        public static unsafe void Reflect(Shader* blob, Guid guid, void** reflector)
        {
            lock (D3DCompiler)
            {
                D3DCompiler.Reflect(blob->Bytecode, blob->Length, Utils.Guid(guid), reflector);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Compile(string code, ShaderMacro[] macros, string entry, string sourceName, string profile, Shader** shader, out Blob? errorBlob)
        {
            Compile(code, macros, entry, sourceName, profile, out var shaderBlob, out errorBlob);
            if (shaderBlob != null)
            {
                Shader* pShader = Alloc<Shader>();
                pShader->Bytecode = AllocCopy((byte*)shaderBlob.BufferPointer, shaderBlob.PointerSize);
                pShader->Length = shaderBlob.PointerSize;
                *shader = pShader;
            }

            if (errorBlob != null)
                ImGuiConsole.Log(errorBlob.AsString());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Compile(string code, string entry, string sourceName, string profile, Shader** shader, out Blob? errorBlob)
        {
            Compile(code, Array.Empty<ShaderMacro>(), entry, sourceName, profile, shader, out errorBlob);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Compile(string code, ShaderMacro[] macros, string entry, string sourceName, string profile, Shader** shader)
        {
            Compile(code, macros, entry, sourceName, profile, shader, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Compile(string code, string entry, string sourceName, string profile, Shader** shader)
        {
            Compile(code, entry, sourceName, profile, shader, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CompileFromFile(string path, ShaderMacro[] macros, string entry, string profile, Shader** shader, out Blob? errorBlob)
        {
            Compile(FileSystem.ReadAllText(Paths.CurrentShaderPath + path), macros, entry, path, profile, out var shaderBlob, out errorBlob);
            if (shaderBlob != null)
            {
                Shader* pShader = Alloc<Shader>();
                pShader->Bytecode = AllocCopy((byte*)shaderBlob.BufferPointer, shaderBlob.PointerSize);
                pShader->Length = shaderBlob.PointerSize;
                *shader = pShader;
            }
            if (errorBlob != null)
                ImGuiConsole.Log(errorBlob.AsString());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CompileFromFile(string path, string entry, string profile, Shader** shader, out Blob? errorBlob)
        {
            CompileFromFile(path, Array.Empty<ShaderMacro>(), entry, profile, shader, out errorBlob);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CompileFromFile(string path, ShaderMacro[] macros, string entry, string profile, Shader** shader)
        {
            CompileFromFile(path, macros, entry, profile, shader, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CompileFromFile(string path, string entry, string profile, Shader** shader)
        {
            CompileFromFile(path, entry, profile, shader, out _);
        }

        public static unsafe void GetShaderOrCompile(string code, string entry, string path, string profile, ShaderMacro[] macros, Shader** shader, bool bypassCache = false)
        {
            Shader* pShader;
            if (bypassCache || !ShaderCache.GetShader(path, macros, &pShader))
            {
                Compile(code, entry, path, profile, &pShader);
                ShaderCache.CacheShader(path, macros, pShader);
            }
            *shader = pShader;
        }

        public static unsafe void GetShaderOrCompileFile(string entry, string path, string profile, ShaderMacro[] macros, Shader** shader, bool bypassCache = false)
        {
            Shader* pShader;
            if (bypassCache || !ShaderCache.GetShader(path, macros, &pShader))
            {
                CompileFromFile(path, entry, profile, &pShader);
                if (pShader == null) return;
                ShaderCache.CacheShader(path, macros, pShader);
            }
            *shader = pShader;
        }

        public static unsafe void GetShaderOrCompile(string code, string entry, string path, string profile, Shader** shader, bool bypassCache = false)
        {
            Shader* pShader;
            if (bypassCache || !ShaderCache.GetShader(path, Array.Empty<ShaderMacro>(), &pShader))
            {
                Compile(code, entry, path, profile, &pShader);
                ShaderCache.CacheShader(path, Array.Empty<ShaderMacro>(), pShader);
            }
            *shader = pShader;
        }

        public static unsafe void GetShaderOrCompileFile(string entry, string path, string profile, Shader** shader, bool bypassCache = false)
        {
            Shader* pShader;
            if (bypassCache || !ShaderCache.GetShader(path, Array.Empty<ShaderMacro>(), &pShader))
            {
                CompileFromFile(path, entry, profile, &pShader);
                ShaderCache.CacheShader(path, Array.Empty<ShaderMacro>(), pShader);
            }
            *shader = pShader;
        }

        public static unsafe void GetShaderOrCompile(string code, string path, string profile, Shader** shader, bool bypassCache = false)
        {
            Shader* pShader;
            if (bypassCache || !ShaderCache.GetShader(path, Array.Empty<ShaderMacro>(), &pShader))
            {
                Compile(code, "main", path, profile, &pShader);
                ShaderCache.CacheShader(path, Array.Empty<ShaderMacro>(), pShader);
            }
            *shader = pShader;
        }

        public static unsafe void GetShaderOrCompileFile(string path, string profile, Shader** shader, bool bypassCache = false)
        {
            Shader* pShader;
            if (bypassCache || !ShaderCache.GetShader(path, Array.Empty<ShaderMacro>(), &pShader))
            {
                CompileFromFile(path, "main", profile, &pShader);
                ShaderCache.CacheShader(path, Array.Empty<ShaderMacro>(), pShader);
            }
            *shader = pShader;
        }
    }
}