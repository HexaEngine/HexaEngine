namespace HexaEngine.Effects
{
    using System.Xml;
    using System.Xml.Serialization;

    [XmlType("SamplerState")]
    public class EffectSamplerState
    {
        [XmlAttribute]
        public string Name { get; set; }

        public EffectSamplerDescription Description { get; set; } = new();
    }
}