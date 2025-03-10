﻿//#define SHADER_FORCE_OPTIMIZE

namespace HexaEngine.D3D11
{
    using Hexa.NET.D3D11;
    using Hexa.NET.D3DCommon;
    using Hexa.NET.D3DCompiler;
    using Hexa.NET.Logging;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Reflection;
    using HexaEngine.Core.Graphics.Shaders;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Security.Cryptography;
    using HexaGen.Runtime.COM;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using D3DRegisterComponentType = Hexa.NET.D3DCommon.RegisterComponentType;
    using D3DShaderMacro = Hexa.NET.D3DCommon.ShaderMacro;
    using ShaderMacro = Core.Graphics.ShaderMacro;

    public class ShaderCompiler
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(ShaderCompiler));

        public static unsafe bool Compile(string source, ShaderMacro[] macros, string entryPoint, string sourceName, string basePath, string profile, out Blob? shaderBlob, out string? error)
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

            string systemInclude = Paths.CurrentShaderPath;

            IncludeHandler handler = new(Path.Combine(Paths.CurrentShaderPath, basePath) ?? string.Empty, systemInclude);
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

            shaderBlob = new(vBlob->GetBufferPointer(), vBlob->GetBufferSize(), copy: true);
            vBlob->Release();

            Logger.Info($"Done: {sourceName}");

            return true;
        }

        public static unsafe bool Compile(byte* pSource, int sourceLen, ShaderMacro[] macros, string entryPoint, string sourceName, string basePath, string profile, out Blob? shaderBlob, out string? error)
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

            string systemInclude = Paths.CurrentShaderPath;

            IncludeHandler handler = new(Path.Combine(Paths.CurrentShaderPath, basePath), systemInclude);
            ID3DInclude* include = (ID3DInclude*)Alloc(sizeof(ID3DInclude) + sizeof(nint));
            include->LpVtbl = (void**)Alloc(sizeof(nint) * 2);
            include->LpVtbl[0] = (void*)Marshal.GetFunctionPointerForDelegate(handler.Open);
            include->LpVtbl[1] = (void*)Marshal.GetFunctionPointerForDelegate(handler.Close);

            D3DCompiler.Compile(pSource, (nuint)sourceLen, pSourceName, pMacros, include, pEntryPoint, pProfile, (uint)flags, 0, &vBlob, &vError);

            Free(include->LpVtbl);
            Free(include);

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

            shaderBlob = new(vBlob->GetBufferPointer(), vBlob->GetBufferSize(), copy: true);
            vBlob->Release();

            Logger.Info($"Done: {sourceName}");

            return true;
        }

        public unsafe Blob GetInputSignature(Blob shader)
        {
            ID3D10Blob* signature;
            D3DCompiler.GetInputSignatureBlob((void*)shader.BufferPointer, (nuint)(int)shader.PointerSize, &signature);
            Blob output = new(signature->GetBufferPointer(), signature->GetBufferSize(), copy: true);
            signature->Release();
            return output;
        }

        public unsafe Blob GetInputSignature(Shader* shader)
        {
            ID3D10Blob* signature;
            D3DCompiler.GetInputSignatureBlob(shader->Bytecode, shader->Length, &signature);
            Blob output = new(signature->GetBufferPointer(), signature->GetBufferSize(), copy: true);
            signature->Release();
            return output;
        }

        public static unsafe void Reflect<T>(Shader* blob, out ComPtr<T> reflector) where T : unmanaged, IComObject<T>
        {
            D3DCompiler.Reflect(blob->Bytecode, blob->Length, out reflector).ThrowIf();
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
                Hexa.NET.D3D11.ShaderBufferDesc shaderBufferDesc;
                cb->GetDesc(&shaderBufferDesc);
                shaderReflection.ConstantBuffers[i] = Helper.ConvertBack(shaderBufferDesc);
                for (uint j = 0; j < shaderBufferDesc.Variables; j++)
                {
                    var vb = cb->GetVariableByIndex(j);
                    Hexa.NET.D3D11.ShaderVariableDesc shaderVariableDesc;
                    vb->GetDesc(&shaderVariableDesc);
                    shaderReflection.ConstantBuffers[i].Variables[j] = Helper.ConvertBack(shaderVariableDesc);
                    var tb = vb->GetType();
                    Hexa.NET.D3D11.ShaderTypeDesc shaderTypeDesc;
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

        public unsafe void GetShaderOrCompileFile(string entry, ShaderSource source, string profile, ShaderMacro[] macros, Shader** shader, bool bypassCache = false)
        {
            Shader* pShader = null;
            if (source is BytecodeShaderSource bytecodeShaderSource)
            {
                pShader = AllocT<Shader>();
                pShader->Bytecode = AllocCopyT(bytecodeShaderSource.Bytecode);
                pShader->Length = (nuint)bytecodeShaderSource.Bytecode.Length;
                *shader = pShader;
                return;
            }

            var data = source.GetData();
            uint crc = Crc32.Compute(data);

            if (bypassCache || !ShaderCache.GetShader(source.Identifier, crc, SourceLanguage.HLSL, macros, &pShader, out _))
            {
                Blob? shaderBlob; string? error;

                fixed (byte* pData = data)
                {
                    Compile(pData, data.Length, macros, entry, source.Identifier, source.WorkingDir, profile, out shaderBlob, out error);
                }
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

                ShaderCache.CacheShader(source.Identifier, crc, SourceLanguage.HLSL, macros, Array.Empty<InputElementDescription>(), pShader);
            }
            *shader = pShader;
        }

        public unsafe void GetShaderOrCompileFileWithInputSignature(string entry, ShaderSource source, string profile, ShaderMacro[] macros, Shader** shader, out InputElementDescription[]? inputElements, out Blob? signature, bool bypassCache = false)
        {
            Shader* pShader = null;
            if (source is BytecodeShaderSource bytecodeShaderSource)
            {
                pShader = AllocT<Shader>();
                pShader->Bytecode = AllocCopyT(bytecodeShaderSource.Bytecode);
                pShader->Length = (nuint)bytecodeShaderSource.Bytecode.Length;
                *shader = pShader;

                signature = GetInputSignature(pShader);
                inputElements = GetInputElementsFromSignature(pShader, signature);
                return;
            }

            var data = source.GetData();
            uint crc = Crc32.Compute(data);

            if (bypassCache || !ShaderCache.GetShader(source.Identifier, crc, SourceLanguage.HLSL, macros, &pShader, out inputElements))
            {
                Blob? shaderBlob; string? error;

                fixed (byte* pData = data)
                {
                    Compile(pData, data.Length, macros, entry, source.Identifier, source.WorkingDir, profile, out shaderBlob, out error);
                }

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
                ShaderCache.CacheShader(source.Identifier, crc, SourceLanguage.HLSL, macros, inputElements, pShader);
            }
            *shader = pShader;
            signature = GetInputSignature(pShader);
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
                        D3DRegisterComponentType.Uint32 => Format.R32UInt,
                        D3DRegisterComponentType.Sint32 => Format.R32SInt,
                        D3DRegisterComponentType.Float32 => Format.R32Float,
                        _ => Format.Unknown,
                    };
                }

                if (parameterDesc.Mask == (byte)(RegisterComponentMaskFlags.ComponentX | RegisterComponentMaskFlags.ComponentY))
                {
                    inputElement.Format = parameterDesc.ComponentType switch
                    {
                        D3DRegisterComponentType.Uint32 => Format.R32G32UInt,
                        D3DRegisterComponentType.Sint32 => Format.R32G32SInt,
                        D3DRegisterComponentType.Float32 => Format.R32G32Float,
                        _ => Format.Unknown,
                    };
                }

                if (parameterDesc.Mask == (byte)(RegisterComponentMaskFlags.ComponentX | RegisterComponentMaskFlags.ComponentY | RegisterComponentMaskFlags.ComponentZ))
                {
                    inputElement.Format = parameterDesc.ComponentType switch
                    {
                        D3DRegisterComponentType.Uint32 => Format.R32G32B32UInt,
                        D3DRegisterComponentType.Sint32 => Format.R32G32B32SInt,
                        D3DRegisterComponentType.Float32 => Format.R32G32B32Float,
                        _ => Format.Unknown,
                    };
                }

                if (parameterDesc.Mask == (byte)(RegisterComponentMaskFlags.ComponentX | RegisterComponentMaskFlags.ComponentY | RegisterComponentMaskFlags.ComponentZ | RegisterComponentMaskFlags.ComponentW))
                {
                    inputElement.Format = parameterDesc.ComponentType switch
                    {
                        D3DRegisterComponentType.Uint32 => Format.R32G32B32A32UInt,
                        D3DRegisterComponentType.Sint32 => Format.R32G32B32A32SInt,
                        D3DRegisterComponentType.Float32 => Format.R32G32B32A32Float,
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