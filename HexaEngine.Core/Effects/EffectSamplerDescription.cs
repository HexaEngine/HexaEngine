namespace HexaEngine.Core.Effects
{
    using HexaEngine.Core.Graphics;

    public struct EffectSamplerDescription
    {
        public int Slot;
        public string Name;
        public string RefName;
        public ShaderStage Stage;
        public SamplerDescription Description;
    }
}