namespace HexaEngine.Graphics.Effects
{
    using HexaEngine.Core.Graphics;

    public class EffectTexture2D : EffectTexture
    {
        public Texture2DDescription Description { get; set; } = new();
    }
}