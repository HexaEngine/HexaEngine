namespace ShaderCompiler
{
    using SharpGen.Runtime;
    using System.IO;
    using System.Runtime.InteropServices;
    using Vortice.D3DCompiler;
    using Vortice.Direct3D;

    public static class Compiler
    {
        public struct ShaderDebugName
        {
            public ushort Flags;       // Reserved, must be set to zero.
            public ushort NameLength;  // Length of the debug name, without null terminator.
                                       // Followed by NameLength bytes of the UTF-8-encoded name.
                                       // Followed by a null terminator.
                                       // Followed by [0-3] zero bytes to align to a 4-byte boundary.
        }

        public class ShaderIncludeHandler : CallbackBase, Include
        {
            public string TargetPath { get; }

            public ShaderIncludeHandler(string targetPath)
            {
                TargetPath = targetPath;
            }

            public Stream Open(IncludeType type, string fileName, Stream? parentStream)
            {
                var includeFile = GetFilePath(fileName);

                if (!File.Exists(includeFile))
                    throw new FileNotFoundException($"Include file '{fileName}' not found.");

                var includeStream = File.OpenRead(includeFile);

                return includeStream;
            }

            private string GetFilePath(string fileName)
            {
                var path = Path.Combine(Path.GetDirectoryName(TargetPath) ?? string.Empty, fileName);
                return path;
            }

            public void Close(Stream stream)
            {
                stream.Dispose();
            }
        }

        public static void Compile(string shaderPath, string entry, string version, out Blob blob)
        {
            ShaderFlags flags = (ShaderFlags)(1 << 21);
#if DEBUG && !RELEASE && !SHADER_FORCE_OPTIMIZE
            flags |= ShaderFlags.Debug | ShaderFlags.SkipOptimization | ShaderFlags.DebugNameForSource;
#endif

            var fs = File.OpenRead(shaderPath);
            var bytes = new byte[fs.Length];
            fs.Read(bytes);
            fs.Dispose();
            ShaderIncludeHandler handler = new(shaderPath);
#nullable disable
            Vortice.D3DCompiler.Compiler.Compile(bytes, null, handler, entry, shaderPath, version, flags, out blob, out var error);
#nullable enable
#if DEBUG && !RELEASE && !SHADER_FORCE_OPTIMIZE
            if (blob != null)
            {
                var pdb = Vortice.D3DCompiler.Compiler.GetBlobPart(blob.BufferPointer, blob.BufferSize, ShaderBytecodePart.Pdb, 0);
                var pdbname = Vortice.D3DCompiler.Compiler.GetBlobPart(blob.BufferPointer, blob.BufferSize, ShaderBytecodePart.DebugName, 0);
                var pDebugNameData = Marshal.PtrToStructure<ShaderDebugName>(pdbname.BufferPointer);
                var name = Marshal.PtrToStringUTF8(pdbname.BufferPointer + 4, pDebugNameData.NameLength);
                Console.WriteLine($"{shaderPath} -> {name}");
                //File.WriteAllBytes(Path.Combine(Paths.CurrentPDBShaderPath, name), pdb.GetBytes());
                pdb.Dispose();
                pdbname.Dispose();
            }
#endif
            if (error is not null)
            {
                var text = error.AsString();
                Console.WriteLine(text);
            }
        }
    }
}