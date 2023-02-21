namespace HexaEngine.Core.Rendering
{
    using System.Collections.Concurrent;

    public unsafe class RenderQueue
    {
        private readonly ConcurrentQueue<RenderQueueItem>[][] queuesSwapBuffer;
        private const int queueCount = 5;
        private readonly int bufferCount;
        private int bufferIndex = 0;

        public RenderQueue(int bufferCount = 2)
        {
            this.bufferCount = bufferCount;

            queuesSwapBuffer = new ConcurrentQueue<RenderQueueItem>[bufferCount][];
            for (int i = 0; i < bufferCount; i++)
            {
                ConcurrentQueue<RenderQueueItem>[] queues = new ConcurrentQueue<RenderQueueItem>[queueCount];
                for (int j = 0; j < queueCount; j++)
                {
                    queues[j] = new();
                }
                queuesSwapBuffer[i] = queues;
            }
        }

        public int BufferCount => bufferCount;

        public void Clear()
        {
            for (int i = 0; i < queueCount; i++)
            {
                queuesSwapBuffer[bufferIndex][i].Clear();
            }
        }

        public void Enqueue(RenderQueueIndex index, IRenderer drawable)
        {
            queuesSwapBuffer[bufferIndex][(int)index].Enqueue(new(index, drawable));
        }

        public ConcurrentQueue<RenderQueueItem> GetQueue(RenderQueueIndex index)
        {
            return queuesSwapBuffer[bufferIndex][(int)index];
        }

        public ConcurrentQueue<RenderQueueItem> GetQueues(RenderQueueIndex index)
        {
            return queuesSwapBuffer[bufferIndex][(int)index];
        }

        public void Swap()
        {
            if (bufferIndex + 1 < bufferCount)
            {
                Interlocked.Increment(ref bufferIndex);
            }
            else
            {
                Interlocked.Exchange(ref bufferIndex, 0);
            }
        }
    }
}