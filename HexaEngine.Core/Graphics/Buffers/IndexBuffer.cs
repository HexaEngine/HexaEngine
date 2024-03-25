namespace HexaEngine.Core.Graphics.Buffers
{
    using HexaEngine.Core.Graphics;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents an index buffer in graphics programming with support for different index types (uint or ushort).
    /// </summary>
    /// <typeparam name="T">The type of indices in the buffer (must be unmanaged and either uint or ushort).</typeparam>
    public unsafe class IndexBuffer<T> : IIndexBuffer<T>, IIndexBuffer, IBuffer where T : unmanaged
    {
        private const int DefaultCapacity = 8;

        private readonly IGraphicsDevice device;
        private readonly string dbgName;

        private IBuffer buffer;
        private BufferDescription description;

        private readonly Format format;
        private readonly IndexFormat indexFormat;

        private T* items;
        private uint count;
        private uint capacity;

        private bool isDirty;

        private bool disposedValue;

        /// <summary>
        /// Creates a new instance of the <see cref="IndexBuffer{T}"/> class with the specified graphics device, CPU access flags, filename, and line number.
        /// </summary>
        /// <param name="device">The graphics device associated with the index buffer.</param>
        /// <param name="flags">The CPU access flags indicating the intended usage of the buffer.</param>
        /// <param name="filename">The filename of the source code file calling this constructor (for debugging purposes).</param>
        /// <param name="lineNumber">The line number in the source code file calling this constructor (for debugging purposes).</param>
        /// <exception cref="InvalidOperationException">Thrown if the type parameter is not uint or ushort.</exception>
        public IndexBuffer(IGraphicsDevice device, CpuAccessFlags flags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (typeof(T) == typeof(uint))
            {
                indexFormat = IndexFormat.UInt32;
            }
            else if (typeof(T) == typeof(ushort))
            {
                indexFormat = IndexFormat.UInt16;
            }
            else
            {
                throw new("Index buffers can only be type of uint or ushort");
            }

            this.device = device;
            dbgName = $"IndexBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";

            items = AllocT<T>(DefaultCapacity);
            ZeroMemoryT(items, (uint)DefaultCapacity);
            capacity = DefaultCapacity;

            description = new(sizeof(T) * DefaultCapacity, BindFlags.IndexBuffer, Usage.Default, flags);
            if ((flags & CpuAccessFlags.Write) != 0)
            {
                description.Usage = Usage.Dynamic;
            }
            if ((flags & CpuAccessFlags.Read) != 0)
            {
                description.Usage = Usage.Staging;
            }
            if ((flags & CpuAccessFlags.None) != 0)
            {
                throw new InvalidOperationException("If cpu access flags are none initial data must be provided");
            }

            format = typeof(T) == typeof(uint) ? Graphics.Format.R32UInt : Graphics.Format.R16UInt;

            buffer = device.CreateBuffer(description);
            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="IndexBuffer{T}"/> class with the specified graphics device, indices, CPU access flags, filename, and line number.
        /// </summary>
        /// <param name="device">The graphics device associated with the index buffer.</param>
        /// <param name="indices">The initial indices to populate the buffer with.</param>
        /// <param name="flags">The CPU access flags indicating the intended usage of the buffer.</param>
        /// <param name="filename">The filename of the source code file calling this constructor (for debugging purposes).</param>
        /// <param name="lineNumber">The line number in the source code file calling this constructor (for debugging purposes).</param>
        /// <exception cref="InvalidOperationException">Thrown if the type parameter is not uint or ushort.</exception>
        public IndexBuffer(IGraphicsDevice device, T[] indices, CpuAccessFlags flags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (typeof(T) == typeof(uint))
            {
                indexFormat = IndexFormat.UInt32;
            }
            else if (typeof(T) == typeof(ushort))
            {
                indexFormat = IndexFormat.UInt16;
            }
            else
            {
                throw new("Index buffers can only be type of uint or ushort");
            }

            this.device = device;
            dbgName = $"IndexBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";

            capacity = (uint)indices.Length;
            count = capacity;

            description = new(sizeof(T) * (int)capacity, BindFlags.IndexBuffer, Usage.Default, flags);

            if ((flags & CpuAccessFlags.None) != 0)
            {
                description.Usage = Usage.Immutable;
                fixed (T* ptr = indices)
                {
                    buffer = device.CreateBuffer(ptr, capacity, description);
                }
                return;
            }
            if ((flags & CpuAccessFlags.Write) != 0)
            {
                description.Usage = Usage.Dynamic;
            }
            if ((flags & CpuAccessFlags.Read) != 0)
            {
                description.Usage = Usage.Staging;
            }

            format = typeof(T) == typeof(uint) ? Graphics.Format.R32UInt : Graphics.Format.R16UInt;

            items = AllocCopyT(indices);
            buffer = device.CreateBuffer(items, capacity, description);
            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="IndexBuffer{T}"/> class with the specified graphics device, indices pointer, count, CPU access flags, filename, and line number.
        /// </summary>
        /// <param name="device">The graphics device associated with the index buffer.</param>
        /// <param name="indices">The pointer to the initial indices to populate the buffer with.</param>
        /// <param name="count">The number of indices pointed to by the indices pointer.</param>
        /// <param name="flags">The CPU access flags indicating the intended usage of the buffer.</param>
        /// <param name="filename">The filename of the source code file calling this constructor (for debugging purposes).</param>
        /// <param name="lineNumber">The line number in the source code file calling this constructor (for debugging purposes).</param>
        /// <exception cref="InvalidOperationException">Thrown if the type parameter is not uint or ushort.</exception>
        public IndexBuffer(IGraphicsDevice device, T* indices, uint count, CpuAccessFlags flags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (typeof(T) == typeof(uint))
            {
                indexFormat = IndexFormat.UInt32;
            }
            else if (typeof(T) == typeof(ushort))
            {
                indexFormat = IndexFormat.UInt16;
            }
            else
            {
                throw new("Index buffers can only be type of uint or ushort");
            }

            this.device = device;
            dbgName = $"IndexBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";

            capacity = count;
            this.count = count;

            description = new(sizeof(T) * (int)capacity, BindFlags.IndexBuffer, Usage.Default, flags);

            if ((flags & CpuAccessFlags.None) != 0)
            {
                description.Usage = Usage.Immutable;
                buffer = device.CreateBuffer(indices, capacity, description);
                return;
            }
            if ((flags & CpuAccessFlags.Write) != 0)
            {
                description.Usage = Usage.Dynamic;
            }
            if ((flags & CpuAccessFlags.Read) != 0)
            {
                description.Usage = Usage.Staging;
            }

            format = typeof(T) == typeof(uint) ? Graphics.Format.R32UInt : Graphics.Format.R16UInt;

            items = AllocCopyT(indices, count);
            buffer = device.CreateBuffer(items, capacity, description);
            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="IndexBuffer{T}"/> class with the specified graphics device, capacity, CPU access flags, filename, and line number.
        /// </summary>
        /// <param name="device">The graphics device associated with the index buffer.</param>
        /// <param name="capacity">The initial capacity of the index buffer.</param>
        /// <param name="flags">The CPU access flags indicating the intended usage of the buffer.</param>
        /// <param name="filename">The filename of the source code file calling this constructor (for debugging purposes).</param>
        /// <param name="lineNumber">The line number in the source code file calling this constructor (for debugging purposes).</param>
        /// <exception cref="InvalidOperationException">Thrown if the type parameter is not uint or ushort.</exception>
        public IndexBuffer(IGraphicsDevice device, uint capacity, CpuAccessFlags flags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (typeof(T) == typeof(uint))
            {
                indexFormat = IndexFormat.UInt32;
            }
            else if (typeof(T) == typeof(ushort))
            {
                indexFormat = IndexFormat.UInt16;
            }
            else
            {
                throw new("Index buffers can only be type of uint or ushort");
            }

            this.device = device;
            dbgName = $"IndexBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";

            this.capacity = capacity;

            description = new(sizeof(T) * (int)capacity, BindFlags.IndexBuffer, Usage.Default, flags);
            if ((flags & CpuAccessFlags.None) != 0)
            {
                throw new InvalidOperationException("If cpu access flags are none initial data must be provided");
            }
            if ((flags & CpuAccessFlags.Write) != 0)
            {
                description.Usage = Usage.Dynamic;
            }
            if ((flags & CpuAccessFlags.Read) != 0)
            {
                description.Usage = Usage.Staging;
            }

            format = typeof(T) == typeof(uint) ? Graphics.Format.R32UInt : Graphics.Format.R16UInt;

            items = AllocT<T>(capacity);
            ZeroMemoryT(items, capacity);
            buffer = device.CreateBuffer(items, capacity, description);
            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);
        }

        /// <summary>
        /// Occurs when the index buffer is disposed.
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
        /// Gets the number of indices in the buffer.
        /// </summary>
        public uint Count => count;

        /// <summary>
        /// Gets or sets the capacity of the index buffer.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the specified capacity is less than the current count.</exception>
        public uint Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => capacity;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (capacity == value)
                    return;

                var tmp = AllocT<T>((int)value);
                var oldsize = count * sizeof(uint);
                var newsize = value * sizeof(uint);
                Buffer.MemoryCopy(items, tmp, newsize, oldsize > newsize ? newsize : oldsize);
                Free(items);
                items = tmp;
                capacity = value;
                count = capacity < count ? capacity : count;
                MemoryManager.Unregister(buffer);
                buffer.Dispose();
                buffer = device.CreateBuffer(items, capacity, description);
                buffer.DebugName = dbgName;
                MemoryManager.Register(buffer);
            }
        }

        /// <summary>
        /// Gets the index buffer format.
        /// </summary>
        public IndexFormat Format => indexFormat;

        /// <summary>
        /// Gets the description of the buffer.
        /// </summary>
        public BufferDescription Description => buffer.Description;

        /// <summary>
        /// Gets the length of the buffer.
        /// </summary>
        public int Length => buffer.Length;

        /// <summary>
        /// Gets the resource dimension of the buffer.
        /// </summary>
        public ResourceDimension Dimension => buffer.Dimension;

        /// <summary>
        /// Gets the native pointer of the buffer.
        /// </summary>
        public nint NativePointer => buffer.NativePointer;

        /// <summary>
        /// Gets or sets the debug name of the buffer.
        /// </summary>
        public string? DebugName { get => buffer.DebugName; set => buffer.DebugName = value; }

        /// <summary>
        /// Gets a value indicating whether the underlying buffer is disposed.
        /// </summary>
        public bool IsDisposed => buffer.IsDisposed;

        /// <summary>
        /// Gets or sets the value at the specified index in the index buffer.
        /// </summary>
        /// <param name="index">The index of the value to get or set.</param>
        /// <returns>The value at the specified index.</returns>
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
        /// Resets the counter of indices to zero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetCounter()
        {
            count = 0;
        }

        /// <summary>
        /// Ensures that the index buffer has at least the specified capacity.
        /// </summary>
        /// <param name="capacity">The minimum capacity to ensure.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(uint capacity)
        {
            if (this.capacity < capacity)
            {
                Grow(capacity);
            }
        }

        /// <summary>
        /// Increases the capacity of the index buffer.
        /// </summary>
        /// <param name="capacity">The new capacity of the index buffer.</param>
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
        /// Adds a single index to the index buffer.
        /// </summary>
        /// <param name="value">The index value to add.</param>
        public void Add(T value)
        {
            uint index = count;
            count++;
            EnsureCapacity(count);

            items[index] = value;

            isDirty = true;
        }

        /// <summary>
        /// Adds multiple indices to the index buffer.
        /// </summary>
        /// <param name="indices">The indices to add.</param>
        public void Add(params T[] indices)
        {
            uint index = count;
            count += (uint)indices.Length;
            EnsureCapacity(count);

            for (int i = 0; i < indices.Length; i++)
            {
                items[index + i] = indices[i];
            }

            isDirty = true;
        }

        /// <summary>
        /// Removes the index at the specified position in the index buffer.
        /// </summary>
        /// <param name="index">The position of the index to remove.</param>
        public void RemoveAt(int index)
        {
            var size = (count - index) * sizeof(uint);
            Buffer.MemoryCopy(&items[index + 1], &items[index], size, size);
            isDirty = true;
        }

        /// <summary>
        /// Updates the index buffer in the graphics context if it is marked as dirty.
        /// </summary>
        /// <param name="context">The graphics context used for the update.</param>
        /// <returns>True if the index buffer was updated; otherwise, false.</returns>
        public bool Update(IGraphicsContext context)
        {
            if (isDirty)
            {
                context.Write(buffer, items, (int)(count * sizeof(uint)));
                isDirty = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Clears the index buffer, resetting the index count to zero.
        /// </summary>
        public void Clear()
        {
            count = 0;
            isDirty = true;
        }

        /// <summary>
        /// Flushes the memory associated with the index buffer.
        /// </summary>
        public void FlushMemory()
        {
            Free(items);
        }

        /// <summary>
        /// Binds the index buffer to the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context to bind the index buffer to.</param>
        public void Bind(IGraphicsContext context)
        {
            context.SetIndexBuffer(buffer, format, 0);
        }

        /// <summary>
        /// Binds the index buffer to the specified graphics context with the specified offset.
        /// </summary>
        /// <param name="context">The graphics context to bind the index buffer to.</param>
        /// <param name="offset">The offset within the index buffer to bind.</param>
        public void Bind(IGraphicsContext context, int offset)
        {
            context.SetIndexBuffer(buffer, format, offset);
        }

        /// <summary>
        /// Unbinds the index buffer from the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context to unbind the index buffer from.</param>
        public void Unbind(IGraphicsContext context)
        {
            context.SetIndexBuffer(null, Graphics.Format.Unknown, 0);
        }

        /// <summary>
        /// Copies the content of the index buffer to another buffer in the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context used for the copy operation.</param>
        /// <param name="buffer">The destination buffer to copy the content to.</param>
        public void CopyTo(IGraphicsContext context, IBuffer buffer)
        {
            context.CopyResource(buffer, this.buffer);
        }

        /// <summary>
        /// Disposes of the index buffer and releases associated resources.
        /// </summary>
        public void Dispose()
        {
            if (!disposedValue)
            {
                MemoryManager.Unregister(buffer);
                buffer?.Dispose();
                capacity = 0;
                count = 0;
                if (items != null)
                {
                    Free(items);
                    items = null;
                }

                disposedValue = true;
            }

            GC.SuppressFinalize(this);
        }
    }
}