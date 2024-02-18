namespace HexaEngine.Graphics
{
    using System.Collections.Generic;

    public class RendererInstanceQueueCollection<T> where T : IRendererInstance
    {
        private readonly Dictionary<RenderQueueIndex, List<T>> queues = [];
        private readonly RenderQueueIndex[] indices;
        private readonly object _lock = new();

        public RendererInstanceQueueCollection()
        {
            indices = Enum.GetValues<RenderQueueIndex>();
            for (int i = 0; i < indices.Length; i++)
            {
                var index = indices[i];
                queues.Add(index, new());
            }
        }

        public object SyncObject => _lock;

        public void Lock()
        {
            Monitor.Enter(_lock);
        }

        public void ReleaseLock()
        {
            Monitor.Exit(_lock);
        }

        public List<T> this[RenderQueueIndex index]
        {
            get => queues[index];
        }

        public void Clear()
        {
            lock (_lock)
            {
                for (int i = 0; i < indices.Length; i++)
                {
                    Clear(indices[i]);
                }
            }
        }

        public void Clear(RenderQueueIndex index)
        {
            lock (_lock)
            {
                var queue = queues[index];
                for (int i = 0; i < queue.Count; i++)
                {
                    queue[i].QueueIndexChanged -= InstanceQueueIndexChanged;
                }
                queues[index].Clear();
            }
        }

        public void AddInstance(T instance)
        {
            lock (_lock)
            {
                instance.QueueIndexChanged += InstanceQueueIndexChanged;
                var queue = queues[GetIndex(instance.QueueIndex)];
                queue.Add(instance);
                queue.Sort(SortRendererInstanceAscending<T>.Instance);
            }
        }

        public bool RemoveInstance(T instance)
        {
            lock (_lock)
            {
                var index = GetIndex(instance.QueueIndex);

                var queue = queues[index];
                if (queue.Remove(instance))
                {
                    instance.QueueIndexChanged -= InstanceQueueIndexChanged;
                    return true;
                }
                return false;
            }
        }

        public bool ContainsInstance(T instance)
        {
            lock (_lock)
            {
                return queues[GetIndex(instance.QueueIndex)].Contains(instance);
            }
        }

        private void InstanceQueueIndexChanged(IRendererInstance sender, uint oldIndex, uint newIndex)
        {
            if (sender is not T instance)
            {
                return;
            }

            lock (_lock)
            {
                List<T> queue = queues[GetIndex(oldIndex)];
                if (queue.Remove(instance))
                {
                    queue = queues[GetIndex(newIndex)];
                    queue.Add(instance);
                    queue.Sort(SortRendererInstanceAscending<T>.Instance);
                }
            }
        }

        private static RenderQueueIndex GetIndex(uint index)
        {
            if (index < (uint)RenderQueueIndex.Geometry)
            {
                return RenderQueueIndex.Background;
            }
            if (index < (uint)RenderQueueIndex.AlphaTest)
            {
                return RenderQueueIndex.Geometry;
            }
            if (index < (uint)RenderQueueIndex.GeometryLast)
            {
                return RenderQueueIndex.AlphaTest;
            }
            if (index < (uint)RenderQueueIndex.Transparency)
            {
                return RenderQueueIndex.GeometryLast;
            }
            if (index < (uint)RenderQueueIndex.Overlay)
            {
                return RenderQueueIndex.Transparency;
            }
            return RenderQueueIndex.Overlay;
        }
    }
}