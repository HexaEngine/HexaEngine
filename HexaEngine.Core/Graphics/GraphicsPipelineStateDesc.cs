namespace HexaEngine.Core.Graphics
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Numerics;
    using System.Xml.Serialization;

    public enum GraphicsPipelineStateFlags
    {
        None = 0,
        CreateResourceBindingList,
    }

    /// <summary>
    /// Represents the description of a graphics pipeline state.
    /// </summary>
    public struct GraphicsPipelineStateDesc : IEquatable<GraphicsPipelineStateDesc>
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
        /// Gets or sets the input elements of the graphics pipeline state.
        /// </summary>
        public InputElementDescription[]? InputElements;

        /// <summary>
        /// Gets or sets the flags of the graphics pipeline state.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(0)]
        public GraphicsPipelineStateFlags Flags;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsPipelineStateDesc"/> struct with default values.
        /// </summary>
        public GraphicsPipelineStateDesc()
        {
            Rasterizer = RasterizerDescription.CullBack;
            DepthStencil = DepthStencilDescription.Default;
            Blend = BlendDescription.Opaque;
            Topology = PrimitiveTopology.TriangleList;
            BlendFactor = default;
            SampleMask = int.MaxValue;
            StencilRef = 0;
            InputElements = null;
        }

        /// <summary>
        /// Gets a default graphics pipeline state with default values.
        /// </summary>
        public static GraphicsPipelineStateDesc Default => new() { DepthStencil = DepthStencilDescription.Default, Rasterizer = RasterizerDescription.CullBack, Blend = BlendDescription.Opaque, Topology = PrimitiveTopology.TriangleList, BlendFactor = default, SampleMask = int.MaxValue };

        /// <summary>
        /// Gets a default graphics pipeline state with alpha blending.
        /// </summary>
        public static GraphicsPipelineStateDesc DefaultAlphaBlend => new() { DepthStencil = DepthStencilDescription.Default, Rasterizer = RasterizerDescription.CullBack, Blend = BlendDescription.AlphaBlend, Topology = PrimitiveTopology.TriangleList, BlendFactor = default, SampleMask = int.MaxValue };

        /// <summary>
        /// Gets a default fullscreen graphics pipeline state for rendering to the entire screen.
        /// </summary>
        public static GraphicsPipelineStateDesc DefaultFullscreen => new() { DepthStencil = DepthStencilDescription.None, Rasterizer = RasterizerDescription.CullBack, Blend = BlendDescription.Opaque, Topology = PrimitiveTopology.TriangleStrip, BlendFactor = default, SampleMask = int.MaxValue };

        /// <summary>
        /// Gets a default additive fullscreen graphics pipeline state for rendering to the entire screen with additive blending.
        /// </summary>
        public static GraphicsPipelineStateDesc DefaultAdditiveFullscreen => new() { DepthStencil = DepthStencilDescription.None, Rasterizer = RasterizerDescription.CullBack, Blend = BlendDescription.Additive, Topology = PrimitiveTopology.TriangleStrip, BlendFactor = default, SampleMask = int.MaxValue };

        /// <summary>
        /// Gets a default alpha blend fullscreen graphics pipeline state for rendering to the entire screen with alpha blending.
        /// </summary>
        public static GraphicsPipelineStateDesc DefaultAlphaBlendFullscreen => new() { DepthStencil = DepthStencilDescription.None, Rasterizer = RasterizerDescription.CullBack, Blend = BlendDescription.AlphaBlend, Topology = PrimitiveTopology.TriangleStrip, BlendFactor = default, SampleMask = int.MaxValue };

        /// <summary>
        /// Gets a default fullscreen graphics pipeline state for rendering to the entire screen.
        /// </summary>
        public static GraphicsPipelineStateDesc DefaultFullscreenF => new() { DepthStencil = DepthStencilDescription.None, Rasterizer = RasterizerDescription.CullBack, Blend = BlendDescription.Opaque, Topology = PrimitiveTopology.TriangleStrip, BlendFactor = default, SampleMask = int.MaxValue, Flags = GraphicsPipelineStateFlags.CreateResourceBindingList };

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj)
        {
            return obj is GraphicsPipelineStateDesc state && Equals(state);
        }

        /// <inheritdoc/>
        public readonly bool Equals(GraphicsPipelineStateDesc other)
        {
            return EqualityComparer<RasterizerDescription>.Default.Equals(Rasterizer, other.Rasterizer) &&
                   EqualityComparer<DepthStencilDescription>.Default.Equals(DepthStencil, other.DepthStencil) &&
                   EqualityComparer<BlendDescription>.Default.Equals(Blend, other.Blend) &&
                   Topology == other.Topology &&
                   BlendFactor.Equals(other.BlendFactor) &&
                   SampleMask == other.SampleMask &&
                   StencilRef == other.StencilRef &&
                   EqualityComparer<InputElementDescription[]?>.Default.Equals(InputElements, other.InputElements); ;
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Rasterizer, DepthStencil, Blend, Topology, BlendFactor, SampleMask, StencilRef, InputElements);
        }

        /// <summary>
        /// Checks if two <see cref="GraphicsPipelineStateDesc"/> instances are equal.
        /// </summary>
        public static bool operator ==(GraphicsPipelineStateDesc left, GraphicsPipelineStateDesc right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Checks if two <see cref="GraphicsPipelineStateDesc"/> instances are not equal.
        /// </summary>
        public static bool operator !=(GraphicsPipelineStateDesc left, GraphicsPipelineStateDesc right)
        {
            return !(left == right);
        }
    }
}