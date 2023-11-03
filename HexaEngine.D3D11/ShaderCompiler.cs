#define SHADER_FORCE_OPTIMIZE

namespace HexaEngine.D3D11
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Shaders;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Unsafes;
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D.Compilers;
    using Silk.NET.Direct3D11;
    using System.Collections.Concurrent;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using static HexaEngine.D3D11.D3D11GraphicsDevice;

    public class ShaderCompiler
    {
        private static readonly D3DCompiler D3DCompiler = D3DCompiler.GetApi();
        private static readonly ConcurrentDictionary<Pointer<ID3DInclude>, string> paths = new();

        public unsafe bool Compile(string source, ShaderMacro[] macros, string entryPoint, string sourceName, string profile, out Blob? shaderBlob, out string? error)
        {
            Logger.Info($"Compiling: {sourceName}");
            shaderBlob = null;
            error = null;
            ShaderFlags flags = (ShaderFlags)(1 << 21);
#if DEBUG && !RELEASE && !SHADER_FORCE_OPTIMIZE
            flags |= ShaderFlags.Debug | ShaderFlags.SkipOptimization | ShaderFlags.DebugNameForSource;
#else
            flags |= ShaderFlags.OptimizationLevel3;
#endif
            byte* pSource = source.ToUTF8();

            var pMacros = macros.Length > 0 ? AllocT<D3DShaderMacro>(macros.Length + 1) : null;

            for (int i = 0; i < macros.Length; i++)
            {
                var macro = macros[i];
                var pName = macro.Name.ToUTF8();
                var pDef = macro.Definition.ToUTF8();
                pMacros[i] = new(pName, pDef);
            }
            if (pMacros != null)
            {
                pMacros[macros.Length].Name = null;
                pMacros[macros.Length].Definition = null;
            }

            byte* pEntryPoint = entryPoint.ToUTF8();
            byte* pSourceName = sourceName.ToUTF8();
            byte* pProfile = profile.ToUTF8();

            ID3D10Blob* vBlob;
            ID3D10Blob* vError;

            // For more about that black magic visit https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/function-pointers
            delegate*<ID3DInclude*, D3DIncludeType, byte*, void*, void**, uint*, int> pOpen = &Open;
            delegate*<ID3DInclude*, void*, int> pClose = &Close;

            void** callbacks = AllocArray(2);

            callbacks[0] = pOpen;
            callbacks[1] = pClose;

            ID3DInclude* include = (ID3DInclude*)Alloc(sizeof(ID3DInclude) + sizeof(nint));

            include->LpVtbl = callbacks;

            paths.TryAdd(include, Path.GetDirectoryName(Path.Combine(Paths.CurrentShaderPath, sourceName)) ?? string.Empty);
            D3DCompiler.Compile(pSource, (nuint)Encoding.UTF8.GetByteCount(source) + 1, pSourceName, pMacros, include, pEntryPoint, pProfile, (uint)flags, 0, &vBlob, &vError);
            paths.Remove(include, out _);

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
                error = ToStringFromUTF8((byte*)vError->GetBufferPointer());
                vError->Release();
            }

            if (vBlob == null)
            {
                Logger.Error($"Error: {sourceName}");
                return false;
            }

            shaderBlob = new(vBlob->Buffer.ToArray());
            vBlob->Release();

            Logger.Info($"Done: {sourceName}");

            return true;
        }

        private static unsafe int Open(ID3DInclude* pInclude, D3DIncludeType IncludeType, byte* pFileName, void* pParentData, void** ppData, uint* pBytes)
        {
            string basePath = paths[pInclude];
            string fileName = Encoding.UTF8.GetString(MemoryMarshal.CreateReadOnlySpanFromNullTerminated(pFileName));
            string path = Path.Combine(basePath, fileName);
            var data = FileSystem.ReadAllBytes(path);

            *ppData = AllocCopyT(data);
            *pBytes = (uint)data.Length;
            return 0;
        }

        private static unsafe int Close(ID3DInclude* pInclude, void* pData)
        {
            Free(pData);
            return 0;
        }

        public unsafe Blob GetInputSignature(Blob shader)
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

        public unsafe Blob GetInputSignature(Shader* shader)
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

        public unsafe void Reflect<T>(Shader* blob, out ComPtr<T> reflector) where T : unmanaged, IComVtbl<T>
        {
            lock (D3DCompiler)
            {
                D3DCompiler.Reflect(blob->Bytecode, blob->Length, out reflector);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Compile(string code, ShaderMacro[] macros, string entry, string sourceName, string profile, Shader** shader, out string? error)
        {
            Compile(code, macros, entry, sourceName, profile, out var shaderBlob, out error);
            if (shaderBlob != null)
            {
                Shader* pShader = AllocT<Shader>();
                pShader->Bytecode = AllocCopyT((byte*)shaderBlob.BufferPointer, shaderBlob.PointerSize);
                pShader->Length = shaderBlob.PointerSize;
                *shader = pShader;
            }

            if (error != null)
            {
                Logger.Log(error);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Compile(string code, string entry, string sourceName, string profile, Shader** shader, out string? error)
        {
            Compile(code, Array.Empty<ShaderMacro>(), entry, sourceName, profile, shader, out error);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Compile(string code, ShaderMacro[] macros, string entry, string sourceName, string profile, Shader** shader)
        {
            Compile(code, macros, entry, sourceName, profile, shader, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Compile(string code, string entry, string sourceName, string profile, Shader** shader)
        {
            Compile(code, entry, sourceName, profile, shader, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void CompileFromFile(string path, ShaderMacro[] macros, string entry, string profile, Shader** shader, out string? error)
        {
            Compile(FileSystem.ReadAllText(Paths.CurrentShaderPath + path), macros, entry, path, profile, out var shaderBlob, out error);
            if (shaderBlob != null)
            {
                Shader* pShader = AllocT<Shader>();
                pShader->Bytecode = AllocCopyT((byte*)shaderBlob.BufferPointer, shaderBlob.PointerSize);
                pShader->Length = shaderBlob.PointerSize;
                *shader = pShader;
            }
            if (error != null)
            {
                Logger.Log(error);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void CompileFromFile(string path, string entry, string profile, Shader** shader, out string? error)
        {
            CompileFromFile(path, Array.Empty<ShaderMacro>(), entry, profile, shader, out error);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void CompileFromFile(string path, ShaderMacro[] macros, string entry, string profile, Shader** shader)
        {
            CompileFromFile(path, macros, entry, profile, shader, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void CompileFromFile(string path, string entry, string profile, Shader** shader)
        {
            CompileFromFile(path, entry, profile, shader, out _);
        }

        public unsafe void GetShaderOrCompileFile(string entry, string path, string profile, ShaderMacro[] macros, Shader** shader, bool bypassCache = false)
        {
            var fullName = Paths.CurrentShaderPath + path;
            uint crc = FileSystem.GetCrc32Hash(fullName);
            Shader* pShader = null;
            if (bypassCache || !ShaderCache.GetShader(path, crc, SourceLanguage.HLSL, macros, &pShader, out _))
            {
                Compile(FileSystem.ReadAllText(fullName), macros, entry, path, profile, out var shaderBlob, out string? error);

                if (shaderBlob != null)
                {
                    pShader = AllocT<Shader>();
                    pShader->Bytecode = AllocCopyT((byte*)shaderBlob.BufferPointer, shaderBlob.PointerSize);
                    pShader->Length = shaderBlob.PointerSize;
                }

                Logger.LogIfNotNull(error);

                if (pShader == null)
                {
                    return;
                }

                ShaderCache.CacheShader(path, crc, SourceLanguage.HLSL, macros, Array.Empty<InputElementDescription>(), pShader);
            }
            *shader = pShader;
        }

        public unsafe void GetShaderOrCompileFileWithInputSignature(string entry, string path, string profile, ShaderMacro[] macros, Shader** shader, out InputElementDescription[]? inputElements, out Blob? signature, bool bypassCache = false)
        {
            uint crc = FileSystem.GetCrc32Hash(Paths.CurrentShaderPath + path);
            Shader* pShader;
            if (bypassCache || !ShaderCache.GetShader(path, crc, SourceLanguage.HLSL, macros, &pShader, out inputElements))
            {
                CompileFromFile(path, macros, entry, profile, &pShader);
                signature = null;
                inputElements = null;
                if (pShader == null)
                {
                    return;
                }

                signature = GetInputSignature(pShader);
                inputElements = GetInputElementsFromSignature(pShader, signature);
                ShaderCache.CacheShader(path, crc, SourceLanguage.HLSL, macros, inputElements, pShader);
            }
            *shader = pShader;
            signature = GetInputSignature(pShader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe InputElementDescription[] GetInputElementsFromSignature(Shader* shader, Blob signature)
        {
            ComPtr<ID3D11ShaderReflection> reflection;
            Compiler.Reflect(shader, out reflection);
            ShaderDesc desc;
            reflection.GetDesc(&desc);

            var inputElements = new InputElementDescription[desc.InputParameters];
            for (uint i = 0; i < desc.InputParameters; i++)
            {
                SignatureParameterDesc parameterDesc;
                reflection.GetInputParameterDesc(i, &parameterDesc);

                InputElementDescription inputElement = new()
                {
                    SemanticName = Utils.ToStr(parameterDesc.SemanticName),
                    SemanticIndex = (int)parameterDesc.SemanticIndex,
                    Slot = 0,
                    AlignedByteOffset = -1,
                    Classification = Core.Graphics.InputClassification.PerVertexData,
                    InstanceDataStepRate = 0
                };

                if (parameterDesc.Mask == (byte)RegisterComponentMaskFlags.ComponentX)
                {
                    inputElement.Format = parameterDesc.ComponentType switch
                    {
                        D3DRegisterComponentType.D3DRegisterComponentUint32 => Format.R32UInt,
                        D3DRegisterComponentType.D3DRegisterComponentSint32 => Format.R32SInt,
                        D3DRegisterComponentType.D3DRegisterComponentFloat32 => Format.R32Float,
                        _ => Format.Unknown,
                    };
                }

                if (parameterDesc.Mask == (byte)(RegisterComponentMaskFlags.ComponentX | RegisterComponentMaskFlags.ComponentY))
                {
                    inputElement.Format = parameterDesc.ComponentType switch
                    {
                        D3DRegisterComponentType.D3DRegisterComponentUint32 => Format.R32G32UInt,
                        D3DRegisterComponentType.D3DRegisterComponentSint32 => Format.R32G32SInt,
                        D3DRegisterComponentType.D3DRegisterComponentFloat32 => Format.R32G32Float,
                        _ => Format.Unknown,
                    };
                }

                if (parameterDesc.Mask == (byte)(RegisterComponentMaskFlags.ComponentX | RegisterComponentMaskFlags.ComponentY | RegisterComponentMaskFlags.ComponentZ))
                {
                    inputElement.Format = parameterDesc.ComponentType switch
                    {
                        D3DRegisterComponentType.D3DRegisterComponentUint32 => Format.R32G32B32UInt,
                        D3DRegisterComponentType.D3DRegisterComponentSint32 => Format.R32G32B32SInt,
                        D3DRegisterComponentType.D3DRegisterComponentFloat32 => Format.R32G32B32Float,
                        _ => Format.Unknown,
                    };
                }

                if (parameterDesc.Mask == (byte)(RegisterComponentMaskFlags.ComponentX | RegisterComponentMaskFlags.ComponentY | RegisterComponentMaskFlags.ComponentZ | RegisterComponentMaskFlags.ComponentW))
                {
                    inputElement.Format = parameterDesc.ComponentType switch
                    {
                        D3DRegisterComponentType.D3DRegisterComponentUint32 => Format.R32G32B32A32UInt,
                        D3DRegisterComponentType.D3DRegisterComponentSint32 => Format.R32G32B32A32SInt,
                        D3DRegisterComponentType.D3DRegisterComponentFloat32 => Format.R32G32B32A32Float,
                        _ => Format.Unknown,
                    };
                }

                inputElements[i] = inputElement;
            }

            reflection.Release();
            return inputElements;
        }
    }
}