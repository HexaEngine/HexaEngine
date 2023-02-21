namespace HexaEngine.Core.Effects
{
    using HexaEngine.Core.Graphics;

    public struct EffectConstantBuffer
    {
        public int Slot;
        public string Name;
        public string RefName;
        public ShaderStage Stage;
    }
}