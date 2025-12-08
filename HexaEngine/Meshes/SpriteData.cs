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
            ScreenPos = new((uint)sprite.ScreenPos.X, (uint)sprite.ScreenPos.Y);
            AltasPos = new((uint)sprite.AltasPos.X, (uint)sprite.AltasPos.Y);
            Size = new((uint)sprite.Size.X, (uint)sprite.Size.Y);
            Layer = sprite.Layer;
        }
    }
}