namespace HexaEngine.Core.Graphics
{
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;

    /// <summary>
    /// Describes the rasterizer state used in the graphics pipeline to control how geometry is rendered.
    /// </summary>
    public struct RasterizerDescription : IEquatable<RasterizerDescription>
    {
        /// <summary>
        /// The default depth bias value.
        /// </summary>
        public const int DefaultDepthBias = unchecked(0);

        /// <summary>
        /// The default depth bias clamp value.
        /// </summary>
        public const float DefaultDepthBiasClamp = unchecked((float)0.0F);

        /// <summary>
        /// The default slope-scaled depth bias value.
        /// </summary>
        public const float DefaultSlopeScaledDepthBias = unchecked((float)0.0F);

        /// <summary>
        /// Gets or sets the fill mode for rendering polygons.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(FillMode.Solid)]
        public FillMode FillMode = FillMode.Solid;

        /// <summary>
        /// Gets or sets the cull mode for determining which triangles to cull.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(CullMode.Back)]
        public CullMode CullMode = CullMode.Back;

        /// <summary>
        /// Gets or sets a value indicating whether the triangles have a front counter-clockwise winding order.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(false)]
        public bool FrontCounterClockwise;

        /// <summary>
        /// Gets or sets the depth bias value.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(DefaultDepthBias)]
        public int DepthBias;

        /// <summary>
        /// Gets or sets the depth bias clamp value.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(DefaultDepthBiasClamp)]
        public float DepthBiasClamp;

        /// <summary>
        /// Gets or sets the slope-scaled depth bias value.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(DefaultSlopeScaledDepthBias)]
        public float SlopeScaledDepthBias;

        /// <summary>
        /// Gets or sets a value indicating whether depth clipping is enabled.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(true)]
        public bool DepthClipEnable;

        /// <summary>
        /// Gets or sets a value indicating whether scissor testing is enabled.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(false)]
        public bool ScissorEnable;

        /// <summary>
        /// Gets or sets a value indicating whether multisample anti-aliasing is enabled.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(true)]
        public bool MultisampleEnable;

        /// <summary>
        /// Gets or sets a value indicating whether antialiased lines are enabled.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(false)]
        public bool AntialiasedLineEnable;

        /// <summary>
        /// Gets or sets the forced sample count for a specific sample pattern.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(0)]
        public uint ForcedSampleCount;

        /// <summary>
        /// Gets or sets the mode for conservative rasterization.
        /// </summary>
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

        /// <summary>
        /// A built-in description with settings for culling primitives with front-facing triangles, and applying depth bias.
        /// </summary>
        public static readonly RasterizerDescription CullFrontDepthBias = new(CullMode.Front, FillMode.Solid, false, -1, 0, 1.0f, true, false, false, false);

        /// <summary>
        /// Initializes a new instance of the <see cref="RasterizerDescription"/> class.
        /// </summary>
        /// <param name="cullMode">A <see cref="CullMode"/>A value that specifies that triangles facing the specified direction are not drawn.</param>
        /// <param name="fillMode">A <see cref="FillMode"/>A value that specifies the fill mode to use when rendering.</param>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="RasterizerDescription"/> class.
        /// </summary>
        /// <param name="cullMode">A <see cref="CullMode"/> value that specifies which triangles should be culled.</param>
        /// <param name="fillMode">A <see cref="FillMode"/> value that specifies how triangles should be filled.</param>
        /// <param name="frontCounterClockwise">True if front-facing triangles are defined in a counter-clockwise (CCW) order; otherwise, false.</param>
        /// <param name="depthBias">A depth value added to a given pixel before rendering to adjust for precision errors.</param>
        /// <param name="depthBiasClamp">The maximum depth bias value.</param>
        /// <param name="slopeScaledDepthBias">The scale factor applied to the depth bias value.</param>
        /// <param name="depthClipEnable">True to enable depth clipping; otherwise, false.</param>
        /// <param name="scissorEnable">True to enable scissor testing; otherwise, false.</param>
        /// <param name="multisampleEnable">True to enable multisampling; otherwise, false.</param>
        /// <param name="antialiasedLineEnable">True to enable antialiased line rendering; otherwise, false.</param>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="RasterizerDescription"/> class.
        /// </summary>
        /// <param name="cullMode">A <see cref="CullMode"/> value that specifies which triangles should be culled.</param>
        /// <param name="fillMode">A <see cref="FillMode"/> value that specifies how triangles should be filled.</param>
        /// <param name="frontCounterClockwise">True if front-facing triangles are defined in a counter-clockwise (CCW) order; otherwise, false.</param>
        /// <param name="depthBias">A depth value added to a given pixel before rendering to adjust for precision errors.</param>
        /// <param name="depthBiasClamp">The maximum depth bias value.</param>
        /// <param name="slopeScaledDepthBias">The scale factor applied to the depth bias value.</param>
        /// <param name="depthClipEnable">True to enable depth clipping; otherwise, false.</param>
        /// <param name="scissorEnable">True to enable scissor testing; otherwise, false.</param>
        /// <param name="multisampleEnable">True to enable multisampling; otherwise, false.</param>
        /// <param name="antialiasedLineEnable">True to enable antialiased line rendering; otherwise, false.</param>
        /// <param name="forcedSampleCount">The number of samples to force when in multisampling mode.</param>
        /// <param name="conservativeRasterization">A value specifying the conservative rasterization mode.</param>
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

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj)
        {
            return obj is RasterizerDescription description && Equals(description);
        }

        /// <inheritdoc/>
        public readonly bool Equals(RasterizerDescription other)
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

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            HashCode hash = new();
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

        /// <summary>
        /// Determines whether two <see cref="RasterizerDescription"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="RasterizerDescription"/> to compare.</param>
        /// <param name="right">The second <see cref="RasterizerDescription"/> to compare.</param>
        /// <returns><c>true</c> if the objects are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(RasterizerDescription left, RasterizerDescription right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="RasterizerDescription"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="RasterizerDescription"/> to compare.</param>
        /// <param name="right">The second <see cref="RasterizerDescription"/> to compare.</param>
        /// <returns><c>true</c> if the objects are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(RasterizerDescription left, RasterizerDescription right)
        {
            return !(left == right);
        }
    }
}