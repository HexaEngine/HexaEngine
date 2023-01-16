namespace HexaEngine.Scenes.Managers
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public unsafe class RenderQueue
    {
        private readonly RenderQueueIndex[] keys;
        private readonly Dictionary<RenderQueueIndex, List<RenderQueueItem>> queues = new();
        private readonly Dictionary<IDrawable, RenderQueueItem> items = new();
        private readonly ConcurrentQueue<IDrawable> updateQueue = new();

        public RenderQueue()
        {
            keys = Enum.GetValues<RenderQueueIndex>();
            foreach (var value in keys)
            {
                queues.Add(value, new List<RenderQueueItem>());
            }
        }

        public virtual void Clear()
        {
            for (int i = 0; i < keys.Length; i++)
            {
                queues[keys[i]].Clear();
            }
        }

        /// <summary>
        /// Issues the IDrawable to be rerecorded
        /// </summary>
        /// <param dbgName="drawable"></param>
        public virtual void Update(IDrawable drawable)
        {
            updateQueue.Enqueue(drawable);
        }

        public virtual void Enqueue(RenderQueueIndex index, IDrawable drawable)
        {
            RenderQueueItem item = new(index, drawable);
            items.Add(drawable, item);
            queues[index].Add(item);
        }

        public virtual void Dequeue(RenderQueueIndex index, IDrawable drawable)
        {
            RenderQueueItem item = items[drawable];
            queues[index].Remove(item);
            items.Remove(drawable);
            item.CommandList?.Dispose();
        }

        public virtual void Enable(IDrawable drawable)
        {
            items[drawable].IsEnabled = true;
        }

        public virtual void Disable(IDrawable drawable)
        {
            items[drawable].IsEnabled = false;
        }

        public virtual IReadOnlyList<RenderQueueItem> GetQueue(RenderQueueIndex index)
        {
            return queues[index];
        }
    }
}