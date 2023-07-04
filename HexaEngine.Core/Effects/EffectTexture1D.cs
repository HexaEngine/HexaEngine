namespace HexaEngine.Core.Effects
{
    using HexaEngine.Core.Graphics;
    using System.Xml.Serialization;

    public class EffectTexture1D : EffectTexture
    {
        public Texture1DDescription Description { get; set; } = new();
    }

    [XmlType("Buffer")]
    public class EffectBuffer
    {
        [XmlAttribute]
        public string Name { get; set; }

        public BufferDescription Description { get; set; }
    }
}