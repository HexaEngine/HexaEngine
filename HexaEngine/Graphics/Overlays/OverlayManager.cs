using Hexa.NET.Mathematics;
using HexaEngine.Core.Graphics;

namespace HexaEngine.Graphics.Overlays
{
    public class OverlayManager
    {
        private readonly Lock lockObj = new();
        private readonly List<IOverlay> overlays = [];
        private readonly Queue<QueueItem> queues = new();
        private bool initialized;

        public static OverlayManager Current { get; } = new();

        private enum QueueOperation
        {
            Add,
            Remove,
            Clear
        }

        private struct QueueItem
        {
            public IOverlay Overlay;
            public QueueOperation Operation;

            public QueueItem(IOverlay overlay, QueueOperation operation)
            {
                Overlay = overlay;
                Operation = operation;
            }
        }

        public void Add(IOverlay overlay)
        {
            lock (lockObj)
            {
                queues.Enqueue(new(overlay, QueueOperation.Add));
            }
        }

        public void Remove(IOverlay overlay)
        {
            lock (lockObj)
            {
                queues.Enqueue(new(overlay, QueueOperation.Remove));
            }
        }

        public void Clear()
        {
            lock (lockObj)
            {
                queues.Enqueue(new(null!, QueueOperation.Clear));
            } 
        }

        public void Draw(IGraphicsContext context, Viewport viewport, Texture2D target, DepthStencil depthStencil)
        {
            lock (lockObj)
            {
                while (queues.TryDequeue(out QueueItem item))
                {
                    var overlay = item.Overlay;
                    switch (item.Operation)
                    {
                        case QueueOperation.Add:
                            int index = overlays.BinarySearch(overlay, OverlayComparer.Instance);
                            if (index < 0) index = ~index;
                            overlays.Insert(index, overlay);
                            if (initialized)
                            {
                                overlay.Init();
                            }
                            break;
                        case QueueOperation.Remove:
                            overlays.Remove(overlay);
                            if (initialized)
                            {
                                overlay.Release();
                            }
                            break;
                        case QueueOperation.Clear:
                            overlays.Clear();
                            break;
                    }
                }
            }

            foreach (var overlay in overlays)
            {
                overlay.Draw(context, viewport, target, depthStencil);
            }
        }

        public void Init()
        {
            foreach (var overlay in overlays)
            {
                overlay.Init();
            }
            initialized = true;
        }

        public void Release()
        {
            foreach (var overlay in overlays)
            {
                overlay.Release();
            }
            initialized = false;
        }
    }
}
