namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using SharpGen.Runtime;
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D11;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Vortice.D3DCompiler;
    using Vortice.Direct3D;
    using Blob = Core.Graphics.Blob;

    public static unsafe class ShaderCompiler
    {
        public static void Compile(string source, string entryPoint, string sourceName, string profile, out Blob? shaderBlob, out Blob? errorBlob)
        {
            ShaderFlags flags = (ShaderFlags)(1 << 21);
#if DEBUG && !RELEASE && !SHADER_FORCE_OPTIMIZE
            flags |= ShaderFlags.Debug | ShaderFlags.SkipOptimization | ShaderFlags.DebugNameForSource;
#endif
#if SHADER_FORCE_OPTIMIZE
            flags |= ShaderFlags.OptimizationLevel3;
#endif

            ShaderIncludeHandler handler = new(Path.Combine(Paths.CurrentShaderPath, sourceName));
            Compiler.Compile(source, null, handler, entryPoint, sourceName, profile, flags, out var vBlob, out var vError);

            shaderBlob = new(vBlob.BufferPointer, (int)vBlob.BufferSize);
            if (vError != null)
                errorBlob = new(vError.BufferPointer, (int)vError.BufferSize);
            else
                errorBlob = null;
        }

        public static Blob GetInputSignature(Blob shader)
        {
            Compiler.GetInputSignatureBlob(shader.BufferPointer, (SharpGen.Runtime.PointerSize)(uint)(int)shader.PointerSize, out var sign);
            return new(sign.BufferPointer, (int)sign.BufferSize);
        }

        public static void Reflect(Blob blob, Guid guid, void** reflector)
        {
            Compiler.Reflect(blob.BufferPointer.ToPointer(), (SharpGen.Runtime.PointerSize)(uint)(int)blob.PointerSize, guid, out IntPtr ptr);
            *reflector = ptr.ToPointer();
        }
    }
}