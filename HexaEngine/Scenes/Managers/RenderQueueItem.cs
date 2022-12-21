namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Core.Graphics;
    using System.Collections.Generic;

    public class RenderQueueItem
    {
        public List<RenderQueueIndex> Indices;
        public IDrawable Drawable;
        public ICommandList? CommandList;
        public bool IsEnabled;

        public RenderQueueItem(RenderQueueIndex index, IDrawable drawable)
        {
            Indices = new() { index };
            Drawable = drawable;
        }
    }
}