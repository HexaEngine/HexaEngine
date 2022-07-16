namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using Vortice.D3DCompiler;
    using Blob = Core.Graphics.Blob;

    public static unsafe class ShaderCompiler
    {
        public static bool Compile(string source, ShaderMacro[] macros, string entryPoint, string sourceName, string profile, out Blob? shaderBlob, out Blob? errorBlob)
        {
            ShaderFlags flags = (ShaderFlags)(1 << 21);
#if DEBUG && !RELEASE && !SHADER_FORCE_OPTIMIZE
            flags |= ShaderFlags.Debug | ShaderFlags.SkipOptimization | ShaderFlags.DebugNameForSource;
#endif
#if SHADER_FORCE_OPTIMIZE
            flags |= ShaderFlags.OptimizationLevel3;
#endif
#nullable disable
            var vmacros = macros.Select(x => new Vortice.Direct3D.ShaderMacro(x.Name, x.Definition)).Union(new Vortice.Direct3D.ShaderMacro[] { new(null, null) }).ToArray();
#nullable enable
            ShaderIncludeHandler handler = new(Path.Combine(Paths.CurrentShaderPath, sourceName));
            Compiler.Compile(source, vmacros, handler, entryPoint, sourceName, profile, flags, out var vBlob, out var vError);

            if (vError != null)
            {
                errorBlob = new(vError.BufferPointer, (int)vError.BufferSize);
                if (vBlob == null)
                {
                    shaderBlob = null;
                    return false;
                }

                shaderBlob = new(vBlob.BufferPointer, (int)vBlob.BufferSize);
            }
            else
            {
                errorBlob = null;
                shaderBlob = new(vBlob.BufferPointer, (int)vBlob.BufferSize);
            }

            return true;
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