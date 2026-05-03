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
        private readonly string dbgName;

        private IBuffer buffer;
        private BufferDescription description;
        private uint capacity;

        private readonly Format format;
        private readonly IndexFormat indexFormat;

        private bool disposedValue;

        /// <summary>
        /// Creates a new instance of the <see cref="IndexBuffer{T}"/> class with the specified graphics device, indices, CPU access flags, filename, and line number.
        /// </summary>
        /// <param name="indices">The initial indices to populate the buffer with.</param>
        /// <param name="flags">The CPU access flags indicating the intended usage of the buffer.</param>
        /// <param name="filename">The filename of the source code file calling this constructor (for debugging purposes).</param>
        /// <param name="lineNumber">The line number in the source code file calling this constructor (for debugging purposes).</param>
        /// <exception cref="InvalidOperationException">Thrown if the type parameter is not uint or ushort.</exception>
        public IndexBuffer(ReadOnlySpan<T> indices, CpuAccessFlags flags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
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

            dbgName = $"IndexBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";

            capacity = (uint)indices.Length;

            var device = Application.GraphicsDevice;
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

            fixed (T* ptr = indices)
            {
                buffer = device.CreateBuffer(ptr, capacity, description);
            }

            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="IndexBuffer{T}"/> class with the specified graphics device, indices pointer, count, CPU access flags, filename, and line number.
        /// </summary>
        /// <param name="indices">The pointer to the initial indices to populate the buffer with.</param>
        /// <param name="count">The number of indices pointed to by the indices pointer.</param>
        /// <param name="flags">The CPU access flags indicating the intended usage of the buffer.</param>
        /// <param name="filename">The filename of the source code file calling this constructor (for debugging purposes).</param>
        /// <param name="lineNumber">The line number in the source code file calling this constructor (for debugging purposes).</param>
        /// <exception cref="InvalidOperationException">Thrown if the type parameter is not uint or ushort.</exception>
        public IndexBuffer(T* indices, uint count, CpuAccessFlags flags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
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

            dbgName = $"IndexBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";

            capacity = count;

            var device = Application.GraphicsDevice;
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

            buffer = device.CreateBuffer(indices, capacity, description);
            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="IndexBuffer{T}"/> class with the specified graphics device, capacity, CPU access flags, filename, and line number.
        /// </summary>
        /// <param name="capacity">The initial capacity of the index buffer.</param>
        /// <param name="flags">The CPU access flags indicating the intended usage of the buffer.</param>
        /// <param name="filename">The filename of the source code file calling this constructor (for debugging purposes).</param>
        /// <param name="lineNumber">The line number in the source code file calling this constructor (for debugging purposes).</param>
        /// <exception cref="InvalidOperationException">Thrown if the type parameter is not uint or ushort.</exception>
        public IndexBuffer(uint capacity, CpuAccessFlags flags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
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

            var device = Application.GraphicsDevice;
            buffer = device.CreateBuffer((T*)null, capacity, description);
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
        public uint Count => capacity;

        /// <summary>
        /// Resizes the buffer to the specified capacity and updates its contents with the provided items.
        /// </summary>
        /// <remarks>This method releases the existing buffer and creates a new one with the specified
        /// capacity. Any previous data in the buffer is discarded. Ensure that the capacity is appropriate for the
        /// intended usage to avoid resource allocation issues.</remarks>
        /// <param name="items">A pointer to the array of items to be stored in the buffer. The array must contain at least as many elements
        /// as specified by the capacity parameter.</param>
        /// <param name="capacity">The new capacity of the buffer, specified as an unsigned integer. Must be greater than zero.</param>
        public void Resize(T* items, uint capacity)
        {
            MemoryManager.Unregister(buffer);
            buffer.Dispose();
            var device = Application.GraphicsDevice;
            buffer = device.CreateBuffer(items, capacity, description);
            buffer.DebugName = dbgName;
            this.capacity = capacity;
            MemoryManager.Register(buffer);
        }

        /// <summary>
        /// Resizes the buffer to the specified capacity, releasing any existing resources and allocating a new buffer
        /// with the given size.
        /// </summary>
        /// <remarks>This method unregisters the current buffer from the memory manager, disposes of it,
        /// and creates a new buffer with the specified capacity. Ensure that the capacity is set appropriately to avoid
        /// runtime errors or resource leaks.</remarks>
        /// <param name="capacity">The new capacity for the buffer, specified as an unsigned integer. Must be greater than zero.</param>
        public void Resize(uint capacity)
        {
            MemoryManager.Unregister(buffer);
            buffer.Dispose();
            var device = Application.GraphicsDevice;
            buffer = device.CreateBuffer((T*)null, capacity, description);
            buffer.DebugName = dbgName;
            this.capacity = capacity;
            MemoryManager.Register(buffer);
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
        /// Updates the graphics buffer with the specified items using the provided graphics context.
        /// </summary>
        /// <param name="context">The graphics context used to perform the buffer update operation.</param>
        /// <param name="items">A pointer to the array of items to write to the buffer.</param>
        /// <param name="count">The number of items to write. Must not exceed the buffer's capacity.</param>
        public void Update(IGraphicsContext context, T* items, uint count)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(count, capacity);
            context.Write(buffer, items, (int)(count * sizeof(T)));
        }

        /// <summary>
        /// Updates the contents of the buffer with the specified data using the provided graphics context.
        /// </summary>
        /// <remarks>Throws an ArgumentOutOfRangeException if the specified size exceeds the buffer's
        /// capacity.</remarks>
        /// <param name="context">The graphics context used to write data to the buffer. This context must be valid and properly initialized.</param>
        /// <param name="data">A pointer to the data to be written to the buffer. The data must be formatted appropriately for the buffer's
        /// usage.</param>
        /// <param name="size">The size, in bytes, of the data to write. Must not exceed the total capacity of the buffer.</param>
        public void Update(IGraphicsContext context, void* data, uint size)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(size, capacity * (uint)sizeof(T));
            context.Write(buffer, data, (int)size);
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
                disposedValue = true;
            }

            GC.SuppressFinalize(this);
        }
    }
}