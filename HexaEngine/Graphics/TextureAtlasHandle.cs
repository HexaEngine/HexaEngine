namespace HexaEngine.Graphics
{
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;

    public struct TextureAtlasHandle : IDisposable
    {
        private TextureAtlas atlas;
        private BinPackingNode node;
        private bool valid;

        public TextureAtlasHandle(TextureAtlas atlas, BinPackingNode node)
        {
            this.atlas = atlas;
            this.node = node;
            valid = true;
        }

        public readonly TextureAtlas Atlas => atlas;

        public readonly BinPackingNode Node => node;

        public readonly Rectangle Rect => node.Rect;

        public readonly Point2 Offset => Rect.Offset;

        public readonly Point2 Size => Rect.Size;

        public readonly Viewport Viewport => new(Offset, Size);

        public void Dispose()
        {
            if (valid)
            {
                atlas.Free(ref this);
            }

            this = default;
        }
    }
}