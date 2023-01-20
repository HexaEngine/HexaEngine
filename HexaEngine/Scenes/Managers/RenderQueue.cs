namespace HexaEngine.Scenes.Managers
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public unsafe class RenderQueue
    {
        private readonly RenderQueueIndex[] keys;
        private readonly Dictionary<RenderQueueIndex, ConcurrentQueue<RenderQueueItem>> queues = new();

        public RenderQueue()
        {
            keys = Enum.GetValues<RenderQueueIndex>();
            foreach (var value in keys)
            {
                queues.Add(value, new ConcurrentQueue<RenderQueueItem>());
            }
        }

        public virtual void Clear()
        {
            for (int i = 0; i < keys.Length; i++)
            {
                queues[keys[i]].Clear();
            }
        }

        public virtual void Enqueue(RenderQueueIndex index, IDrawable drawable)
        {
            queues[index].Enqueue(new(index, drawable));
        }

        public virtual ConcurrentQueue<RenderQueueItem> GetQueue(RenderQueueIndex index)
        {
            return queues[index];
        }
    }
}