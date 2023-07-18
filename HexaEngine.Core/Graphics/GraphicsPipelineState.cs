namespace HexaEngine.Core.Graphics
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Numerics;
    using System.Xml.Serialization;

    public struct GraphicsPipelineState : IEquatable<GraphicsPipelineState>
    {
        public RasterizerDescription Rasterizer;
        public DepthStencilDescription DepthStencil;
        public BlendDescription Blend;

        [XmlAttribute]
        [DefaultValue(PrimitiveTopology.TriangleList)]
        public PrimitiveTopology Topology;

        public Vector4 BlendFactor;

        [XmlAttribute]
        [DefaultValue(int.MaxValue)]
        public uint SampleMask;

        [XmlAttribute]
        [DefaultValue(0)]
        public uint StencilRef;

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

        public static GraphicsPipelineState Default => new() { DepthStencil = DepthStencilDescription.Default, Rasterizer = RasterizerDescription.CullBack, Blend = BlendDescription.Opaque, Topology = PrimitiveTopology.TriangleList, BlendFactor = default, SampleMask = int.MaxValue };

        public static GraphicsPipelineState DefaultFullscreen => new() { DepthStencil = DepthStencilDescription.None, Rasterizer = RasterizerDescription.CullBack, Blend = BlendDescription.Opaque, Topology = PrimitiveTopology.TriangleStrip, BlendFactor = default, SampleMask = int.MaxValue };

        public override bool Equals(object? obj)
        {
            return obj is GraphicsPipelineState state && Equals(state);
        }

        public bool Equals(GraphicsPipelineState other)
        {
            return EqualityComparer<RasterizerDescription>.Default.Equals(Rasterizer, other.Rasterizer) &&
                   EqualityComparer<DepthStencilDescription>.Default.Equals(DepthStencil, other.DepthStencil) &&
                   EqualityComparer<BlendDescription>.Default.Equals(Blend, other.Blend) &&
                   Topology == other.Topology &&
                   BlendFactor.Equals(other.BlendFactor) &&
                   SampleMask == other.SampleMask &&
                   StencilRef == other.StencilRef;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Rasterizer, DepthStencil, Blend, Topology, BlendFactor, SampleMask, StencilRef);
        }

        public static bool operator ==(GraphicsPipelineState left, GraphicsPipelineState right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GraphicsPipelineState left, GraphicsPipelineState right)
        {
            return !(left == right);
        }
    }
}