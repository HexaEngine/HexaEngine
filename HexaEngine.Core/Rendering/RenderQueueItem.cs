namespace HexaEngine.Core.Rendering
{
    public readonly struct RenderQueueItem
    {
        public readonly RenderQueueIndex Index;
        public readonly IRenderer Drawable;

        public RenderQueueItem(RenderQueueIndex index, IRenderer drawable)
        {
            Index = index;
            Drawable = drawable;
        }
    }
}