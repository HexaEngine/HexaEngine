﻿namespace HexaEngine.Core.Graphics
{
    using System.Numerics;

    public partial struct SamplerDescription
    {
        public const int MaxMaxAnisotropy = unchecked(16);
        public Filter Filter;
        public TextureAddressMode AddressU;
        public TextureAddressMode AddressV;
        public TextureAddressMode AddressW;
        public float MipLODBias;
        public int MaxAnisotropy;
        public ComparisonFunction ComparisonFunction;
        public Vector4 BorderColor;
        public float MinLOD;
        public float MaxLOD;

        public static readonly SamplerDescription PointWrap = new(Filter.MinMagMipPoint, TextureAddressMode.Wrap);
        public static readonly SamplerDescription PointClamp = new(Filter.MinMagMipPoint, TextureAddressMode.Clamp);

        public static readonly SamplerDescription LinearWrap = new(Filter.MinMagMipLinear, TextureAddressMode.Wrap);
        public static readonly SamplerDescription LinearClamp = new(Filter.MinMagMipLinear, TextureAddressMode.Clamp);

        public static readonly SamplerDescription AnisotropicWrap = new(Filter.Anisotropic, TextureAddressMode.Wrap, 0.0f, MaxMaxAnisotropy);
        public static readonly SamplerDescription AnisotropicClamp = new(Filter.Anisotropic, TextureAddressMode.Clamp, 0.0f, MaxMaxAnisotropy);

        /// <summary>
        /// Initializes a new instance of the <see cref="SamplerDescription"/> struct.
        /// </summary>
        /// <param name="filter">Filtering method to use when sampling a texture.</param>
        /// <param name="addressU">Method to use for resolving a u texture coordinate that is outside the 0 to 1 range.</param>
        /// <param name="addressV">Method to use for resolving a v texture coordinate that is outside the 0 to 1 range.</param>
        /// <param name="addressW">Method to use for resolving a w texture coordinate that is outside the 0 to 1 range.</param>
        /// <param name="mipLODBias">Offset from the calculated mipmap level.</param>
        /// <param name="maxAnisotropy">Clamping value used if <see cref="Filter.Anisotropic"/> or <see cref="Filter.ComparisonAnisotropic"/> is specified in Filter. Valid values are between 1 and 16.</param>
        /// <param name="comparisonFunction">A function that compares sampled data against existing sampled data. </param>
        /// <param name="borderColor">Border color to use if <see cref="TextureAddressMode.Border"/> is specified for AddressU, AddressV, or AddressW.</param>
        /// <param name="minLOD">Lower end of the mipmap range to clamp access to, where 0 is the largest and most detailed mipmap level and any level higher than that is less detailed.</param>
        /// <param name="maxLOD">Upper end of the mipmap range to clamp access to, where 0 is the largest and most detailed mipmap level and any level higher than that is less detailed. This value must be greater than or equal to MinLOD. </param>
        public SamplerDescription(
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
        /// Initializes a new instance of the <see cref="SamplerDescription"/> struct.
        /// </summary>
        /// <param name="filter">Filtering method to use when sampling a texture.</param>
        /// <param name="addressU">Method to use for resolving a u texture coordinate that is outside the 0 to 1 range.</param>
        /// <param name="addressV">Method to use for resolving a v texture coordinate that is outside the 0 to 1 range.</param>
        /// <param name="addressW">Method to use for resolving a w texture coordinate that is outside the 0 to 1 range.</param>
        /// <param name="mipLODBias">Offset from the calculated mipmap level.</param>
        /// <param name="maxAnisotropy">Clamping value used if <see cref="Filter.Anisotropic"/> or <see cref="Filter.ComparisonAnisotropic"/> is specified in Filter. Valid values are between 1 and 16.</param>
        /// <param name="comparisonFunction">A function that compares sampled data against existing sampled data. </param>
        /// <param name="minLOD">Lower end of the mipmap range to clamp access to, where 0 is the largest and most detailed mipmap level and any level higher than that is less detailed.</param>
        /// <param name="maxLOD">Upper end of the mipmap range to clamp access to, where 0 is the largest and most detailed mipmap level and any level higher than that is less detailed. This value must be greater than or equal to MinLOD. </param>
        public SamplerDescription(
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
        /// Initializes a new instance of the <see cref="SamplerDescription"/> struct.
        /// </summary>
        /// <param name="filter">Filtering method to use when sampling a texture.</param>
        /// <param name="address">Method to use for resolving a u, v e w texture coordinate that is outside the 0 to 1 range.</param>
        /// <param name="mipLODBias">Offset from the calculated mipmap level.</param>
        /// <param name="maxAnisotropy">Clamping value used if <see cref="Filter.Anisotropic"/> or <see cref="Filter.ComparisonAnisotropic"/> is specified in Filter. Valid values are between 1 and 16.</param>
        /// <param name="comparisonFunction">A function that compares sampled data against existing sampled data. </param>
        /// <param name="minLOD">Lower end of the mipmap range to clamp access to, where 0 is the largest and most detailed mipmap level and any level higher than that is less detailed.</param>
        /// <param name="maxLOD">Upper end of the mipmap range to clamp access to, where 0 is the largest and most detailed mipmap level and any level higher than that is less detailed. This value must be greater than or equal to MinLOD. </param>
        public SamplerDescription(
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
    }
}