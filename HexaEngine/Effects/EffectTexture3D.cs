namespace HexaEngine.Effects
{
    using HexaEngine.Core.Graphics;

    public class EffectTexture3D : EffectTexture
    {
        public Texture3DDescription Description { get; set; } = new();
    }
}