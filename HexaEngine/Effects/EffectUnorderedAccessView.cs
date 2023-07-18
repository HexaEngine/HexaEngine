namespace HexaEngine.Effects
{
    using System.Xml.Serialization;

    [XmlType("UnorderedAccessView")]
    public struct EffectUnorderedAccessView
    {
        public EffectUnorderedAccessView(string name, int slot)
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