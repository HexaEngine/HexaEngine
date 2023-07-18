namespace HexaEngine.Core.Unsafes
{
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents an unsafe list of elements of type T.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    public unsafe struct UnsafeList<T> : IFreeable where T : unmanaged
    {
        private const uint DefaultCapacity = 4;

        private Allocator* allocator;
        private T* data;
        private uint size;
        private uint capacity;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsafeList{T}"/> struct.
        /// </summary>
        public UnsafeList()
        {
            allocator = Allocator.Default;
        }

        public UnsafeList(T[] values)
        {
            allocator = Allocator.Default;
            EnsureCapacity((uint)values.Length);
            AddRange(values);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsafeList{T}"/> struct with a custom allocator.
        /// </summary>
        /// <param name="customAllocator">The custom allocator to use.</param>
        public UnsafeList(Allocator* customAllocator)
        {
            allocator = customAllocator;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsafeList{T}"/> struct with the specified capacity.
        /// </summary>
        /// <param name="capacity">The initial capacity of the list.</param>
        public UnsafeList(uint capacity)
        {
            allocator = Allocator.Default;
            EnsureCapacity(capacity);
        }

        /// <summary>
        /// Gets the number of elements in the list.
        /// </summary>
        public uint Size => size;

        /// <summary>
        /// Gets the pointer to the underlying data array.
        /// </summary>
        public T* Data => data;

        /// <summary>
        /// Gets or sets the capacity of the list.
        /// </summary>
        public uint Capacity
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

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        public T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => data[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => data[index] = value;
        }

        public T* GetPointer(uint index)
        {
            return &data[index];
        }

        /// <summary>
        /// Sets the allocator for the list.
        /// </summary>
        /// <param name="allocator">The allocator to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAllocator(Allocator* allocator)
        {
            this.allocator = allocator;
        }

        /// <summary>
        /// Initializes the list with the default capacity.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
        {
            Grow(DefaultCapacity);
        }

        /// <summary>
        /// Initializes the list with the specified capacity.
        /// </summary>
        /// <param name="capacity">The initial capacity of the list.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init(uint capacity)
        {
            Grow(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Grow(uint capacity)
        {
            uint newcapacity = size == 0 ? DefaultCapacity : 2 * size;

            if (newcapacity < capacity)
            {
                newcapacity = capacity;
            }

            Capacity = newcapacity;
        }

        /// <summary>
        /// Ensures that the list has the specified capacity.
        /// </summary>
        /// <param name="capacity">The desired capacity.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(uint capacity)
        {
            if (this.capacity < capacity)
            {
                Grow(capacity);
            }
        }

        /// <summary>
        /// Adds an item to the list.
        /// </summary>
        /// <param name="item">The item to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushBack(T item)
        {
            EnsureCapacity(size + 1);
            data[size] = item;
            size++;
        }

        /// <summary>
        /// Adds a range of items to the list.
        /// </summary>
        /// <param name="values">The array of items to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(T[] values)
        {
            EnsureCapacity(size + (uint)values.Length);

            fixed (T* src = values)
            {
                MemoryCopy(src, &data[size], capacity * sizeof(T), values.Length * sizeof(T));
            }
            size += (uint)values.Length;
        }

        /// <summary>
        /// Removes the specified item from the list.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(T item)
        {
            RemoveAt(IndexOf(&item));
        }

        /// <summary>
        /// Removes the item at the specified index from the list.
        /// </summary>
        /// <param name="index">The index of the item to remove.</param>
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

        /// <summary>
        /// Inserts an item at the specified index in the list.
        /// </summary>
        /// <param name="item">The item to insert.</param>
        /// <param name="index">The index at which to insert the item.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(T item, uint index)
        {
            EnsureCapacity(this.size + 1);

            var size = (this.size - index) * sizeof(T);
            Buffer.MemoryCopy(&data[index], &data[index + 1], size, size);
            data[index] = item;
            this.size++;
        }

        /// <summary>
        /// Copies the contents of the list to the specified array.
        /// </summary>
        /// <param name="array">The destination array.</param>
        /// <param name="arrayIndex">The starting index in the destination array.</param>
        /// <param name="arraySize">The size of the destination array.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T* array, uint arrayIndex, uint arraySize)
        {
            Buffer.MemoryCopy(data, &array[arrayIndex], (arraySize - arrayIndex) * sizeof(T), size * sizeof(T));
        }

        /// <summary>
        /// Copies a range of elements from the list to the specified array.
        /// </summary>
        /// <param name="array">The destination array.</param>
        /// <param name="arrayIndex">The starting index in the destination array.</param>
        /// <param name="arraySize">The size of the destination array.</param>
        /// <param name="offset">The starting index in the list.</param>
        /// <param name="count">The number of elements to copy.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T* array, uint arrayIndex, uint arraySize, uint offset, uint count)
        {
            Buffer.MemoryCopy(&data[offset], &array[arrayIndex], (arraySize - arrayIndex) * sizeof(T), (count - offset) * sizeof(T));
        }

        /// <summary>
        /// Clears the list.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            size = 0;
        }

        /// <summary>
        /// Determines whether the list contains the specified item.
        /// </summary>
        /// <param name="item">The item to search for.</param>
        /// <returns><c>true</c> if the item is found; otherwise, <c>false</c>.</returns>
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

        /// <summary>
        /// Searches for the specified item and returns the index of the first occurrence within the entire list.
        /// </summary>
        /// <param name="item">The item to search for.</param>
        /// <returns>The index of the first occurrence of the item, or -1 if not found.</returns>
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
        /// Reverses the order of the elements in the list.
        /// </summary>
        public readonly void Reverse()
        {
            new Span<T>(data, (int)size).Reverse();
        }

        public void Move(UnsafeList<T> list)
        {
            allocator->Free(data);
            data = list.data;
            capacity = list.capacity;
            size = list.size;
        }

        /// <summary>
        /// Releases the memory associated with the list.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release()
        {
            allocator->Free(data);
            data = null;
            capacity = 0;
            size = 0;
        }
    }
}