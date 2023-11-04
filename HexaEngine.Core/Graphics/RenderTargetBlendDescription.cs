namespace HexaEngine.Core.Graphics
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;

    /// <summary>
    /// Describes the blending and logic operations used in rendering to a render target.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RenderTargetBlendDescription : IEquatable<RenderTargetBlendDescription>
    {
        /// <summary>
        /// Gets or sets whether blending is enabled for this render target.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(false)]
        public bool IsBlendEnabled;

        /// <summary>
        /// Gets or sets whether logic operations are enabled for this render target.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(false)]
        public bool IsLogicOpEnabled;

        /// <summary>
        /// Gets or sets the source blend factor for color data.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(Blend.One)]
        public Blend SourceBlend;

        /// <summary>
        /// Gets or sets the destination blend factor for color data.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(Blend.Zero)]
        public Blend DestinationBlend;

        /// <summary>
        /// Gets or sets the blend operation for color data.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(BlendOperation.Add)]
        public BlendOperation BlendOperation;

        /// <summary>
        /// Gets or sets the source blend factor for alpha data.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(Blend.One)]
        public Blend SourceBlendAlpha;

        /// <summary>
        /// Gets or sets the destination blend factor for alpha data.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(Blend.Zero)]
        public Blend DestinationBlendAlpha;

        /// <summary>
        /// Gets or sets the blend operation for alpha data.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(BlendOperation.Add)]
        public BlendOperation BlendOperationAlpha;

        /// <summary>
        /// Gets or sets the logic operation used for rendering to the render target.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(LogicOperation.Clear)]
        public LogicOperation LogicOperation;

        /// <summary>
        /// Gets or sets the write mask that enables or disables writing of individual color channels.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(ColorWriteEnable.All)]
        public ColorWriteEnable RenderTargetWriteMask;

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj)
        {
            return obj is RenderTargetBlendDescription description && Equals(description);
        }

        /// <inheritdoc/>
        public readonly bool Equals(RenderTargetBlendDescription other)
        {
            return IsBlendEnabled == other.IsBlendEnabled &&
                   IsLogicOpEnabled == other.IsLogicOpEnabled &&
                   SourceBlend == other.SourceBlend &&
                   DestinationBlend == other.DestinationBlend &&
                   BlendOperation == other.BlendOperation &&
                   SourceBlendAlpha == other.SourceBlendAlpha &&
                   DestinationBlendAlpha == other.DestinationBlendAlpha &&
                   BlendOperationAlpha == other.BlendOperationAlpha &&
                   LogicOperation == other.LogicOperation &&
                   RenderTargetWriteMask == other.RenderTargetWriteMask;
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            HashCode hash = new();
            hash.Add(IsBlendEnabled);
            hash.Add(IsLogicOpEnabled);
            hash.Add(SourceBlend);
            hash.Add(DestinationBlend);
            hash.Add(BlendOperation);
            hash.Add(SourceBlendAlpha);
            hash.Add(DestinationBlendAlpha);
            hash.Add(BlendOperationAlpha);
            hash.Add(LogicOperation);
            hash.Add(RenderTargetWriteMask);
            return hash.ToHashCode();
        }

        /// <summary>
        /// Determines whether two <see cref="RenderTargetBlendDescription"/> instances are equal.
        /// </summary>
        public static bool operator ==(RenderTargetBlendDescription left, RenderTargetBlendDescription right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="RenderTargetBlendDescription"/> instances are not equal.
        /// </summary>
        public static bool operator !=(RenderTargetBlendDescription left, RenderTargetBlendDescription right)
        {
            return !(left == right);
        }
    }
}