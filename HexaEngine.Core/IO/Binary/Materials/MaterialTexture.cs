namespace HexaEngine.Core.IO.Binary.Materials
{
    using HexaEngine.Core.Extensions;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Structure representing a material texture.
    /// </summary>
    public struct MaterialTexture
    {
        /// <summary>
        /// Array of all material texture types.
        /// </summary>
        public static readonly MaterialTextureType[] TextureTypes = Enum.GetValues<MaterialTextureType>();

        /// <summary>
        /// Array of names corresponding to material texture types.
        /// </summary>
        public static readonly string[] TextureTypeNames = Enum.GetNames(typeof(MaterialTextureType));

        /// <summary>
        /// Array of all blend modes.
        /// </summary>
        public static readonly BlendMode[] BlendModes = Enum.GetValues<BlendMode>();

        /// <summary>
        /// Array of names corresponding to blend modes.
        /// </summary>
        public static readonly string[] BlendModeNames = Enum.GetNames(typeof(BlendMode));

        /// <summary>
        /// Array of all texture operations.
        /// </summary>
        public static readonly TextureOp[] TextureOps = Enum.GetValues<TextureOp>();

        /// <summary>
        /// Array of names corresponding to texture operations.
        /// </summary>
        public static readonly string[] TextureOpNames = Enum.GetNames(typeof(TextureOp));

        /// <summary>
        /// Array of all texture map modes.
        /// </summary>
        public static readonly TextureMapMode[] TextureMapModes = Enum.GetValues<TextureMapMode>();

        /// <summary>
        /// Array of names corresponding to texture map modes.
        /// </summary>
        public static readonly string[] TextureMapModeNames = Enum.GetNames<TextureMapMode>();

        /// <summary>
        /// Type of the material texture.
        /// </summary>
        public MaterialTextureType Type;

        /// <summary>
        /// File path of the texture.
        /// </summary>
        public Guid File;

        /// <summary>
        /// Blend mode of the texture.
        /// </summary>
        public BlendMode Blend;

        /// <summary>
        /// Texture operation.
        /// </summary>
        public TextureOp Op;

        /// <summary>
        /// Mapping value.
        /// </summary>
        public int Mapping;

        /// <summary>
        /// UVW source.
        /// </summary>
        public int UVWSrc;

        /// <summary>
        /// U-axis texture map mode.
        /// </summary>
        public TextureMapMode U;

        /// <summary>
        /// V-axis texture map mode.
        /// </summary>
        public TextureMapMode V;

        /// <summary>
        /// Texture flags.
        /// </summary>
        public TextureFlags Flags;

        /// <summary>
        /// Gets the name of the material texture.
        /// </summary>
        public readonly string Name => Type.ToString();

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialTexture"/> struct.
        /// </summary>
        public MaterialTexture(MaterialTextureType type, Guid file, BlendMode blend, TextureOp op, int mapping, int uVWSrc, TextureMapMode u, TextureMapMode v, TextureFlags flags)
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

        /// <summary>
        /// Reads a <see cref="MaterialTexture"/> from the provided <see cref="Stream"/> using the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="encoding">The encoding used for reading strings.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        /// <returns>The read <see cref="MaterialTexture"/>.</returns>
        public static MaterialTexture Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            MaterialTexture data = new();
            data.Type = (MaterialTextureType)stream.ReadInt32(endianness);
            data.File = stream.ReadGuid(endianness);
            data.Blend = (BlendMode)stream.ReadInt32(endianness);
            data.Op = (TextureOp)stream.ReadInt32(endianness);
            data.Mapping = stream.ReadInt32(endianness);
            data.UVWSrc = stream.ReadInt32(endianness);
            data.U = (TextureMapMode)stream.ReadInt32(endianness);
            data.V = (TextureMapMode)stream.ReadInt32(endianness);
            data.Flags = (TextureFlags)stream.ReadInt32(endianness);
            return data;
        }

        /// <summary>
        /// Writes the <see cref="MaterialTexture"/> to the provided <see cref="Stream"/> using the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="encoding">The encoding used for writing strings.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        public readonly void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            stream.WriteInt32((int)Type, endianness);
            stream.WriteGuid(File, endianness);
            stream.WriteInt32((int)Blend, endianness);
            stream.WriteInt32((int)Op, endianness);
            stream.WriteInt32(Mapping, endianness);
            stream.WriteInt32(UVWSrc, endianness);
            stream.WriteInt32((int)U, endianness);
            stream.WriteInt32((int)V, endianness);
            stream.WriteInt32((int)Flags, endianness);
        }

        /// <summary>
        /// Gets the <see cref="SamplerStateDescription"/> based on the <see cref="MaterialTexture"/> properties.
        /// </summary>
        /// <returns>The <see cref="SamplerStateDescription"/> for the material texture.</returns>
        public readonly SamplerStateDescription GetSamplerDesc()
        {
            return new(Filter.Anisotropic, Convert(U), Convert(V), TextureAddressMode.Clamp, 0, 16, ComparisonFunction.Never, default, 0, int.MaxValue);
        }

        /// <summary>
        /// Gets a <see cref="ShaderMacro"/> based on the <see cref="MaterialTexture"/> properties.
        /// </summary>
        /// <returns>The <see cref="ShaderMacro"/> for the material texture.</returns>
        public readonly ShaderMacro AsShaderMacro()
        {
            var type = Type;

            return new($"Has{type}Tex", (File != Guid.Empty).ToHLSL());
        }

        /// <summary>
        /// Converts the <see cref="TextureMapMode"/> to <see cref="TextureAddressMode"/>.
        /// </summary>
        /// <param name="mode">The <see cref="TextureMapMode"/> to convert.</param>
        /// <returns>The equivalent <see cref="TextureAddressMode"/>.</returns>
        public static TextureAddressMode Convert(TextureMapMode mode)
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

        /// <summary>
        /// Converts a texture address mode to a corresponding texture map mode.
        /// </summary>
        /// <param name="mode">The texture address mode to convert.</param>
        /// <returns>The converted texture map mode.</returns>
        public static TextureMapMode Convert(TextureAddressMode mode)
        {
            return mode switch
            {
                TextureAddressMode.Wrap => TextureMapMode.Wrap,
                TextureAddressMode.Clamp => TextureMapMode.Clamp,
                TextureAddressMode.Mirror => TextureMapMode.Mirror,
                _ => throw new NotSupportedException(),
            };
        }

        /// <summary>
        /// Converts the <see cref="MaterialTexture"/> to its string representation.
        /// </summary>
        public override readonly string ToString()
        {
            return $"{Type}, {File}, {Blend}, {Op}, {Mapping}, {UVWSrc}, {U}, {V}, {Flags}";
        }

        /// <summary>
        /// Gets an enumerable of name aliases for the material texture.
        /// </summary>
        public readonly IEnumerable<string> GetNameAlias()
        {
            yield return Name;

            if (Type == MaterialTextureType.Diffuse)
                yield return MaterialTextureType.BaseColor.ToString();
        }

        /// <summary>
        /// Deep clones a <see cref="MaterialTexture"/> instance.
        /// </summary>
        /// <returns>The deep cloned <see cref="MaterialTexture"/> instance.</returns>
        public readonly MaterialTexture Clone()
        {
            MaterialTexture texture = default;
            texture.Type = Type;
            texture.File = File;
            texture.Blend = Blend;
            texture.Op = Op;
            texture.Mapping = Mapping;
            texture.UVWSrc = UVWSrc;
            texture.U = U;
            texture.V = V;
            texture.Flags = Flags;
            return texture;
        }
    }
}