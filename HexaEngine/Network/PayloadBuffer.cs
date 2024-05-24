namespace HexaEngine.Network
{
    public unsafe class PayloadBuffer : IDisposable
    {
        private readonly int size;
        private byte* buffer;
        private int head;
        private int tail;

        public PayloadBuffer(int size = 8192 * 4)
        {
            this.size = size;
            buffer = AllocT<byte>(size);
        }

        public PayloadBufferSegment Rent(int length)
        {
            int index = head;
            int newHead = head + length;
            if (newHead >= size)
            {
                newHead = length;
                index = 0;
                if (newHead >= tail)
                {
                    return PayloadBufferSegment.Invalid;
                }
            }
            else if (newHead >= tail && head < tail)
            {
                return PayloadBufferSegment.Invalid;
            }

            head = newHead;
            return new(buffer, index, length);
        }

        public void Return(PayloadBufferSegment segment)
        {
            if (tail != segment.Start)
            {
                throw new InvalidOperationException();
            }

            tail = (tail + segment.Length) % size;

            if (tail == head)
            {
                tail = head = 0;
            }
        }

        public void Dispose()
        {
            if (buffer != null)
            {
                Free(buffer);
                buffer = null;
            }
            GC.SuppressFinalize(this);
        }
    }
}