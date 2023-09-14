namespace HexaEngine.Graphics.Effects
{
    using HexaEngine.Core.Graphics;
    using System.Xml;
    using System.Xml.Serialization;

    [XmlType("Binding")]
    public struct EffectBinding
    {
        public EffectBinding(string name, ShaderStage stage, uint slot)
        {
            Name = name;
            Stage = stage;
            Slot = slot;
        }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public ShaderStage Stage { get; set; }

        [XmlAttribute]
        public uint Slot { get; set; }
    }
}