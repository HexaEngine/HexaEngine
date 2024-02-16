namespace HexaEngine.Core.Graphics
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Xml.Serialization;

    /// <summary>
    /// Defines the blending settings for a graphics pipeline.
    /// </summary>
    public struct BlendDescription : IEquatable<BlendDescription>
    {
        /// <summary>
        /// Predefined blend description for opaque rendering.
        /// </summary>
        public static readonly BlendDescription Opaque = new(Blend.One, Blend.Zero);

        /// <summary>
        /// Predefined blend description for alpha blending.
        /// </summary>
        public static readonly BlendDescription AlphaBlend = new(Blend.One, Blend.InverseSourceAlpha);

        /// <summary>
        /// Predefined blend description for additive blending.
        /// </summary>
        public static readonly BlendDescription Additive = new(Blend.SourceAlpha, Blend.One);

        /// <summary>
        /// Predefined blend description for non-premultiplied blending.
        /// </summary>
        public static readonly BlendDescription NonPremultiplied = new(Blend.SourceAlpha, Blend.InverseSourceAlpha);

        /// <summary>
        /// The number of simultaneous render targets supported.
        /// </summary>
        public const int SimultaneousRenderTargetCount = unchecked(8);

        /// <summary>
        /// Gets or sets a value indicating whether alpha-to-coverage is enabled.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(false)]
        public bool AlphaToCoverageEnable = false;

        /// <summary>
        /// Gets or sets a value indicating whether independent blend is enabled.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(false)]
        public bool IndependentBlendEnable = false;

        /// <summary>
        /// An array of render target blend descriptions.
        /// </summary>
        public RenderTargetBlendDescription[] RenderTarget = new RenderTargetBlendDescription[SimultaneousRenderTargetCount];

        /// <summary>
        /// Initializes a new instance of the <see cref="BlendDescription"/> struct.
        /// </summary>
        public BlendDescription()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlendDescription"/> struct with the specified source and destination blend modes.
        /// </summary>
        /// <param name="sourceBlend">The source blend mode.</param>
        /// <param name="destinationBlend">The destination blend mode.</param>
        public BlendDescription(Blend sourceBlend, Blend destinationBlend) : this(sourceBlend, destinationBlend, sourceBlend, destinationBlend)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlendDescription"/> struct with the specified source, destination, source alpha, and destination alpha blend modes.
        /// </summary>
        /// <param name="sourceBlend">The source blend mode.</param>
        /// <param name="destinationBlend">The destination blend mode.</param>
        /// <param name="srcBlendAlpha">The source alpha blend mode.</param>
        /// <param name="destBlendAlpha">The destination alpha blend mode.</param>
        public BlendDescription(Blend sourceBlend, Blend destinationBlend, Blend srcBlendAlpha, Blend destBlendAlpha)
            : this()
        {
            AlphaToCoverageEnable = false;
            IndependentBlendEnable = false;

            for (int i = 0; i < SimultaneousRenderTargetCount; i++)
            {
                RenderTarget[i].SourceBlend = sourceBlend;
                RenderTarget[i].DestinationBlend = destinationBlend;
                RenderTarget[i].BlendOperation = BlendOperation.Add;
                RenderTarget[i].SourceBlendAlpha = srcBlendAlpha;
                RenderTarget[i].DestinationBlendAlpha = destBlendAlpha;
                RenderTarget[i].BlendOperationAlpha = BlendOperation.Add;
                RenderTarget[i].RenderTargetWriteMask = ColorWriteEnable.All;
                RenderTarget[i].IsBlendEnabled = IsBlendEnabled(ref RenderTarget[i]);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlendDescription"/> struct with the specified source, destination, source alpha, destination alpha, blend operation, and alpha blend operation modes.
        /// </summary>
        /// <param name="sourceBlend">The source blend mode.</param>
        /// <param name="destinationBlend">The destination blend mode.</param>
        /// <param name="srcBlendAlpha">The source alpha blend mode.</param>
        /// <param name="destBlendAlpha">The destination alpha blend mode.</param>
        /// <param name="blendOperation">The blend operation mode.</param>
        /// <param name="blendOperationAlpha">The alpha blend operation mode.</param>
        public BlendDescription(Blend sourceBlend, Blend destinationBlend, Blend srcBlendAlpha, Blend destBlendAlpha, BlendOperation blendOperation, BlendOperation blendOperationAlpha)
            : this()
        {
            AlphaToCoverageEnable = false;
            IndependentBlendEnable = false;

            for (int i = 0; i < SimultaneousRenderTargetCount; i++)
            {
                RenderTarget[i].SourceBlend = sourceBlend;
                RenderTarget[i].DestinationBlend = destinationBlend;
                RenderTarget[i].BlendOperation = blendOperation;
                RenderTarget[i].SourceBlendAlpha = srcBlendAlpha;
                RenderTarget[i].DestinationBlendAlpha = destBlendAlpha;
                RenderTarget[i].BlendOperationAlpha = blendOperationAlpha;
                RenderTarget[i].RenderTargetWriteMask = ColorWriteEnable.All;
                RenderTarget[i].IsBlendEnabled = IsBlendEnabled(ref RenderTarget[i]);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlendDescription"/> struct with the specified source, destination, source alpha, destination alpha, blend operation, and alpha blend operation modes.
        /// </summary>
        /// <param name="sourceBlend">The source blend mode.</param>
        /// <param name="destinationBlend">The destination blend mode.</param>
        /// <param name="srcBlendAlpha">The source alpha blend mode.</param>
        /// <param name="destBlendAlpha">The destination alpha blend mode.</param>
        /// <param name="blendOperation">The blend operation mode.</param>
        /// <param name="blendOperationAlpha">The alpha blend operation mode.</param>
        /// <param name="logicOperation">The logic operation.</param>
        public BlendDescription(Blend sourceBlend, Blend destinationBlend, Blend srcBlendAlpha, Blend destBlendAlpha, BlendOperation blendOperation, BlendOperation blendOperationAlpha, LogicOperation logicOperation)
            : this()
        {
            AlphaToCoverageEnable = false;
            IndependentBlendEnable = false;

            for (int i = 0; i < SimultaneousRenderTargetCount; i++)
            {
                RenderTarget[i].SourceBlend = sourceBlend;
                RenderTarget[i].DestinationBlend = destinationBlend;
                RenderTarget[i].BlendOperation = blendOperation;
                RenderTarget[i].SourceBlendAlpha = srcBlendAlpha;
                RenderTarget[i].DestinationBlendAlpha = destBlendAlpha;
                RenderTarget[i].BlendOperationAlpha = blendOperationAlpha;
                RenderTarget[i].LogicOperation = logicOperation;
                RenderTarget[i].IsLogicOpEnabled = true;
                RenderTarget[i].RenderTargetWriteMask = ColorWriteEnable.All;
                RenderTarget[i].IsBlendEnabled = IsBlendEnabled(ref RenderTarget[i]);
            }
        }

        private static bool IsBlendEnabled(ref RenderTargetBlendDescription renderTarget)
        {
            return renderTarget.BlendOperationAlpha != BlendOperation.Add
                    || renderTarget.SourceBlendAlpha != Blend.One
                    || renderTarget.DestinationBlendAlpha != Blend.Zero
                    || renderTarget.BlendOperation != BlendOperation.Add
                    || renderTarget.SourceBlend != Blend.One
                    || renderTarget.DestinationBlend != Blend.Zero;
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj)
        {
            return obj is BlendDescription description && Equals(description);
        }

        /// <inheritdoc/>
        public readonly bool Equals(BlendDescription other)
        {
            return AlphaToCoverageEnable == other.AlphaToCoverageEnable &&
                   IndependentBlendEnable == other.IndependentBlendEnable &&
                   EqualityComparer<RenderTargetBlendDescription[]>.Default.Equals(RenderTarget, other.RenderTarget);
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(AlphaToCoverageEnable, IndependentBlendEnable, RenderTarget);
        }

        /// <summary>
        /// Compares two <see cref="BlendDescription"/> objects for equality.
        /// </summary>
        public static bool operator ==(BlendDescription left, BlendDescription right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two <see cref="BlendDescription"/> objects for inequality.
        /// </summary>
        public static bool operator !=(BlendDescription left, BlendDescription right)
        {
            return !(left == right);
        }
    }
}