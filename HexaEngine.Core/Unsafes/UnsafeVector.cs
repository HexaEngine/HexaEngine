namespace HexaEngine.Core.Unsafes
{
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents an unsafe vector.
    /// </summary>
    /// <typeparam name="T">The type of elements in the vector.</typeparam>
    public unsafe struct UnsafeVector<T> : IFreeable where T : unmanaged
    {
        private const int DefaultCapacity = 4;

        private int size;
        private int capacity;
        private T* data;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsafeVector{T}"/> struct with the specified capacity.
        /// </summary>
        /// <param name="capacity">The initial capacity of the vector.</param>
        public UnsafeVector(int capacity)
        {
            EnsureCapacity(capacity);
        }

        /// <summary>
        /// Gets the number of elements in the vector.
        /// </summary>
        public int Size => size;

        /// <summary>
        /// Gets a pointer to the underlying data of the vector.
        /// </summary>
        public T* Data => data;

        /// <summary>
        /// Gets or sets the capacity of the vector.
        /// </summary>
        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => capacity;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                var tmp = AllocT<T>(value);
                var oldsize = size * sizeof(T);
                var newsize = value * sizeof(T);
                Buffer.MemoryCopy(data, tmp, newsize, oldsize > newsize ? newsize : oldsize);
                Free(data);
                data = tmp;
                capacity = value;
                size = capacity < size ? capacity : size;
            }
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => data[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => data[index] = value;
        }

        /// <summary>
        /// Initializes the vector with the default capacity.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
        {
            Grow(DefaultCapacity);
        }

        /// <summary>
        /// Initializes the vector with the specified capacity.
        /// </summary>
        /// <param name="capacity">The initial capacity of the vector.</param>
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
                MemcpyT(src, &data[size], capacity * sizeof(T), values.Length * sizeof(T));
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

        /// <summary>
        /// Resizes the vector to the specified size. If the new size is larger than the current capacity, the capacity will be increased accordingly.
        /// </summary>
        /// <param name="newSize">The new size of the vector.</param>
        public void Resize(int newSize)
        {
            size = newSize;
            EnsureCapacity(newSize);
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

        /// <summary>
        /// Reverses the order of the elements in the vector.
        /// </summary>
        public void Reverse()
        {
            new Span<T>(data, size).Reverse();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release()
        {
            Free(data);
            data = null;
            capacity = 0;
            size = 0;
        }
    }
}