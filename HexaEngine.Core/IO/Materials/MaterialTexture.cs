namespace HexaEngine.Core.IO.Materials
{
    using HexaEngine.Core.Extensions;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System.Text;

    public struct MaterialTexture
    {
        public static readonly TextureType[] TextureTypes = Enum.GetValues<TextureType>();
        public static readonly string[] TextureTypeNames = Enum.GetNames(typeof(TextureType));
        public static readonly BlendMode[] BlendModes = Enum.GetValues<BlendMode>();
        public static readonly string[] BlendModeNames = Enum.GetNames(typeof(BlendMode));
        public static readonly TextureOp[] TextureOps = Enum.GetValues<TextureOp>();
        public static readonly string[] TextureOpNames = Enum.GetNames(typeof(TextureOp));
        public static readonly TextureMapMode[] TextureMapModes = Enum.GetValues<TextureMapMode>();
        public static readonly string[] TextureMapModeNames = Enum.GetNames<TextureMapMode>();

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
            data.File = stream.ReadString(encoding, endianness) ?? string.Empty;
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

        public SamplerDescription GetSamplerDesc()
        {
            return new(Filter.Anisotropic, Convert(U), Convert(V), TextureAddressMode.Clamp, 0, 16, ComparisonFunction.Never, default, 0, int.MaxValue);
        }

        public ShaderMacro AsShaderMacro()
        {
            var type = Type;
            if (type == TextureType.Diffuse)
                type = TextureType.BaseColor;
            return new($"Has{type}Tex", (!string.IsNullOrWhiteSpace(File)).ToHLSL());
        }

        private static TextureAddressMode Convert(TextureMapMode mode)
        {
            return mode switch
            {
                TextureMapMode.Wrap => TextureAddressMode.Wrap,
                TextureMapMode.Clamp => TextureAddressMode.Clamp,
                TextureMapMode.Mirror => TextureAddressMode.Mirror,
                TextureMapMode.Decal => TextureAddressMode.Clamp,
                _ => throw new NotSupportedException(),
            };
        }

        public override string ToString()
        {
            return $"{Type}, {File}, {Blend}, {Op}, {Mapping}, {UVWSrc}, {U}, {V}, {Flags}";
        }
    }
}