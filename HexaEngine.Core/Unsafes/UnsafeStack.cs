namespace HexaEngine.Core.Unsafes
{
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents an unsafe stack of elements of type T.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    public unsafe struct UnsafeStack<T> : IFreeable where T : unmanaged
    {
        private const int DefaultCapacity = 4;

        private Allocator* allocator;
        private T* data;
        private int size;
        private int capacity;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsafeStack{T}"/> struct with the default capacity.
        /// </summary>
        public UnsafeStack()
        {
            allocator = Allocator.Default;
            EnsureCapacity(DefaultCapacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsafeStack{T}"/> struct with a custom allocator and the default capacity.
        /// </summary>
        /// <param name="customAllocator">The custom allocator to use.</param>
        public UnsafeStack(Allocator* customAllocator)
        {
            allocator = customAllocator;
            EnsureCapacity(DefaultCapacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsafeStack{T}"/> struct with the specified capacity.
        /// </summary>
        /// <param name="capacity">The initial capacity of the stack.</param>
        public UnsafeStack(int capacity)
        {
            allocator = Allocator.Default;
            EnsureCapacity(capacity);
        }

        /// <summary>
        /// Gets the number of elements in the stack.
        /// </summary>
        public int Size => size;

        /// <summary>
        /// Gets or sets the capacity of the stack.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => data[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => data[index] = value;
        }

        /// <summary>
        /// Sets the allocator for the stack.
        /// </summary>
        /// <param name="allocator">The allocator to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAllocator(Allocator* allocator)
        {
            this.allocator = allocator;
        }

        /// <summary>
        /// Initializes the stack with the default capacity.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
        {
            Grow(DefaultCapacity);
        }

        /// <summary>
        /// Initializes the stack with the specified capacity.
        /// </summary>
        /// <param name="capacity">The initial capacity of the stack.</param>
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

        /// <summary>
        /// Ensures that the stack has the specified capacity.
        /// </summary>
        /// <param name="capacity">The desired capacity.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(int capacity)
        {
            if (this.capacity < capacity)
            {
                Grow(capacity);
            }
        }

        /// <summary>
        /// Pushes an item onto the stack.
        /// </summary>
        /// <param name="item">The item to push.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(T item)
        {
            EnsureCapacity(size + 1);
            data[size] = item;
            size++;
        }

        /// <summary>
        /// Removes and returns the item at the top of the stack.
        /// </summary>
        /// <returns>The item that was popped.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Pop()
        {
            var tmp = data[size - 1];
            data[size - 1] = default;
            size--;
            return tmp;
        }

        /// <summary>
        /// Returns the item at the top of the stack without removing it.
        /// </summary>
        /// <returns>The item at the top of the stack.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Peek()
        {
            return data[size - 1];
        }

        /// <summary>
        /// Attempts to remove and return the item at the top of the stack.
        /// </summary>
        /// <param name="value">When this method returns, contains the popped item, if the operation was successful.</param>
        /// <returns><c>true</c> if an item was successfully popped; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPop(out T value)
        {
            if (size == 0)
            {
                value = default;
                return false;
            }

            value = data[size - 1];
            data[size - 1] = default;
            size--;
            return true;
        }

        /// <summary>
        /// Attempts to retrieve the item at the top of the stack without removing it.
        /// </summary>
        /// <param name="value">When this method returns, contains the item at the top of the stack, if the operation was successful.</param>
        /// <returns><c>true</c> if an item was successfully retrieved; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeek(out T value)
        {
            if (size == 0)
            {
                value = default;
                return false;
            }

            value = data[size - 1];
            return true;
        }

        /// <summary>
        /// Copies the elements of the stack to the specified array.
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
        /// Copies a range of elements from the stack to the specified array.
        /// </summary>
        /// <param name="array">The destination array.</param>
        /// <param name="arrayIndex">The starting index in the destination array.</param>
        /// <param name="arraySize">The size of the destination array.</param>
        /// <param name="offset">The starting index in the stack.</param>
        /// <param name="count">The number of elements to copy.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T* array, uint arrayIndex, uint arraySize, uint offset, uint count)
        {
            Buffer.MemoryCopy(&data[offset], &array[arrayIndex], (arraySize - arrayIndex) * sizeof(T), (count - offset) * sizeof(T));
        }

        /// <summary>
        /// Removes all items from the stack.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            var ptr = data;
            for (int i = 0; i < size; i++)
            {
                ptr[i] = default;
            }
            size = 0;
        }

        /// <summary>
        /// Determines whether the stack contains the specified item.
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
        /// Releases the memory associated with the stack.
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