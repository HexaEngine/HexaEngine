namespace HexaEngine.Core.IO.Materials
{
    using HexaEngine.Core.Extensions;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System.Collections.Generic;
    using System.Text;

    public struct MaterialTexture
    {
        public static readonly MaterialTextureType[] TextureTypes = Enum.GetValues<MaterialTextureType>();
        public static readonly string[] TextureTypeNames = Enum.GetNames(typeof(MaterialTextureType));
        public static readonly BlendMode[] BlendModes = Enum.GetValues<BlendMode>();
        public static readonly string[] BlendModeNames = Enum.GetNames(typeof(BlendMode));
        public static readonly TextureOp[] TextureOps = Enum.GetValues<TextureOp>();
        public static readonly string[] TextureOpNames = Enum.GetNames(typeof(TextureOp));
        public static readonly TextureMapMode[] TextureMapModes = Enum.GetValues<TextureMapMode>();
        public static readonly string[] TextureMapModeNames = Enum.GetNames<TextureMapMode>();

        public MaterialTextureType Type;
        public string File;
        public BlendMode Blend;
        public TextureOp Op;
        public int Mapping;
        public int UVWSrc;
        public TextureMapMode U;
        public TextureMapMode V;
        public TextureFlags Flags;

        public readonly string Name => Type.ToString();

        public MaterialTexture(MaterialTextureType type, string file, BlendMode blend, TextureOp op, int mapping, int uVWSrc, TextureMapMode u, TextureMapMode v, TextureFlags flags)
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
            data.Type = (MaterialTextureType)stream.ReadInt32(endianness);
            data.File = stream.ReadString(encoding, endianness) ?? string.Empty;
            data.Blend = (BlendMode)stream.ReadInt32(endianness);
            data.Op = (TextureOp)stream.ReadInt32(endianness);
            data.Mapping = stream.ReadInt32(endianness);
            data.UVWSrc = stream.ReadInt32(endianness);
            data.U = (TextureMapMode)stream.ReadInt32(endianness);
            data.V = (TextureMapMode)stream.ReadInt32(endianness);
            data.Flags = (TextureFlags)stream.ReadInt32(endianness);
            return data;
        }

        public readonly void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            stream.WriteInt32((int)Type, endianness);
            stream.WriteString(File, encoding, endianness);
            stream.WriteInt32((int)Blend, endianness);
            stream.WriteInt32((int)Op, endianness);
            stream.WriteInt32(Mapping, endianness);
            stream.WriteInt32(UVWSrc, endianness);
            stream.WriteInt32((int)U, endianness);
            stream.WriteInt32((int)V, endianness);
            stream.WriteInt32((int)Flags, endianness);
        }

        public readonly SamplerStateDescription GetSamplerDesc()
        {
            return new(Filter.Anisotropic, Convert(U), Convert(V), TextureAddressMode.Clamp, 0, 16, ComparisonFunction.Never, default, 0, int.MaxValue);
        }

        public readonly ShaderMacro AsShaderMacro()
        {
            var type = Type;
            if (type == MaterialTextureType.Diffuse)
            {
                type = MaterialTextureType.BaseColor;
            }

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

        public override readonly string ToString()
        {
            return $"{Type}, {File}, {Blend}, {Op}, {Mapping}, {UVWSrc}, {U}, {V}, {Flags}";
        }

        public readonly IEnumerable<string> GetNameAlias()
        {
            yield return Name;

            if (Type == MaterialTextureType.Diffuse)
                yield return MaterialTextureType.BaseColor.ToString();
        }
    }
}