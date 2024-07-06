//#define SHADER_FORCE_OPTIMIZE

namespace HexaEngine.D3D11
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Reflection;
    using HexaEngine.Core.Graphics.Shaders;
    using HexaEngine.Core.IO;
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D.Compilers;
    using Silk.NET.Direct3D11;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    public class ShaderCompiler
    {
        private static readonly ILogger Logger = LoggerFactory.General;
        private static readonly D3DCompiler D3DCompiler = D3DCompiler.GetApi();

        public static unsafe bool Compile(string source, ShaderMacro[] macros, string entryPoint, string sourceName, string profile, out Blob? shaderBlob, out string? error)
        {
            Logger.Info($"Compiling: {sourceName}");
            shaderBlob = null;
            error = null;
            ShaderFlags flags = (ShaderFlags)(1 << 21);
#if DEBUG && !RELEASE && !SHADER_FORCE_OPTIMIZE
            flags |= ShaderFlags.Debug | ShaderFlags.DebugNameForSource;
#else
            flags |= ShaderFlags.OptimizationLevel3;
#endif
            byte* pSource = source.ToUTF8Ptr();
            int sourceLen = Encoding.UTF8.GetByteCount(source) + 1;

            var pMacros = macros.Length > 0 ? AllocT<D3DShaderMacro>(macros.Length + 1) : null;

            for (int i = 0; i < macros.Length; i++)
            {
                var macro = macros[i];
                var pName = macro.Name.ToUTF8Ptr();
                var pDef = macro.Definition.ToUTF8Ptr();
                pMacros[i] = new(pName, pDef);
            }
            if (pMacros != null)
            {
                pMacros[macros.Length].Name = null;
                pMacros[macros.Length].Definition = null;
            }

            byte* pEntryPoint = entryPoint.ToUTF8Ptr();
            byte* pSourceName = sourceName.ToUTF8Ptr();
            byte* pProfile = profile.ToUTF8Ptr();

            ID3D10Blob* vBlob;
            ID3D10Blob* vError;

            IncludeHandler handler = new(Path.GetDirectoryName(Path.Combine(Paths.CurrentShaderPath, sourceName)) ?? string.Empty);
            ID3DInclude* include = (ID3DInclude*)Alloc(sizeof(ID3DInclude) + sizeof(nint));
            include->LpVtbl = (void**)Alloc(sizeof(nint) * 2);
            include->LpVtbl[0] = (void*)Marshal.GetFunctionPointerForDelegate(handler.Open);
            include->LpVtbl[1] = (void*)Marshal.GetFunctionPointerForDelegate(handler.Close);

            D3DCompiler.Compile(pSource, (nuint)sourceLen, pSourceName, pMacros, include, pEntryPoint, pProfile, (uint)flags, 0, &vBlob, &vError);

            Free(include->LpVtbl);
            Free(include);

            Free(pSource);
            Free(pEntryPoint);
            Free(pSourceName);
            Free(pProfile);

            for (int i = 0; i < macros.Length; i++)
            {
                var macro = pMacros[i];
                Free(macro.Name);
                Free(macro.Definition);
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

        public static unsafe void Reflect<T>(Shader* blob, out ComPtr<T> reflector) where T : unmanaged, IComVtbl<T>
        {
            D3DCompiler.Reflect(blob->Bytecode, blob->Length, out reflector);
        }

        public unsafe ShaderReflection Reflect(Shader* blob)
        {
            Reflect(blob, out ComPtr<ID3D11ShaderReflection> reflection);

            ShaderReflection shaderReflection = new();

            ShaderDesc shaderDesc;
            reflection.GetDesc(&shaderDesc);

            shaderReflection.Version = shaderDesc.Version;
            shaderReflection.Creator = ToStringFromUTF8(shaderDesc.Creator) ?? string.Empty;
            shaderReflection.Flags = shaderDesc.Flags;
            shaderReflection.ConstantBuffers = new Core.Graphics.Reflection.ShaderBufferDesc[shaderDesc.ConstantBuffers];
            shaderReflection.BoundResources = new ShaderInputBindDescription[shaderDesc.BoundResources];
            shaderReflection.InputParameters = shaderDesc.InputParameters;
            shaderReflection.OutputParameters = shaderDesc.OutputParameters;
            shaderReflection.GSOutputTopology = Helper.ConvertBack(shaderDesc.GSOutputTopology);
            shaderReflection.GSMaxOutputVertexCount = shaderDesc.GSMaxOutputVertexCount;
            shaderReflection.InputPrimitive = Helper.ConvertBack(shaderDesc.InputPrimitive);
            shaderReflection.PatchConstantParameters = shaderDesc.PatchConstantParameters;
            shaderReflection.CGSInstanceCount = shaderDesc.CGSInstanceCount;
            shaderReflection.CControlPoints = shaderDesc.CControlPoints;
            shaderReflection.HSOutputPrimitive = Helper.ConvertBack(shaderDesc.HSOutputPrimitive);
            shaderReflection.HSPartitioning = Helper.ConvertBack(shaderDesc.HSPartitioning);
            shaderReflection.TessellatorDomain = Helper.ConvertBack(shaderDesc.TessellatorDomain);
            shaderReflection.CBarrierInstructions = shaderDesc.CBarrierInstructions;
            shaderReflection.CInterlockedInstructions = shaderDesc.CInterlockedInstructions;
            shaderReflection.CTextureStoreInstructions = shaderDesc.CTextureStoreInstructions;

            for (uint i = 0; i < shaderDesc.ConstantBuffers; i++)
            {
                var cb = reflection.GetConstantBufferByIndex(i);
                Silk.NET.Direct3D11.ShaderBufferDesc shaderBufferDesc;
                cb->GetDesc(&shaderBufferDesc);
                shaderReflection.ConstantBuffers[i] = Helper.ConvertBack(shaderBufferDesc);
                for (uint j = 0; j < shaderBufferDesc.Variables; j++)
                {
                    var vb = cb->GetVariableByIndex(j);
                    Silk.NET.Direct3D11.ShaderVariableDesc shaderVariableDesc;
                    vb->GetDesc(&shaderVariableDesc);
                    shaderReflection.ConstantBuffers[i].Variables[j] = Helper.ConvertBack(shaderVariableDesc);
                    var tb = vb->GetType();
                    Silk.NET.Direct3D11.ShaderTypeDesc shaderTypeDesc;
                    tb->GetDesc(&shaderTypeDesc);
                }
            }

            for (uint i = 0; i < shaderDesc.BoundResources; i++)
            {
                ShaderInputBindDesc shaderInputBindDesc;
                reflection.GetResourceBindingDesc(i, &shaderInputBindDesc);
                shaderReflection.BoundResources[i] = Helper.ConvertBack(shaderInputBindDesc);
            }

            reflection.Release();

            return shaderReflection;
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

        public unsafe void GetShaderOrCompileCode(string entry, string path, string code, string profile, ShaderMacro[] macros, Shader** shader, bool bypassCache = false)
        {
            uint crc = FileSystem.GetCrc32HashFromText(code);
            Shader* pShader = null;
            if (bypassCache || !ShaderCache.GetShader(path, crc, SourceLanguage.HLSL, macros, &pShader, out _))
            {
                Compile(code, macros, entry, path, profile, out var shaderBlob, out string? error);

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

                ShaderCache.CacheShader(path, crc, SourceLanguage.HLSL, macros, [], pShader);
            }
            *shader = pShader;
        }

        public unsafe void GetShaderOrCompileFileOrCode(string entry, string path, string? code, string profile, ShaderMacro[] macros, Shader** shader, bool bypassCache = false)
        {
            if (code == null)
            {
                GetShaderOrCompileFile(entry, path, profile, macros, shader, bypassCache);
            }
            else
            {
                GetShaderOrCompileCode(entry, path, code, profile, macros, shader, bypassCache);
            }
        }

        public unsafe void GetShaderOrCompileFileWithInputSignature(string entry, string path, string profile, ShaderMacro[] macros, Shader** shader, out InputElementDescription[]? inputElements, out Blob? signature, bool bypassCache = false)
        {
            var fullName = Paths.CurrentShaderPath + path;
            uint crc = FileSystem.GetCrc32Hash(fullName);
            Shader* pShader = null;
            if (bypassCache || !ShaderCache.GetShader(path, crc, SourceLanguage.HLSL, macros, &pShader, out inputElements))
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
                    signature = null;
                    inputElements = null;
                    return;
                }

                signature = GetInputSignature(pShader);
                inputElements = GetInputElementsFromSignature(pShader, signature);
                ShaderCache.CacheShader(path, crc, SourceLanguage.HLSL, macros, inputElements, pShader);
            }
            *shader = pShader;
            signature = GetInputSignature(pShader);
        }

        public unsafe void GetShaderOrCompileCodeWithInputSignature(string entry, string path, string code, string profile, ShaderMacro[] macros, Shader** shader, out InputElementDescription[]? inputElements, out Blob? signature, bool bypassCache = false)
        {
            uint crc = FileSystem.GetCrc32HashFromText(code);
            Shader* pShader = null;
            if (bypassCache || !ShaderCache.GetShader(path, crc, SourceLanguage.HLSL, macros, &pShader, out inputElements))
            {
                Compile(code, macros, entry, path, profile, out var shaderBlob, out string? error);

                if (shaderBlob != null)
                {
                    pShader = AllocT<Shader>();
                    pShader->Bytecode = AllocCopyT((byte*)shaderBlob.BufferPointer, shaderBlob.PointerSize);
                    pShader->Length = shaderBlob.PointerSize;
                }

                Logger.LogIfNotNull(error);

                if (pShader == null)
                {
                    signature = null;
                    inputElements = null;
                    return;
                }

                signature = GetInputSignature(pShader);
                inputElements = GetInputElementsFromSignature(pShader, signature);
                ShaderCache.CacheShader(path, crc, SourceLanguage.HLSL, macros, inputElements, pShader);
            }
            *shader = pShader;
            signature = GetInputSignature(pShader);
        }

        public unsafe void GetShaderOrCompileFileOrCodeWithInputSignature(string entry, string path, string? code, string profile, ShaderMacro[] macros, Shader** shader, out InputElementDescription[]? inputElements, out Blob? signature, bool bypassCache = false)
        {
            if (code == null)
            {
                GetShaderOrCompileFileWithInputSignature(entry, path, profile, macros, shader, out inputElements, out signature, bypassCache);
            }
            else
            {
                GetShaderOrCompileCodeWithInputSignature(entry, path, code, profile, macros, shader, out inputElements, out signature, bypassCache);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe InputElementDescription[] GetInputElementsFromSignature(Shader* shader, Blob signature)
        {
            Reflect(shader, out ComPtr<ID3D11ShaderReflection> reflection);
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