namespace HexaEngine.Core.Effects
{
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.Serialization;

    [XmlType("Effect")]
    public class EffectDescription
    {
        public EffectDescription()
        {
            Textures.Add(new EffectTexture1D());
            Textures.Add(new EffectTexture2D() { Description = new(Graphics.Format.B4G4R4A4UNorm, 1023, 3124, 1, 1, Graphics.BindFlags.ShaderResource | Graphics.BindFlags.UnorderedAccess) });
            Textures.Add(new EffectTexture3D());
            SamplerStates.Add(new EffectSamplerState());
            Buffers.Add(new EffectBuffer());
        }

        public EffectTechnique Technique { get; set; } = new();

        [XmlArrayItem("Texture1D", typeof(EffectTexture1D))]
        [XmlArrayItem("Texture2D", typeof(EffectTexture2D))]
        [XmlArrayItem("Texture3D", typeof(EffectTexture3D))]
        public List<EffectTexture> Textures { get; set; } = new();

        public List<EffectSamplerState> SamplerStates { get; set; } = new();

        public List<EffectBuffer> Buffers { get; set; } = new();

        public void Serialize()
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
            };

            using var stream = new StringWriter();
            using var writer = XmlWriter.Create(stream, settings);
            var serializer = new XmlSerializer(typeof(EffectDescription));
            serializer.Serialize(writer, this);
            File.WriteAllText("test.xml", stream.ToString());
        }
    }
}