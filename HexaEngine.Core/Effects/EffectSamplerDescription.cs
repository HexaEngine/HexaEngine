namespace HexaEngine.Core.Effects
{
    using HexaEngine.Core.Graphics;
    using System.ComponentModel;
    using System.Numerics;
    using System.Xml.Serialization;

    public struct EffectSamplerDescription
    {
        [XmlAttribute]
        public Filter Filter = Filter.MinMagMipPoint;

        [XmlAttribute]
        public TextureAddressMode AddressU = TextureAddressMode.Wrap;

        [XmlAttribute]
        public TextureAddressMode AddressV = TextureAddressMode.Wrap;

        [XmlAttribute]
        public TextureAddressMode AddressW = TextureAddressMode.Wrap;

        [DefaultValue(0.0f)]
        [XmlAttribute]
        public float MipLODBias = 0;

        [DefaultValue(0)]
        [XmlAttribute]
        public int MaxAnisotropy = 0;

        [DefaultValue(ComparisonFunction.Never)]
        [XmlAttribute]
        public ComparisonFunction ComparisonFunction = ComparisonFunction.Never;

        [DefaultValue("0, 0, 0, 0")]
        [XmlAttribute]
        public string BorderColor = "0, 0, 0, 0";

        [DefaultValue(float.MinValue)]
        [XmlAttribute]
        public float MinLOD = float.MinValue;

        [DefaultValue(float.MaxValue)]
        [XmlAttribute]
        public float MaxLOD = float.MaxValue;

        public EffectSamplerDescription()
        {
        }
    }
}