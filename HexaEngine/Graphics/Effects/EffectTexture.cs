namespace HexaEngine.Graphics.Effects
{
    using System.Xml;
    using System.Xml.Serialization;

    public class EffectTexture
    {
        [XmlAttribute]
        public string Name { get; set; }
    }
}