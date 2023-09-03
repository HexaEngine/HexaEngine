namespace HexaEngine.Core.IO.Materials
{
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Text;

    public struct MaterialShader
    {
        public MaterialShaderType Type;
        public string Source;
        public byte[]? Bytecode;

        public MaterialShader(MaterialShaderType type, string source, byte[]? bytecode = null)
        {
            Type = type;
            Source = source;
            Bytecode = bytecode;
        }

        public static MaterialShader Read(Stream src, Encoding encoding, Endianness endianness)
        {
            MaterialShader shader;
            shader.Type = (MaterialShaderType)src.ReadInt32(endianness);
            shader.Source = src.ReadString(encoding, endianness);
            var bytecount = src.ReadInt32(endianness);
            if (bytecount > 0)
            {
                shader.Bytecode = src.Read(bytecount);
            }
            else
            {
                shader.Bytecode = null;
            }
            return shader;
        }

        public readonly void Write(Stream dst, Encoding encoding, Endianness endianness)
        {
            dst.WriteInt32((int)Type, endianness);
            dst.WriteString(Source, encoding, endianness);
            dst.WriteInt32(Bytecode?.Length ?? 0, endianness);
            if (Bytecode != null)
            {
                dst.Write(Bytecode);
            }
        }
    }
}