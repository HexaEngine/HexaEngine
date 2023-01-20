namespace HexaEngine.Scenes.Managers
{
    public readonly struct RenderQueueItem
    {
        public readonly RenderQueueIndex Index;
        public readonly IDrawable Drawable;

        public RenderQueueItem(RenderQueueIndex index, IDrawable drawable)
        {
            Index = index;
            Drawable = drawable;
        }
    }
}