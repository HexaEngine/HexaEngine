namespace HexaEngine.Core.Unsafes
{
    using System;
    using System.Runtime.CompilerServices;

    public unsafe struct UnsafeQueue<T> : IFreeable where T : unmanaged
    {
        private const int DefaultCapacity = 4;

        private Allocator* allocator;
        private T* items;
        private T* head;
        private T* tail;
        private uint count;
        private uint capacity;

        public uint Count => count;

        public uint Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => capacity;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                var tmp = allocator->Allocate<T>(value);
                var oldsize = count * sizeof(T);
                var newsize = value * sizeof(T);
                Buffer.MemoryCopy(items, tmp, newsize, oldsize > newsize ? newsize : oldsize);
                allocator->Free(items);
                head = tail = items = tmp;
                capacity = value;
                count = capacity < count ? capacity : count;
            }
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => items[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => items[index] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAllocator(Allocator* allocator)
        {
            this.allocator = allocator;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
        {
            Grow(DefaultCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init(uint capacity)
        {
            Grow(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Grow(uint capacity)
        {
            uint newcapacity = count == 0 ? DefaultCapacity : 2 * count;

            if (newcapacity < capacity) newcapacity = capacity;

            Capacity = newcapacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(uint capacity)
        {
            if (this.capacity < capacity)
            {
                Grow(capacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(T item)
        {
            EnsureCapacity(count + 1);
            head++;
            *head = item;
            count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Dequeue()
        {
            var tmp = *tail;
            *tail = default;
            tail++;
            if (head == tail)
            {
                head = tail = items;
            }
            count--;
            return tmp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out T item)
        {
            if (count == 0)
            {
                item = default;
                return false;
            }
            item = *tail;
            *tail = default;
            tail++;
            if (head == tail)
            {
                head = tail = items;
            }
            count--;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T* array, uint arrayIndex, uint arraySize)
        {
            Buffer.MemoryCopy(items, &array[arrayIndex], (arraySize - arrayIndex) * sizeof(T), count * sizeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T* array, uint arrayIndex, uint arraySize, uint offset, uint count)
        {
            Buffer.MemoryCopy(&items[offset], &array[arrayIndex], (arraySize - arrayIndex) * sizeof(T), (count - offset) * sizeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            var ptr = items;
            for (int i = 0; i < count; i++)
            {
                ptr[i] = default;
            }
            count = 0;
            head = tail = items;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T* item)
        {
            for (int i = 0; i < count; i++)
            {
                var current = &items[i];
                if (current == null) break;
                if (current == item)
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Free()
        {
            allocator->Free(items);
            items = null;
            head = null;
            tail = null;
            capacity = 0;
            count = 0;
            fixed (void* ptr = &this)
            {
                allocator->Free(ptr);
            }
        }
    }
}