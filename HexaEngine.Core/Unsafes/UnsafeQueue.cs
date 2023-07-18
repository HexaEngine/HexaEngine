namespace HexaEngine.Core.Unsafes
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents an unsafe queue of elements of type T.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    public unsafe struct UnsafeQueue<T> : IFreeable where T : unmanaged
    {
        private const int DefaultCapacity = 4;

        private Allocator* allocator;
        private T* items;
        private T* head;
        private T* tail;
        private int size;
        private int capacity;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsafeQueue{T}"/> struct with the default capacity.
        /// </summary>
        public UnsafeQueue()
        {
            allocator = Allocator.Default;
            EnsureCapacity(DefaultCapacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsafeQueue{T}"/> struct with a custom allocator and the default capacity.
        /// </summary>
        /// <param name="customAllocator">The custom allocator to use.</param>
        public UnsafeQueue(Allocator* customAllocator)
        {
            allocator = customAllocator;
            EnsureCapacity(DefaultCapacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsafeQueue{T}"/> struct with the specified capacity.
        /// </summary>
        /// <param name="capacity">The initial capacity of the queue.</param>
        public UnsafeQueue(int capacity)
        {
            allocator = Allocator.Default;
            EnsureCapacity(capacity);
        }

        /// <summary>
        /// Gets the number of elements in the queue.
        /// </summary>
        public int Size => size;

        /// <summary>
        /// Gets or sets the capacity of the queue.
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
                Buffer.MemoryCopy(items, tmp, newsize, oldsize > newsize ? newsize : oldsize);
                allocator->Free(items);
                head = tail = items = tmp;
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
            get => items[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => items[index] = value;
        }

        /// <summary>
        /// Sets the allocator for the queue.
        /// </summary>
        /// <param name="allocator">The allocator to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAllocator(Allocator* allocator)
        {
            this.allocator = allocator;
        }

        /// <summary>
        /// Initializes the queue with the default capacity.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
        {
            Grow(DefaultCapacity);
        }

        /// <summary>
        /// Initializes the queue with the specified capacity.
        /// </summary>
        /// <param name="capacity">The initial capacity of the queue.</param>
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
        /// Ensures that the queue has the specified capacity.
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
        /// Adds an item to the end of the queue.
        /// </summary>
        /// <param name="item">The item to enqueue.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(T item)
        {
            EnsureCapacity(size + 1);
            head++;
            *head = item;
            size++;
        }

        /// <summary>
        /// Removes and returns the item at the front of the queue.
        /// </summary>
        /// <returns>The item that was dequeued.</returns>
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
            size--;
            return tmp;
        }

        /// <summary>
        /// Attempts to remove and return the item at the front of the queue.
        /// </summary>
        /// <param name="item">When this method returns, contains the dequeued item, if the operation was successful.</param>
        /// <returns><c>true</c> if an item was successfully dequeued; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out T item)
        {
            if (size == 0)
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
            size--;
            return true;
        }

        /// <summary>
        /// Copies the elements of the queue to the specified array.
        /// </summary>
        /// <param name="array">The destination array.</param>
        /// <param name="arrayIndex">The starting index in the destination array.</param>
        /// <param name="arraySize">The size of the destination array.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T* array, uint arrayIndex, uint arraySize)
        {
            Buffer.MemoryCopy(items, &array[arrayIndex], (arraySize - arrayIndex) * sizeof(T), size * sizeof(T));
        }

        /// <summary>
        /// Copies a range of elements from the queue to the specified array.
        /// </summary>
        /// <param name="array">The destination array.</param>
        /// <param name="arrayIndex">The starting index in the destination array.</param>
        /// <param name="arraySize">The size of the destination array.</param>
        /// <param name="offset">The starting index in the queue.</param>
        /// <param name="count">The number of elements to copy.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(T* array, uint arrayIndex, uint arraySize, uint offset, uint count)
        {
            Buffer.MemoryCopy(&items[offset], &array[arrayIndex], (arraySize - arrayIndex) * sizeof(T), (count - offset) * sizeof(T));
        }

        /// <summary>
        /// Clears the queue.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            var ptr = items;
            for (int i = 0; i < size; i++)
            {
                ptr[i] = default;
            }
            size = 0;
            head = tail = items;
        }

        /// <summary>
        /// Determines whether the queue contains the specified item.
        /// </summary>
        /// <param name="item">The item to search for.</param>
        /// <returns><c>true</c> if the item is found; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T* item)
        {
            for (int i = 0; i < size; i++)
            {
                var current = &items[i];
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
        /// Releases the memory associated with the queue.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release()
        {
            allocator->Free(items);
            items = null;
            head = null;
            tail = null;
            capacity = 0;
            size = 0;
        }
    }
}