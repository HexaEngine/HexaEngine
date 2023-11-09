namespace HexaEngine.Core.Graphics
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Numerics;
    using System.Xml.Serialization;

    /// <summary>
    /// Represents the state of a graphics pipeline.
    /// </summary>
    public struct GraphicsPipelineState : IEquatable<GraphicsPipelineState>
    {
        /// <summary>
        /// Gets or sets the rasterizer description for the pipeline state.
        /// </summary>
        public RasterizerDescription Rasterizer;

        /// <summary>
        /// Gets or sets the depth-stencil description for the pipeline state.
        /// </summary>
        public DepthStencilDescription DepthStencil;

        /// <summary>
        /// Gets or sets the blend description for the pipeline state.
        /// </summary>
        public BlendDescription Blend;

        /// <summary>
        /// Gets or sets the primitive topology for rendering (e.g., TriangleList).
        /// </summary>
        [XmlAttribute]
        [DefaultValue(PrimitiveTopology.TriangleList)]
        public PrimitiveTopology Topology;

        /// <summary>
        /// Gets or sets the blend factor as a vector of four float values.
        /// </summary>
        public Vector4 BlendFactor;

        /// <summary>
        /// Gets or sets the sample mask for the pipeline state.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(int.MaxValue)]
        public uint SampleMask;

        /// <summary>
        /// Gets or sets the stencil reference value for the pipeline state.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(0)]
        public uint StencilRef;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsPipelineState"/> struct with default values.
        /// </summary>
        public GraphicsPipelineState()
        {
            Rasterizer = RasterizerDescription.CullBack;
            DepthStencil = DepthStencilDescription.Default;
            Blend = BlendDescription.Opaque;
            Topology = PrimitiveTopology.TriangleList;
            BlendFactor = default;
            SampleMask = int.MaxValue;
            StencilRef = 0;
        }

        /// <summary>
        /// Gets a default graphics pipeline state with default values.
        /// </summary>
        public static GraphicsPipelineState Default => new() { DepthStencil = DepthStencilDescription.Default, Rasterizer = RasterizerDescription.CullBack, Blend = BlendDescription.Opaque, Topology = PrimitiveTopology.TriangleList, BlendFactor = default, SampleMask = int.MaxValue };

        /// <summary>
        /// Gets a default graphics pipeline state with alpha blending.
        /// </summary>
        public static GraphicsPipelineState DefaultAlphaBlend => new() { DepthStencil = DepthStencilDescription.Default, Rasterizer = RasterizerDescription.CullBack, Blend = BlendDescription.AlphaBlend, Topology = PrimitiveTopology.TriangleList, BlendFactor = default, SampleMask = int.MaxValue };

        /// <summary>
        /// Gets a default fullscreen graphics pipeline state for rendering to the entire screen.
        /// </summary>
        public static GraphicsPipelineState DefaultFullscreen => new() { DepthStencil = DepthStencilDescription.None, Rasterizer = RasterizerDescription.CullBack, Blend = BlendDescription.Opaque, Topology = PrimitiveTopology.TriangleStrip, BlendFactor = default, SampleMask = int.MaxValue };

        /// <summary>
        /// Gets a default additive fullscreen graphics pipeline state for rendering to the entire screen with additive blending.
        /// </summary>
        public static GraphicsPipelineState DefaultAdditiveFullscreen => new() { DepthStencil = DepthStencilDescription.None, Rasterizer = RasterizerDescription.CullBack, Blend = BlendDescription.Additive, Topology = PrimitiveTopology.TriangleStrip, BlendFactor = default, SampleMask = int.MaxValue };

        /// <summary>
        /// Gets a default alpha blend fullscreen graphics pipeline state for rendering to the entire screen with alpha blending.
        /// </summary>
        public static GraphicsPipelineState DefaultAlphaBlendFullscreen => new() { DepthStencil = DepthStencilDescription.None, Rasterizer = RasterizerDescription.CullBack, Blend = BlendDescription.AlphaBlend, Topology = PrimitiveTopology.TriangleStrip, BlendFactor = default, SampleMask = int.MaxValue };

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj)
        {
            return obj is GraphicsPipelineState state && Equals(state);
        }

        /// <inheritdoc/>
        public readonly bool Equals(GraphicsPipelineState other)
        {
            return EqualityComparer<RasterizerDescription>.Default.Equals(Rasterizer, other.Rasterizer) &&
                   EqualityComparer<DepthStencilDescription>.Default.Equals(DepthStencil, other.DepthStencil) &&
                   EqualityComparer<BlendDescription>.Default.Equals(Blend, other.Blend) &&
                   Topology == other.Topology &&
                   BlendFactor.Equals(other.BlendFactor) &&
                   SampleMask == other.SampleMask &&
                   StencilRef == other.StencilRef;
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Rasterizer, DepthStencil, Blend, Topology, BlendFactor, SampleMask, StencilRef);
        }

        /// <summary>
        /// Checks if two <see cref="GraphicsPipelineState"/> instances are equal.
        /// </summary>
        public static bool operator ==(GraphicsPipelineState left, GraphicsPipelineState right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Checks if two <see cref="GraphicsPipelineState"/> instances are not equal.
        /// </summary>
        public static bool operator !=(GraphicsPipelineState left, GraphicsPipelineState right)
        {
            return !(left == right);
        }
    }
}