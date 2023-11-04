namespace HexaEngine.Core.Graphics
{
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;

    public struct RasterizerDescription : IEquatable<RasterizerDescription>
    {
        public const int DefaultDepthBias = unchecked(0);
        public const float DefaultDepthBiasClamp = unchecked((float)0.0F);
        public const float DefaultSlopeScaledDepthBias = unchecked((float)0.0F);

        [XmlAttribute]
        [DefaultValue(FillMode.Solid)]
        public FillMode FillMode = FillMode.Solid;

        [XmlAttribute]
        [DefaultValue(CullMode.Back)]
        public CullMode CullMode = CullMode.Back;

        [XmlAttribute]
        [DefaultValue(false)]
        public bool FrontCounterClockwise;

        [XmlAttribute]
        [DefaultValue(DefaultDepthBias)]
        public int DepthBias;

        [XmlAttribute]
        [DefaultValue(DefaultDepthBiasClamp)]
        public float DepthBiasClamp;

        [XmlAttribute]
        [DefaultValue(DefaultSlopeScaledDepthBias)]
        public float SlopeScaledDepthBias;

        [XmlAttribute]
        [DefaultValue(true)]
        public bool DepthClipEnable;

        [XmlAttribute]
        [DefaultValue(false)]
        public bool ScissorEnable;

        [XmlAttribute]
        [DefaultValue(true)]
        public bool MultisampleEnable;

        [XmlAttribute]
        [DefaultValue(false)]
        public bool AntialiasedLineEnable;

        [XmlAttribute]
        [DefaultValue(0)]
        public uint ForcedSampleCount;

        [XmlAttribute]
        [DefaultValue(ConservativeRasterizationMode.Off)]
        public ConservativeRasterizationMode ConservativeRaster;

        /// <summary>
        /// A built-in description with settings with settings for not culling any primitives.
        /// </summary>
        public static readonly RasterizerDescription CullNone = new(CullMode.None, FillMode.Solid);

        /// <summary>
        /// A built-in description with settings for culling primitives with clockwise winding order.
        /// </summary>
        public static readonly RasterizerDescription CullFront = new(CullMode.Front, FillMode.Solid);

        /// <summary>
        /// A built-in description with settings for culling primitives with counter-clockwise winding order.
        /// </summary>
        public static readonly RasterizerDescription CullBack = new(CullMode.Back, FillMode.Solid);

        /// <summary>
        /// A built-in description with settings for not culling any primitives and wireframe fill mode.
        /// </summary>
        public static readonly RasterizerDescription Wireframe = new(CullMode.None, FillMode.Wireframe);

        public static readonly RasterizerDescription CullFrontDepthBias = new(CullMode.Front, FillMode.Solid, false, -1, 0, 1.0f, true, false, false, false);

        /// <summary>
        /// Initializes a new instance of the <see cref="RasterizerDescription"/> class.
        /// </summary>
        /// <param name="cullMode">A <see cref="CullMode"/> _value that specifies that triangles facing the specified direction are not drawn..</param>
        /// <param name="fillMode">A <see cref="FillMode"/> _value that specifies the fill mode to use when rendering.</param>
        public RasterizerDescription(CullMode cullMode, FillMode fillMode)
        {
            CullMode = cullMode;
            FillMode = fillMode;
            FrontCounterClockwise = false;
            DepthBias = DefaultDepthBias;
            DepthBiasClamp = DefaultDepthBiasClamp;
            SlopeScaledDepthBias = DefaultSlopeScaledDepthBias;
            DepthClipEnable = true;
            ScissorEnable = false;
            MultisampleEnable = true;
            AntialiasedLineEnable = false;
            ForcedSampleCount = 0;
            ConservativeRaster = ConservativeRasterizationMode.Off;
        }

        public RasterizerDescription(CullMode cullMode, FillMode fillMode, bool frontCounterClockwise, int depthBias, float depthBiasClamp, float slopeScaledDepthBias, bool depthClipEnable, bool scissorEnable, bool multisampleEnable, bool antialiasedLineEnable)
        {
            FillMode = fillMode;
            CullMode = cullMode;
            FrontCounterClockwise = frontCounterClockwise;
            DepthBias = depthBias;
            DepthBiasClamp = depthBiasClamp;
            SlopeScaledDepthBias = slopeScaledDepthBias;
            DepthClipEnable = depthClipEnable;
            ScissorEnable = scissorEnable;
            MultisampleEnable = multisampleEnable;
            AntialiasedLineEnable = antialiasedLineEnable;
            ForcedSampleCount = 0;
            ConservativeRaster = ConservativeRasterizationMode.Off;
        }

        public RasterizerDescription(CullMode cullMode, FillMode fillMode, bool frontCounterClockwise, int depthBias, float depthBiasClamp, float slopeScaledDepthBias, bool depthClipEnable, bool scissorEnable, bool multisampleEnable, bool antialiasedLineEnable, uint forcedSampleCount, ConservativeRasterizationMode conservativeRasterization)
        {
            FillMode = fillMode;
            CullMode = cullMode;
            FrontCounterClockwise = frontCounterClockwise;
            DepthBias = depthBias;
            DepthBiasClamp = depthBiasClamp;
            SlopeScaledDepthBias = slopeScaledDepthBias;
            DepthClipEnable = depthClipEnable;
            ScissorEnable = scissorEnable;
            MultisampleEnable = multisampleEnable;
            AntialiasedLineEnable = antialiasedLineEnable;
            ForcedSampleCount = forcedSampleCount;
            ConservativeRaster = conservativeRasterization;
        }

        public override bool Equals(object? obj)
        {
            return obj is RasterizerDescription description && Equals(description);
        }

        public bool Equals(RasterizerDescription other)
        {
            return FillMode == other.FillMode &&
                   CullMode == other.CullMode &&
                   FrontCounterClockwise == other.FrontCounterClockwise &&
                   DepthBias == other.DepthBias &&
                   DepthBiasClamp == other.DepthBiasClamp &&
                   SlopeScaledDepthBias == other.SlopeScaledDepthBias &&
                   DepthClipEnable == other.DepthClipEnable &&
                   ScissorEnable == other.ScissorEnable &&
                   MultisampleEnable == other.MultisampleEnable &&
                   AntialiasedLineEnable == other.AntialiasedLineEnable &&
                   ForcedSampleCount == other.ForcedSampleCount &&
                   ConservativeRaster == other.ConservativeRaster;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(FillMode);
            hash.Add(CullMode);
            hash.Add(FrontCounterClockwise);
            hash.Add(DepthBias);
            hash.Add(DepthBiasClamp);
            hash.Add(SlopeScaledDepthBias);
            hash.Add(DepthClipEnable);
            hash.Add(ScissorEnable);
            hash.Add(MultisampleEnable);
            hash.Add(AntialiasedLineEnable);
            hash.Add(ForcedSampleCount);
            hash.Add(ConservativeRaster);
            return hash.ToHashCode();
        }

        public static bool operator ==(RasterizerDescription left, RasterizerDescription right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RasterizerDescription left, RasterizerDescription right)
        {
            return !(left == right);
        }
    }
}