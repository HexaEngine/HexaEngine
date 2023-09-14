namespace D3D12Testing.Unsafes
{
    using System.Collections;
    using System.Runtime.CompilerServices;

    public unsafe class UnsafeList<T> : IEnumerable<T>, IDisposable where T : unmanaged
    {
        private const int DefaultCapacity = 4;

        private Allocator* allocator;
        private T* data;
        private int size;
        private int capacity;
        private bool disposedValue;

        public UnsafeList()
        {
            allocator = Allocator.Default;
        }

        public UnsafeList(Allocator* customAllocator)
        {
            allocator = customAllocator;
        }

        public UnsafeList(int capacity)
        {
            allocator = Allocator.Default;
            EnsureCapacity(capacity);
        }

        public UnsafeList(T* data, int size)
        {
            allocator = Allocator.Default;
            Capacity = size;
            this.size = size;
            if (data != null)
            {
                Memcpy(data, this.data, size * sizeof(T), size * sizeof(T));
            }
        }

        ~UnsafeList()
        {
            Dispose(false);
        }

        public int Size => size;

        public T* Data => data;

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => capacity;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                var tmp = allocator->Allocate<T>(value);
                var oldsize = size * sizeof(T);
                var newsize = value * sizeof(T);
                Buffer.MemoryCopy(data, tmp, newsize, oldsize > newsize ? newsize : oldsize);
                allocator->Free(data);
                data = tmp;
                capacity = value;
                size = capacity < size ? capacity : size;
            }
        }

        public bool Empty => Size == 0;

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => data[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => data[index] = value;
        }

        public T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => data[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => data[index] = value;
        }

        public void Resize(int newSize)
        {
            EnsureCapacity(newSize);
            size = newSize;
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
        public void Init(int capacity)
        {
            Grow(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Grow(int capacity)
        {
            int newcapacity = size == 0 ? DefaultCapacity : 2 * size;

            if (newcapacity < capacity)
            {
                newcapacity = capacity;
            }

            Capacity = newcapacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(int capacity)
        {
            if (this.capacity < capacity)
            {
                Grow(capacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            EnsureCapacity(size + 1);
            data[size] = item;
            size++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(T[] values)
        {
            EnsureCapacity(size + values.Length);

            fixed (T* src = values)
            {
                MemoryCopy(src, &data[size], capacity * sizeof(T), values.Length * sizeof(T));
            }
            size += values.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(T item)
        {
            RemoveAt(IndexOf(&item));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index)
        {
            if (index == this.size - 1)
            {
                data[this.size - 1] = default;
                this.size--;
                return;
            }

            var size = (this.size - index) * sizeof(T);
            Buffer.MemoryCopy(&data[index + 1], &data[index], size, size);
            this.size--;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(T item, uint index)
        {
            EnsureCapacity(this.size + 1);

            var size = (this.size - index) * sizeof(T);
            Buffer.MemoryCopy(&data[index], &data[index + 1], size, size);
            data[index] = item;
            this.size++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T* array, uint arrayIndex, uint arraySize)
        {
            Buffer.MemoryCopy(data, &array[arrayIndex], (arraySize - arrayIndex) * sizeof(T), size * sizeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T* array, uint arrayIndex, uint arraySize, uint offset, uint count)
        {
            Buffer.MemoryCopy(&data[offset], &array[arrayIndex], (arraySize - arrayIndex) * sizeof(T), (count - offset) * sizeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            size = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T* item)
        {
            for (int i = 0; i < size; i++)
            {
                var current = &data[i];
                if (current == null)
                {
                    break;
                }

                if (current == item)
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(T* item)
        {
            for (int i = 0; i < size; i++)
            {
                var current = &data[i];
                if (current == null)
                {
                    break;
                }

                if (current == item)
                {
                    return i;
                }
            }

            return -1;
        }

        public void Reverse()
        {
            new Span<T>(data, size).Reverse();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new UnsafeListEnumerator<T>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new UnsafeListEnumerator<T>(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                allocator->Free(data);
                data = null;
                capacity = 0;
                size = 0;

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}