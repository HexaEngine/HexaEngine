namespace HexaEngine.Core.Renderers
{
    using HexaEngine.Core.Unsafes;
    using System.Collections.Concurrent;

    public class DrawQueue
    {
        private ConcurrentQueue<Pointer<ObjectHandle>>[] queues = new ConcurrentQueue<Pointer<ObjectHandle>>[2] { new(), new() };
        private int enqueueIndex = 0;
        private int dequeueIndex = 1;

        public void Swap()
        {
            (dequeueIndex, enqueueIndex) = (enqueueIndex, dequeueIndex);
        }

        public void Enqueue(Pointer<ObjectHandle> handle)
        {
            queues[enqueueIndex].Enqueue(handle);
        }

        public bool IsEmpty => queues[dequeueIndex].IsEmpty;

        public bool TryDequeue(out Pointer<ObjectHandle> handle)
        {
            return queues[dequeueIndex].TryDequeue(out handle);
        }
    }
}