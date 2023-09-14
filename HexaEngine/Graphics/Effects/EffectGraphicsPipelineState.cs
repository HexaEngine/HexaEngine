namespace HexaEngine.Graphics.Effects
{
    using HexaEngine.Core.Graphics;
    using System.ComponentModel;
    using System.Xml;
    using System.Xml.Serialization;

    public class EffectGraphicsPipelineState
    {
        [XmlAttribute]
        public EffectRasterizer Rasterizer { get; set; } = EffectRasterizer.CullBack;

        [XmlAttribute]
        public EffectDepthStencil DepthStencil { get; set; } = EffectDepthStencil.Default;

        [XmlAttribute]
        public EffectBlend Blend { get; set; } = EffectBlend.Opaque;

        [XmlAttribute]
        [DefaultValue(PrimitiveTopology.TriangleList)]
        public PrimitiveTopology Topology { get; set; } = PrimitiveTopology.TriangleList;

        [XmlAttribute]
        [DefaultValue("0, 0, 0, 0")]
        public string BlendFactor { get; set; } = "0, 0, 0, 0";

        [XmlAttribute]
        [DefaultValue(uint.MaxValue)]
        public uint SampleMask { get; set; } = uint.MaxValue;

        [XmlAttribute]
        [DefaultValue(0)]
        public uint StencilRef { get; set; } = 0;
    }
}