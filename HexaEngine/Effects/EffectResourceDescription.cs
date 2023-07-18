namespace HexaEngine.Effects
{
    using HexaEngine.Core.Graphics;

    public struct EffectResourceDescription
    {
        public int Slot;
        public string Name;
        public string RefName;
        public ShaderStage Stage;
        public ShaderResourceViewDimension Dimension;
    }
}