namespace HexaEngine.Graphics
{
    using HexaEngine.Core.Graphics;
    using System.Collections.Generic;

    public class RenderQueue
    {
        private readonly List<RenderQueueGroup> groups = [];
        private readonly List<IDrawable> renderers = [];
        private RenderQueueIndex queueIndex;
        private string pass = null!;

        public RenderQueue()
        {
        }

        public void Execute(IGraphicsContext context)
        {
            for (int i = 0; i < groups.Count; i++)
            {
                groups[i].ExecuteGroup(context);
            }
        }

        public void Add(IDrawable component)
        {
            renderers.Add(component);

            uint index = component.QueueIndex;
            RendererFlags flags = component.Flags;
            bool isDynamic = (flags & RendererFlags.Dynamic) != 0;
            RenderQueueGroup? groupLast = null;
            for (int i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                RenderQueueGroup? next = i + 1 < groups.Count ? groups[i + 1] : null;

                if (group.BaseIndex > index)
                {
                    continue;
                }

                if ((next?.BaseIndex ?? uint.MaxValue) <= index)
                {
                    break;
                }

                if (group.IsDynamic == isDynamic)
                {
                    groupLast = group;
                }
            }

            if (groupLast == null)
            {
                groupLast = new(queueIndex, isDynamic ? QueueGroupFlags.Dynamic : QueueGroupFlags.None, pass);
                groups.Add(groupLast);
            }

            groupLast.Add(component);
        }

        public bool Remove(IDrawable component)
        {
            if (!renderers.Remove(component))
            {
                return false;
            }

            foreach (var group in groups)
            {
                if (group.Remove(component))
                {
                    return true;
                }
            }

            return false;
        }
    }
}