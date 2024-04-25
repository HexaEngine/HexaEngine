namespace HexaEngine.Core.Graphics.Buffers
{
    using HexaEngine.Core.Graphics;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a structured buffer in graphics memory containing elements of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements stored in the buffer. Must be an unmanaged type.</typeparam>
    public unsafe class StructuredBuffer<T> : IStructuredBuffer<T>, IBuffer where T : unmanaged
    {
        private const int DefaultCapacity = 128;

        private readonly string dbgName;
        private IBuffer buffer;
        private IShaderResourceView srv;
        private BufferDescription description;

        private T* items;
        private uint count;
        private volatile bool isDirty;
        private uint capacity;

        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="StructuredBuffer{T}"/> class with default capacity.
        /// </summary>
        /// <param name="cpuAccessFlags">The CPU access flags indicating how the CPU can access the buffer.</param>
        /// <param name="filename">The name of the file calling this constructor (automatically set by the compiler).</param>
        /// <param name="lineNumber">The line number in the file where this constructor is called (automatically set by the compiler).</param>
        public StructuredBuffer(CpuAccessFlags cpuAccessFlags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"StructuredBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            description = new(sizeof(T) * DefaultCapacity, BindFlags.ShaderResource, Usage.Default, cpuAccessFlags, ResourceMiscFlag.BufferStructured, sizeof(T));
            if (cpuAccessFlags.HasFlag(CpuAccessFlags.Write))
            {
                description.Usage = Usage.Dynamic;
            }
            if (cpuAccessFlags.HasFlag(CpuAccessFlags.Read))
            {
                description.Usage = Usage.Staging;
            }
            var device = Application.GraphicsDevice;
            capacity = DefaultCapacity;
            items = AllocT<T>(DefaultCapacity);
            ZeroMemory(items, DefaultCapacity * sizeof(T));
            buffer = device.CreateBuffer(items, DefaultCapacity, description);
            buffer.DebugName = dbgName;
            srv = device.CreateShaderResourceView(buffer);
            srv.DebugName = dbgName + ".SRV";
            MemoryManager.Register(buffer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StructuredBuffer{T}"/> class with a specified initial capacity.
        /// </summary>
        /// <param name="initialCapacity">The initial capacity of the buffer.</param>
        /// <param name="cpuAccessFlags">The CPU access flags indicating how the CPU can access the buffer.</param>
        /// <param name="filename">The name of the file calling this constructor (automatically set by the compiler).</param>
        /// <param name="lineNumber">The line number in the file where this constructor is called (automatically set by the compiler).</param>
        public StructuredBuffer(uint initialCapacity, CpuAccessFlags cpuAccessFlags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"StructuredBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            description = new(sizeof(T) * (int)initialCapacity, BindFlags.ShaderResource, Usage.Default, cpuAccessFlags, ResourceMiscFlag.BufferStructured, sizeof(T));
            if (cpuAccessFlags.HasFlag(CpuAccessFlags.Write))
            {
                description.Usage = Usage.Dynamic;
            }
            if (cpuAccessFlags.HasFlag(CpuAccessFlags.Read))
            {
                description.Usage = Usage.Staging;
            }
            var device = Application.GraphicsDevice;
            capacity = initialCapacity;
            items = AllocT<T>(initialCapacity);
            ZeroMemory(items, (int)initialCapacity * sizeof(T));
            buffer = device.CreateBuffer(items, initialCapacity, description);
            buffer.DebugName = dbgName;
            srv = device.CreateShaderResourceView(buffer);
            srv.DebugName = dbgName + ".SRV";
            MemoryManager.Register(buffer);
        }

        /// <summary>
        /// Event triggered when the buffer is disposed.
        /// </summary>
        public event EventHandler? OnDisposed
        {
            add
            {
                buffer.OnDisposed += value;
            }

            remove
            {
                buffer.OnDisposed -= value;
            }
        }

        /// <summary>
        /// Gets the shader resource view (SRV) associated with the buffer.
        /// </summary>
        public IShaderResourceView SRV => srv;

        /// <summary>
        /// Gets the number of elements currently in the buffer.
        /// </summary>
        public uint Count => count;

        /// <summary>
        /// Gets or sets the capacity (maximum number of elements) of the buffer.
        /// </summary>
        public uint Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => capacity;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (capacity == value)
                {
                    return;
                }

                var tmp = AllocT<T>((int)value);
                var oldsize = count * sizeof(T);
                var newsize = value * sizeof(T);
                Buffer.MemoryCopy(items, tmp, newsize, oldsize > newsize ? newsize : oldsize);
                Free(items);
                items = tmp;
                capacity = value;
                count = capacity < count ? capacity : count;
                srv.Dispose();
                MemoryManager.Unregister(buffer);
                buffer.Dispose();
                var device = Application.GraphicsDevice;
                buffer = device.CreateBuffer(items, capacity, description);
                buffer.DebugName = dbgName;
                MemoryManager.Register(buffer);
                srv = device.CreateShaderResourceView(buffer);
                srv.DebugName = dbgName + ".SRV";
            }
        }

        /// <summary>
        /// Gets the description of the buffer, specifying its size, usage, and other properties.
        /// </summary>
        public BufferDescription Description => buffer.Description;

        /// <summary>
        /// Gets the length, in bytes, of the buffer.
        /// </summary>
        public int Length => buffer.Length;

        /// <summary>
        /// Gets the dimension (type) of the resource.
        /// </summary>
        public ResourceDimension Dimension => buffer.Dimension;

        /// <summary>
        /// Gets the native pointer associated with the buffer.
        /// </summary>
        public nint NativePointer => buffer.NativePointer;

        /// <summary>
        /// Gets or sets the debug name of the buffer.
        /// </summary>
        public string? DebugName { get => buffer.DebugName; set => buffer.DebugName = value; }

        /// <summary>
        /// Gets a value indicating whether the buffer is disposed.
        /// </summary>
        public bool IsDisposed => buffer.IsDisposed;

        /// <summary>
        /// Gets or sets the element at the specified index in the buffer.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => items[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                items[index] = value;
                isDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the element at the specified index in the buffer.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        public T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => items[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                items[index] = value;
                isDirty = true;
            }
        }

        /// <summary>
        /// Resets the item counter to zero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetCounter()
        {
            count = 0;
        }

        /// <summary>
        /// Clears the buffer by resetting the item counter to zero and marking the buffer as dirty.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            count = 0;
            isDirty = true;
        }

        /// <summary>
        /// Erases the contents of the buffer by zeroing out the memory and resetting the item counter to zero and marking the buffer as dirty.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Erase()
        {
            ZeroMemoryT(items, capacity);
            count = 0;
            isDirty = true;
        }

        /// <summary>
        /// Ensures that the buffer has the specified capacity. If the current capacity is less than the specified capacity,
        /// the buffer is resized to accommodate the new capacity.
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Grow(uint capacity)
        {
            uint newcapacity = count == 0 ? DefaultCapacity : 2 * count;

            if (newcapacity < capacity)
            {
                newcapacity = capacity;
            }

            Capacity = newcapacity;
        }

        /// <summary>
        /// Adds an item to the buffer, updating the count and marking the buffer as dirty.
        /// </summary>
        /// <param name="item">The item to add to the buffer.</param>
        public void Add(T item)
        {
            uint index = count;
            count++;
            EnsureCapacity(count);
            items[index] = item;
            isDirty = true;
        }

        /// <summary>
        /// Removes the specified item from the buffer.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns><c>true</c> if the item is successfully removed; otherwise, <c>false</c>.</returns>
        public bool Remove(T item)
        {
            int idx = IndexOf(item);
            if (idx == -1)
            {
                return false;
            }
            RemoveAt(idx);
            isDirty = true;
            return true;
        }

        /// <summary>
        /// Determines whether the buffer contains the specified item.
        /// </summary>
        /// <param name="item">The item to locate in the buffer.</param>
        /// <returns><c>true</c> if the item is found in the buffer; otherwise, <c>false</c>.</returns>
        public bool Contains(T item)
        {
            for (int i = 0; i < count; i++)
            {
                var it = items[i];
                if (item.Equals(it))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the index of the specified item in the buffer.
        /// </summary>
        /// <param name="item">The item to locate in the buffer.</param>
        /// <returns>The index of the item if found; otherwise, -1.</returns>
        public int IndexOf(T item)
        {
            for (int i = 0; i < count; i++)
            {
                var it = items[i];
                if (item.Equals(it))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Removes the item at the specified index from the buffer.
        /// </summary>
        /// <param name="index">The index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            var size = (count - index) * sizeof(T);
            Buffer.MemoryCopy(&items[index + 1], &items[index], size, size);
            isDirty = true;
        }

        /// <summary>
        /// Updates the buffer data on the graphics context if the buffer is marked as dirty.
        /// </summary>
        /// <param name="context">The graphics context.</param>
        /// <returns><c>true</c> if the buffer data was updated; otherwise, <c>false</c>.</returns>
        public bool Update(IGraphicsContext context)
        {
            if (isDirty)
            {
                context.Write(buffer, items, (int)(count * sizeof(T)));
                isDirty = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Releases the resources held by the structured buffer.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                MemoryManager.Unregister(buffer);
                srv.Dispose();
                buffer.Dispose();
                if (items != null)
                {
                    Free(items);
                    items = null;
                }

                count = 0;
                capacity = 0;
                disposedValue = true;
            }
        }

        /// <summary>
        /// Releases the resources held by the structured buffer.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}