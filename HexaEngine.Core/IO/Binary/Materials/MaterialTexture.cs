namespace HexaEngine.Core.IO.Binary.Materials
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Extensions;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using System.Collections.Generic;
    using System.Numerics;
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
        /// Gets the name of the material texture.
        /// </summary>
        public string Name;

        /// <summary>
        /// Type of the material texture.
        /// </summary>
        public MaterialTextureType Type;

        /// <summary>
        /// File path of the texture.
        /// </summary>
        public Guid File;

        /// <summary>
        /// UVW source.
        /// </summary>
        public int UVWSrc;

        /// <summary>
        /// Texture filter mode.
        /// </summary>
        public TextureMapFilter Filter;

        /// <summary>
        /// U-axis texture map mode.
        /// </summary>
        public TextureMapMode U;

        /// <summary>
        /// V-axis texture map mode.
        /// </summary>
        public TextureMapMode V;

        /// <summary>
        /// W-axis texture map mode.
        /// </summary>
        public TextureMapMode W;

        /// <summary>
        /// Gets or sets the bias to apply to mip level calculations.
        /// </summary>
        public float MipLODBias = 0.0f;

        /// <summary>
        /// Gets or sets the maximum anisotropy value.
        /// </summary>
        public int MaxAnisotropy = MaxMaxAnisotropy;

        /// <summary>
        /// Gets or sets the border color for texture addressing mode <see cref="TextureMapMode.Border"/>.
        /// </summary>
        public Vector4 BorderColor = Vector4.Zero;

        /// <summary>
        /// Gets or sets the minimum level-of-detail value.
        /// </summary>
        public float MinLOD = float.MinValue;

        /// <summary>
        /// Gets or sets the maximum level-of-detail value.
        /// </summary>
        public float MaxLOD = float.MaxValue;

        /// <summary>
        /// Texture flags.
        /// </summary>
        public TextureFlags Flags;

        public const int MaxMaxAnisotropy = unchecked(16);

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialTexture"/> struct.
        /// </summary>
        public MaterialTexture(string name, MaterialTextureType type, Guid file, int uVWSrc, TextureMapFilter filter, TextureMapMode u, TextureMapMode v, TextureMapMode w, float mipLODBias, int maxAnisotropy, Vector4 borderColor, float minLOD, float maxLOD, TextureFlags flags)
        {
            Name = name;
            Type = type;
            File = file;
            UVWSrc = uVWSrc;
            Filter = filter;
            U = u;
            V = v;
            W = w;
            MipLODBias = mipLODBias;
            MaxAnisotropy = maxAnisotropy;
            BorderColor = borderColor;
            MinLOD = minLOD;
            MaxLOD = maxLOD;
            Flags = flags;
        }

        /// <summary>
        /// Reads a <see cref="MaterialTexture"/> from the provided <see cref="Stream"/> using the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="encoding">The encoding used for reading strings.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        /// <param name="version">The sub version of the MaterialTexture sub type.</param>
        /// <returns>The read <see cref="MaterialTexture"/>.</returns>
        public static MaterialTexture Read(Stream stream, Encoding encoding, Endianness endianness, Version version)
        {
            MaterialTexture data = new();

            if (version >= new Version(2, 0, 0, 0))
            {
                data.Name = stream.ReadString(encoding, endianness)!;
            }

            data.Type = (MaterialTextureType)stream.ReadInt32(endianness);

            if (version == new Version(1, 0, 0, 0))
            {
                data.Name = data.Type.ToString();
            }

            data.File = stream.ReadGuid(endianness);
            data.UVWSrc = stream.ReadInt32(endianness);
            data.Filter = (TextureMapFilter)stream.ReadInt32(endianness);
            data.U = (TextureMapMode)stream.ReadInt32(endianness);
            data.V = (TextureMapMode)stream.ReadInt32(endianness);
            data.W = (TextureMapMode)stream.ReadInt32(endianness);
            data.MipLODBias = stream.ReadFloat(endianness);
            data.MaxAnisotropy = stream.ReadInt32(endianness);
            data.BorderColor = stream.ReadVector4(endianness);
            data.MinLOD = stream.ReadFloat(endianness);
            data.MaxLOD = stream.ReadFloat(endianness);
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
            stream.WriteString(Name, encoding, endianness);
            stream.WriteInt32((int)Type, endianness);
            stream.WriteGuid(File, endianness);
            stream.WriteInt32(UVWSrc, endianness);
            stream.WriteInt32((int)Filter, endianness);
            stream.WriteInt32((int)U, endianness);
            stream.WriteInt32((int)V, endianness);
            stream.WriteInt32((int)W, endianness);
            stream.WriteFloat(MipLODBias, endianness);
            stream.WriteInt32(MaxAnisotropy, endianness);
            stream.WriteVector4(BorderColor, endianness);
            stream.WriteFloat(MinLOD, endianness);
            stream.WriteFloat(MaxLOD, endianness);
            stream.WriteInt32((int)Flags, endianness);
        }

        /// <summary>
        /// Gets the <see cref="SamplerStateDescription"/> based on the <see cref="MaterialTexture"/> properties.
        /// </summary>
        /// <returns>The <see cref="SamplerStateDescription"/> for the material texture.</returns>
        public readonly SamplerStateDescription GetSamplerDesc()
        {
            return new SamplerStateDescription((Filter)Filter, Convert(U), Convert(V), Convert(W), MipLODBias, MaxAnisotropy, ComparisonFunction.Never, BorderColor, MinLOD, MaxLOD);
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
                TextureMapMode.Border => TextureAddressMode.Border,
                TextureMapMode.MirrorOnce => TextureAddressMode.MirrorOnce,
                _ => TextureAddressMode.Clamp,
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
                TextureAddressMode.Border => TextureMapMode.Border,
                TextureAddressMode.MirrorOnce => TextureMapMode.MirrorOnce,
                _ => TextureMapMode.Clamp,
            };
        }

        /// <summary>
        /// Converts the <see cref="MaterialTexture"/> to its string representation.
        /// </summary>
        public override readonly string ToString()
        {
            return $"{Type}, {File}, {UVWSrc}, {Filter}, {U}, {V}, {W}, {Flags}";
        }

        /// <summary>
        /// Gets an enumerable of name aliases for the material texture.
        /// </summary>
        public readonly IEnumerable<string> GetNameAlias()
        {
            yield return Name;

            if (Type == MaterialTextureType.Diffuse)
            {
                yield return MaterialTextureType.BaseColor.ToString();
            }
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
            texture.UVWSrc = UVWSrc;
            texture.Filter = Filter;
            texture.U = U;
            texture.V = V;
            texture.W = W;
            texture.MipLODBias = MipLODBias;
            texture.MaxAnisotropy = MaxAnisotropy;
            texture.BorderColor = BorderColor;
            texture.MinLOD = MinLOD;
            texture.MaxLOD = MaxLOD;
            texture.Flags = Flags;
            return texture;
        }
    }
}