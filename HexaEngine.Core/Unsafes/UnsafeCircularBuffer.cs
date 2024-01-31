namespace HexaEngine.Core.Unsafes
{
    using System;

    public unsafe struct UnsafeCircularBuffer<T> where T : unmanaged
    {
        private T* buffer;
        private int capacity;
        private int head;
        private int tail;

        public UnsafeCircularBuffer(int capacity)
        {
            this.capacity = capacity;
            buffer = AllocT<T>(capacity);
            head = tail = 0;
        }

        public void Resize(int capacity)
        {
            buffer = ReAllocT(buffer, capacity);
            this.capacity = capacity;
        }

        public void Enqueue(T item)
        {
            buffer[head] = item;
            head = (head + 1) % capacity;
            if (head == tail)
            {
                tail = (tail + 1) % capacity;
            }
        }

        public T Peak()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("Buffer is empty.");
            }

            T item = buffer[tail];
            return item;
        }

        public T Dequeue()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("Buffer is empty.");
            }

            T item = buffer[tail];
            tail = (tail + 1) % capacity;
            return item;
        }

        public bool TryPeek(out T item)
        {
            if (IsEmpty)
            {
                item = default;
                return false;
            }

            item = buffer[tail];
            return true;
        }

        public bool TryDequeue(out T item)
        {
            if (IsEmpty)
            {
                item = default;
                return false;
            }

            item = buffer[tail];
            tail = (tail + 1) % capacity;
            return true;
        }

        public bool TryPeek(T* item)
        {
            if (IsEmpty)
            {
                return false;
            }

            *item = buffer[tail];
            return true;
        }

        public bool TryDequeue(T* item)
        {
            if (IsEmpty)
            {
                return false;
            }

            *item = buffer[tail];
            tail = (tail + 1) % capacity;
            return true;
        }

        public readonly bool IsEmpty => head == tail;

        public readonly int Count => head >= tail ? head - tail : capacity - tail + head;

        public void Clear()
        {
            head = tail = 0;
        }

        public void Dispose()
        {
            Free(buffer);
            buffer = null;
        }
    }
}