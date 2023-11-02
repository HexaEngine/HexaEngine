namespace HexaEngine.Core.Unsafes
{
    using System.Collections;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents an unsafe list of elements of type T.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    public unsafe struct UnsafeList<T> : IFreeable, IEnumerable<T>, IList<T> where T : unmanaged
    {
        private const uint DefaultCapacity = 4;

        private T* pointer;
        private uint size;
        private uint capacity;

        public struct Enumerator : IEnumerator<T>
        {
            private readonly T* pointer;
            private readonly uint size;
            private int currentIndex;

            internal Enumerator(UnsafeList<T> list)
            {
                pointer = list.pointer;
                size = list.size;
                currentIndex = -1;
            }

            public T Current => pointer[currentIndex];

            object IEnumerator.Current => Current;

            public readonly void Dispose()
            {
                // Enumerator does not own resources, so nothing to dispose.
            }

            public bool MoveNext()
            {
                if (currentIndex < size - 1)
                {
                    currentIndex++;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                currentIndex = 0;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsafeList{T}"/> struct.
        /// </summary>
        public UnsafeList()
        {
            Capacity = DefaultCapacity;
        }

        public UnsafeList(T[] values)
        {
            Capacity = (uint)values.Length;
            AppendRange(values);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsafeList{T}"/> struct with the specified capacity.
        /// </summary>
        /// <param name="capacity">The initial capacity of the list.</param>
        public UnsafeList(uint capacity)
        {
            Capacity = capacity;
        }

        /// <summary>
        /// Gets the number of elements in the list.
        /// </summary>
        public readonly uint Size => size;

        public readonly int Count => (int)size;

        readonly bool ICollection<T>.IsReadOnly => false;

        /// <summary>
        /// Gets the pointer to the underlying data array.
        /// </summary>
        public readonly T* Data => pointer;

        public readonly bool Empty => size == 0;

        public readonly T* Front => pointer;

        public readonly T* Back => &pointer[size - 1];

        /// <summary>
        /// Gets or sets the capacity of the list.
        /// </summary>
        public uint Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => capacity;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (pointer == null)
                {
                    pointer = AllocT<T>(value);
                    capacity = value;
                    Erase();
                    return;
                }
                pointer = ReAllocT(pointer, value);
                capacity = value;
                size = capacity < size ? capacity : size;
            }
        }

        T IList<T>.this[int index] { get => this[index]; set => this[index] = value; }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        public T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => pointer[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => pointer[index] = value;
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => pointer[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => pointer[index] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T At(uint index)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, size);
            return this[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T At(int index)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, size);
            return this[index];
        }

        public T* GetPointer(uint index)
        {
            return &pointer[index];
        }

        public T* GetPointer(int index)
        {
            return &pointer[index];
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
        public void Reserve(uint capacity)
        {
            if (this.capacity < capacity || pointer == null)
            {
                Grow(capacity);
            }
        }

        public void Resize(uint newSize)
        {
            if (size == newSize)
            {
                return;
            }
            Capacity = newSize;
            size = newSize;
        }

        public readonly void Erase()
        {
            ZeroMemoryT(pointer, capacity);
        }

        /// <summary>
        /// Adds an item to the list.
        /// </summary>
        /// <param name="item">The item to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushBack(T item)
        {
            Reserve(size + 1);
            pointer[size] = item;
            size++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            PushBack(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PopBack()
        {
            pointer[size - 1] = default;
            size--;
        }

        /// <summary>
        /// Adds a range of items to the list.
        /// </summary>
        /// <param name="values">The array of items to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendRange(T[] values)
        {
            Reserve(size + (uint)values.Length);

            fixed (T* src = values)
            {
                Memcpy(src, &pointer[size], capacity * sizeof(T), values.Length * sizeof(T));
            }
            size += (uint)values.Length;
        }

        /// <summary>
        /// Removes the specified item from the list.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T item)
        {
            var index = IndexOf(&item);
            if (index == -1)
                return false;
            RemoveAt(index);
            return true;
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
                pointer[this.size - 1] = default;
                this.size--;
                return;
            }

            var size = (this.size - index) * sizeof(T);
            Buffer.MemoryCopy(&pointer[index + 1], &pointer[index], size, size);
            this.size--;
        }

        /// <summary>
        /// Removes the item at the specified index from the list.
        /// </summary>
        /// <param name="index">The index of the item to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(uint index)
        {
            if (index == this.size - 1)
            {
                pointer[this.size - 1] = default;
                this.size--;
                return;
            }

            var size = (this.size - index) * sizeof(T);
            Buffer.MemoryCopy(&pointer[index + 1], &pointer[index], size, size);
            this.size--;
        }

        /// <summary>
        /// Inserts an item at the specified index in the list.
        /// </summary>
        /// <param name="index">The index at which to insert the item.</param>
        /// <param name="item">The item to insert.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, T item)
        {
            Reserve(this.size + 1);

            var size = (this.size - index) * sizeof(T);
            Buffer.MemoryCopy(&pointer[index], &pointer[index + 1], size, size);
            pointer[index] = item;
            this.size++;
        }

        /// <summary>
        /// Inserts an item at the specified index in the list.
        /// </summary>
        /// <param name="index">The index at which to insert the item.</param>
        /// <param name="item">The item to insert.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(uint index, T item)
        {
            Reserve(this.size + 1);

            var size = (this.size - index) * sizeof(T);
            Buffer.MemoryCopy(&pointer[index], &pointer[index + 1], size, size);
            pointer[index] = item;
            this.size++;
        }

        /// <summary>
        /// Copies the contents of the list to the specified array.
        /// </summary>
        /// <param name="array">The destination array.</param>
        /// <param name="arrayIndex">The starting index in the destination array.</param>
        /// <param name="arraySize">The size of the destination array.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(T* array, uint arrayIndex, uint arraySize)
        {
            Buffer.MemoryCopy(pointer, &array[arrayIndex], (arraySize - arrayIndex) * sizeof(T), size * sizeof(T));
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
            Buffer.MemoryCopy(&pointer[offset], &array[arrayIndex], (arraySize - arrayIndex) * sizeof(T), (count - offset) * sizeof(T));
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
                var current = &pointer[i];
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

        bool ICollection<T>.Contains(T item)
        {
            return Contains(&item);
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
                var current = &pointer[i];
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

        int IList<T>.IndexOf(T item)
        {
            return IndexOf(&item);
        }

        /// <summary>
        /// Reverses the order of the elements in the list.
        /// </summary>
        public readonly void Reverse()
        {
            new Span<T>(pointer, (int)size).Reverse();
        }

        public void Move(UnsafeList<T> list)
        {
            Free(pointer);
            pointer = list.pointer;
            capacity = list.capacity;
            size = list.size;
        }

        // Implement IEnumerable<T>
        public readonly Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Releases the memory associated with the list.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release()
        {
            Free(pointer);
            this = default;
        }

        void IList<T>.Insert(int index, T item)
        {
            Insert(index, item);
        }

        void IList<T>.RemoveAt(int index)
        {
            RemoveAt(index);
        }

        void ICollection<T>.Add(T item)
        {
            PushBack(item);
        }

        void ICollection<T>.Clear()
        {
            Clear();
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            fixed (T* dst = array)
            {
                MemcpyT(&pointer[arrayIndex], dst, array.Length);
            }
        }

        bool ICollection<T>.Remove(T item)
        {
            return Remove(item);
        }
    }
}