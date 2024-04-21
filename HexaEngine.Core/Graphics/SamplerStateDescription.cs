namespace HexaEngine.Core.Graphics
{
    using System;
    using System.ComponentModel;
    using System.Numerics;
    using System.Xml.Serialization;

    /// <summary>
    /// Describes the properties of a sampler state in a graphics API or framework.
    /// </summary>
    public partial struct SamplerStateDescription : IEquatable<SamplerStateDescription>
    {
        /// <summary>
        /// The maximum allowed value for <see cref="MaxAnisotropy"/>.
        /// </summary>
        public const int MaxMaxAnisotropy = unchecked(16);

        /// <summary>
        /// Gets or sets the filtering method to use.
        /// </summary>
        [XmlAttribute]
        public Filter Filter = Filter.MinMagMipPoint;

        /// <summary>
        /// Gets or sets the texture addressing mode for the U coordinate axis.
        /// </summary>
        [XmlAttribute]
        public TextureAddressMode AddressU = TextureAddressMode.Wrap;

        /// <summary>
        /// Gets or sets the texture addressing mode for the V coordinate axis.
        /// </summary>
        [XmlAttribute]
        public TextureAddressMode AddressV = TextureAddressMode.Wrap;

        /// <summary>
        /// Gets or sets the texture addressing mode for the W coordinate axis.
        /// </summary>
        [XmlAttribute]
        public TextureAddressMode AddressW = TextureAddressMode.Wrap;

        /// <summary>
        /// Gets or sets the bias to apply to mip level calculations.
        /// </summary>
        [DefaultValue(0.0f)]
        [XmlAttribute]
        public float MipLODBias = 0;

        /// <summary>
        /// Gets or sets the maximum anisotropy value.
        /// </summary>
        [DefaultValue(0)]
        [XmlAttribute]
        public int MaxAnisotropy = 0;

        /// <summary>
        /// Gets or sets the comparison function to use when comparing sampled data.
        /// </summary>
        [DefaultValue(ComparisonFunction.Never)]
        [XmlAttribute]
        public ComparisonFunction ComparisonFunction = ComparisonFunction.Never;

        /// <summary>
        /// Gets or sets the border color for texture addressing mode <see cref="TextureAddressMode.Border"/>.
        /// </summary>
        public Vector4 BorderColor = Vector4.Zero;

        /// <summary>
        /// Gets or sets the minimum level-of-detail value.
        /// </summary>
        [DefaultValue(float.MinValue)]
        [XmlAttribute]
        public float MinLOD = float.MinValue;

        /// <summary>
        /// Gets or sets the maximum level-of-detail value.
        /// </summary>
        [DefaultValue(float.MaxValue)]
        [XmlAttribute]
        public float MaxLOD = float.MaxValue;

        /// <summary>
        /// Predefined instance for point sampling with wrapping.
        /// </summary>
        public static readonly SamplerStateDescription PointWrap = new(Filter.MinMagMipPoint, TextureAddressMode.Wrap);

        /// <summary>
        /// Predefined instance for point sampling with clamping.
        /// </summary>
        public static readonly SamplerStateDescription PointClamp = new(Filter.MinMagMipPoint, TextureAddressMode.Clamp);

        /// <summary>
        /// Predefined instance for linear sampling with mirror.
        /// </summary>
        public static readonly SamplerStateDescription LinearMirror = new(Filter.MinMagMipLinear, TextureAddressMode.Mirror);

        /// <summary>
        /// Predefined instance for linear sampling with wrapping.
        /// </summary>
        public static readonly SamplerStateDescription LinearWrap = new(Filter.MinMagMipLinear, TextureAddressMode.Wrap);

        /// <summary>
        /// Predefined instance for linear sampling with clamping.
        /// </summary>
        public static readonly SamplerStateDescription LinearClamp = new(Filter.MinMagMipLinear, TextureAddressMode.Clamp);

        /// <summary>
        /// Predefined instance for linear sampling with a border color.
        /// </summary>
        public static readonly SamplerStateDescription LinearBorder = new(Filter.MinMagMipLinear, TextureAddressMode.Border) { BorderColor = default };

        /// <summary>
        /// Predefined instance for anisotropic sampling with wrapping.
        /// </summary>
        public static readonly SamplerStateDescription AnisotropicWrap = new(Filter.Anisotropic, TextureAddressMode.Wrap, 0.0f, MaxMaxAnisotropy);

        /// <summary>
        /// Predefined instance for anisotropic sampling with clamping.
        /// </summary>
        public static readonly SamplerStateDescription AnisotropicClamp = new(Filter.Anisotropic, TextureAddressMode.Clamp, 0.0f, MaxMaxAnisotropy);

        /// <summary>
        /// Predefined instance for linear sampling with a border color and comparison function.
        /// </summary>
        public static readonly SamplerStateDescription ComparisonLinearBorder = new(Filter.ComparisonMinMagMipLinear, TextureAddressMode.Border, 0, 0, ComparisonFunction.LessEqual, 0, float.MaxValue);

        /// <summary>
        /// Initializes a new instance of the <see cref="SamplerStateDescription"/> struct.
        /// </summary>
        public SamplerStateDescription()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SamplerStateDescription"/> struct.
        /// </summary>
        /// <param name="filter">Filtering method to use when sampling a texture.</param>
        /// <param name="addressU">Method to use for resolving a u texture coordinate that is outside the 0 to 1 range.</param>
        /// <param name="addressV">Method to use for resolving a v texture coordinate that is outside the 0 to 1 range.</param>
        /// <param name="addressW">Method to use for resolving a w texture coordinate that is outside the 0 to 1 range.</param>
        /// <param name="mipLODBias">Offset from the calculated mipmap level.</param>
        /// <param name="maxAnisotropy">Clamping _value used if <see cref="Filter.Anisotropic"/> or <see cref="Filter.ComparisonAnisotropic"/> is specified in Filter. Valid values are between 1 and 16.</param>
        /// <param name="comparisonFunction">A function that compares sampled data against existing sampled data. </param>
        /// <param name="borderColor">Border color to use if <see cref="TextureAddressMode.Border"/> is specified for AddressU, AddressV, or AddressW.</param>
        /// <param name="minLOD">Lower end of the mipmap range to clamp access to, where 0 is the largest and most detailed mipmap level and any level higher than that is less detailed.</param>
        /// <param name="maxLOD">Upper end of the mipmap range to clamp access to, where 0 is the largest and most detailed mipmap level and any level higher than that is less detailed. This _value must be greater than or equal to MinLOD. </param>
        public SamplerStateDescription(
            Filter filter,
            TextureAddressMode addressU,
            TextureAddressMode addressV,
            TextureAddressMode addressW,
            float mipLODBias,
            int maxAnisotropy,
            ComparisonFunction comparisonFunction,
            Vector4 borderColor,
            float minLOD,
            float maxLOD)
        {
            Filter = filter;
            AddressU = addressU;
            AddressV = addressV;
            AddressW = addressW;
            MipLODBias = mipLODBias;
            MaxAnisotropy = maxAnisotropy;
            ComparisonFunction = comparisonFunction;
            BorderColor = borderColor;
            MinLOD = minLOD;
            MaxLOD = maxLOD;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SamplerStateDescription"/> struct.
        /// </summary>
        /// <param name="filter">Filtering method to use when sampling a texture.</param>
        /// <param name="addressU">Method to use for resolving a u texture coordinate that is outside the 0 to 1 range.</param>
        /// <param name="addressV">Method to use for resolving a v texture coordinate that is outside the 0 to 1 range.</param>
        /// <param name="addressW">Method to use for resolving a w texture coordinate that is outside the 0 to 1 range.</param>
        /// <param name="mipLODBias">Offset from the calculated mipmap level.</param>
        /// <param name="maxAnisotropy">Clamping _value used if <see cref="Filter.Anisotropic"/> or <see cref="Filter.ComparisonAnisotropic"/> is specified in Filter. Valid values are between 1 and 16.</param>
        /// <param name="comparisonFunction">A function that compares sampled data against existing sampled data. </param>
        /// <param name="minLOD">Lower end of the mipmap range to clamp access to, where 0 is the largest and most detailed mipmap level and any level higher than that is less detailed.</param>
        /// <param name="maxLOD">Upper end of the mipmap range to clamp access to, where 0 is the largest and most detailed mipmap level and any level higher than that is less detailed. This _value must be greater than or equal to MinLOD. </param>
        public SamplerStateDescription(
            Filter filter,
            TextureAddressMode addressU,
            TextureAddressMode addressV,
            TextureAddressMode addressW,
            float mipLODBias = 0.0f,
            int maxAnisotropy = 1,
            ComparisonFunction comparisonFunction = ComparisonFunction.Never,
            float minLOD = float.MinValue,
            float maxLOD = float.MaxValue)
        {
            Filter = filter;
            AddressU = addressU;
            AddressV = addressV;
            AddressW = addressW;
            MipLODBias = mipLODBias;
            MaxAnisotropy = maxAnisotropy;
            ComparisonFunction = comparisonFunction;
            BorderColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            MinLOD = minLOD;
            MaxLOD = maxLOD;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SamplerStateDescription"/> struct.
        /// </summary>
        /// <param name="filter">Filtering method to use when sampling a texture.</param>
        /// <param name="address">Method to use for resolving a u, v e w texture coordinate that is outside the 0 to 1 range.</param>
        /// <param name="mipLODBias">Offset from the calculated mipmap level.</param>
        /// <param name="maxAnisotropy">Clamping _value used if <see cref="Filter.Anisotropic"/> or <see cref="Filter.ComparisonAnisotropic"/> is specified in Filter. Valid values are between 1 and 16.</param>
        /// <param name="comparisonFunction">A function that compares sampled data against existing sampled data. </param>
        /// <param name="minLOD">Lower end of the mipmap range to clamp access to, where 0 is the largest and most detailed mipmap level and any level higher than that is less detailed.</param>
        /// <param name="maxLOD">Upper end of the mipmap range to clamp access to, where 0 is the largest and most detailed mipmap level and any level higher than that is less detailed. This _value must be greater than or equal to MinLOD. </param>
        public SamplerStateDescription(
            Filter filter,
            TextureAddressMode address,
            float mipLODBias = 0.0f,
            int maxAnisotropy = 1,
            ComparisonFunction comparisonFunction = ComparisonFunction.Never,
            float minLOD = float.MinValue,
            float maxLOD = float.MaxValue)
        {
            Filter = filter;
            AddressU = address;
            AddressV = address;
            AddressW = address;
            MipLODBias = mipLODBias;
            MaxAnisotropy = maxAnisotropy;
            ComparisonFunction = comparisonFunction;
            BorderColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            MinLOD = minLOD;
            MaxLOD = maxLOD;
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj)
        {
            return obj is SamplerStateDescription description && Equals(description);
        }

        /// <inheritdoc/>
        public readonly bool Equals(SamplerStateDescription other)
        {
            return Filter == other.Filter &&
                   AddressU == other.AddressU &&
                   AddressV == other.AddressV &&
                   AddressW == other.AddressW &&
                   MipLODBias == other.MipLODBias &&
                   MaxAnisotropy == other.MaxAnisotropy &&
                   ComparisonFunction == other.ComparisonFunction &&
                   BorderColor.Equals(other.BorderColor) &&
                   MinLOD == other.MinLOD &&
                   MaxLOD == other.MaxLOD;
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            HashCode hash = new();
            hash.Add(Filter);
            hash.Add(AddressU);
            hash.Add(AddressV);
            hash.Add(AddressW);
            hash.Add(MipLODBias);
            hash.Add(MaxAnisotropy);
            hash.Add(ComparisonFunction);
            hash.Add(BorderColor);
            hash.Add(MinLOD);
            hash.Add(MaxLOD);
            return hash.ToHashCode();
        }

        /// <summary>
        /// Determines whether two <see cref="SamplerStateDescription"/> instances are equal.
        /// </summary>
        /// <param name="left">The first <see cref="SamplerStateDescription"/> to compare.</param>
        /// <param name="right">The second <see cref="SamplerStateDescription"/> to compare.</param>
        /// <returns>
        /// <see langword="true"/> if the specified instances are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(SamplerStateDescription left, SamplerStateDescription right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="SamplerStateDescription"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="SamplerStateDescription"/> to compare.</param>
        /// <param name="right">The second <see cref="SamplerStateDescription"/> to compare.</param>
        /// <returns>
        /// <see langword="true"/> if the specified instances are not equal; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(SamplerStateDescription left, SamplerStateDescription right)
        {
            return !(left == right);
        }
    }
}