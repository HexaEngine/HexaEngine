namespace HexaEngine.Meshes
{
    using Hexa.NET.Mathematics;

    public struct SpriteData
    {
        public UPoint2 ScreenPos;
        public UPoint2 Size;
        public UPoint2 AltasPos;
        public uint Layer;

        public SpriteData(Sprite sprite)
        {
            ScreenPos = sprite.ScreenPos;
            AltasPos = sprite.AltasPos;
            Size = sprite.Size;
            Layer = sprite.Layer;
        }
    }
}