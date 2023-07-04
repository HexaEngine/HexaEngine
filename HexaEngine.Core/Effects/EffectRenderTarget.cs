namespace HexaEngine.Core.Effects
{
    using System.Xml.Serialization;

    [XmlType("RenderTarget")]
    public struct EffectRenderTarget
    {
        public EffectRenderTarget(string name, int slot)
        {
            Name = name;
            Slot = slot;
        }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public int Slot { get; set; }
    }
}