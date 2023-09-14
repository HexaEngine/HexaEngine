namespace HexaEngine.Graphics.Effects
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class EffectTechnique
    {
        public EffectTechnique()
        {
            Passes.Add(new EffectGraphicsPass());
            Passes.Add(new EffectComputePass());
        }

        [XmlArrayItem("GraphicsPass", typeof(EffectGraphicsPass))]
        [XmlArrayItem("ComputePass", typeof(EffectComputePass))]
        public List<EffectPass> Passes { get; set; } = new();
    }
}