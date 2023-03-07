namespace HexaEngine.Core.IO.Materials
{
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System.Text;

    public struct MaterialTexture
    {
        public TextureType Type;
        public string File;
        public BlendMode Blend;
        public TextureOp Op;
        public int Mapping;
        public int UVWSrc;
        public TextureMapMode U;
        public TextureMapMode V;
        public TextureFlags Flags;

        public MaterialTexture(TextureType type, string file, BlendMode blend, TextureOp op, int mapping, int uVWSrc, TextureMapMode u, TextureMapMode v, TextureFlags flags)
        {
            Type = type;
            File = file;
            Blend = blend;
            Op = op;
            Mapping = mapping;
            UVWSrc = uVWSrc;
            U = u;
            V = v;
            Flags = flags;
        }

        public static MaterialTexture Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            MaterialTexture data = new();
            data.Type = (TextureType)stream.ReadInt(endianness);
            data.File = stream.ReadString(encoding, endianness);
            data.Blend = (BlendMode)stream.ReadInt(endianness);
            data.Op = (TextureOp)stream.ReadInt(endianness);
            data.Mapping = stream.ReadInt(endianness);
            data.UVWSrc = stream.ReadInt(endianness);
            data.U = (TextureMapMode)stream.ReadInt(endianness);
            data.V = (TextureMapMode)stream.ReadInt(endianness);
            data.Flags = (TextureFlags)stream.ReadInt(endianness);
            return data;
        }

        public void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            stream.WriteInt((int)Type, endianness);
            stream.WriteString(File, encoding, endianness);
            stream.WriteInt((int)Blend, endianness);
            stream.WriteInt((int)Op, endianness);
            stream.WriteInt(Mapping, endianness);
            stream.WriteInt(UVWSrc, endianness);
            stream.WriteInt((int)U, endianness);
            stream.WriteInt((int)V, endianness);
            stream.WriteInt((int)Flags, endianness);
        }
    }
}