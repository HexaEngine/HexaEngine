namespace HexaEngine.Core.Graphics.Buffers
{
    using HexaEngine.Core.Graphics;
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a generic vertex buffer for graphics rendering.
    /// </summary>
    /// <typeparam name="T">The type of elements in the buffer, must be unmanaged.</typeparam>
    public unsafe class VertexBuffer<T> : IVertexBuffer<T>, IVertexBuffer, IBuffer where T : unmanaged
    {
        private readonly string dbgName;

        private IBuffer buffer;
        private BufferDescription description;
        private uint count;

        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexBuffer{T}"/> class with an array of vertices.
        /// </summary>
        /// <param name="vertices">An array of vertices to initialize the buffer.</param>
        /// <param name="flags">The CPU access flags for the buffer.</param>
        /// <param name="filename">The name of the file calling the constructor (for debugging purposes).</param>
        /// <param name="lineNumber">The line number in the file calling the constructor (for debugging purposes).</param>
        public VertexBuffer(ReadOnlySpan<T> vertices, CpuAccessFlags flags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            var device = Application.GraphicsDevice;
            dbgName = $"VertexBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";

            count = (uint)vertices.Length;

            description = new(sizeof(T) * (int)count, BindFlags.VertexBuffer, Usage.Default, flags);
            if ((flags & CpuAccessFlags.None) != 0)
            {
                count = 0;
                description.Usage = Usage.Immutable;
                fixed (T* ptr = vertices)
                {
                    buffer = device.CreateBuffer(ptr, count, description);
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

            fixed (T* ptr = vertices)
                buffer = device.CreateBuffer(ptr, count, description);
            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexBuffer{T}"/> class with a pointer to vertices.
        /// </summary>
        /// <param name="vertices">A pointer to the vertices to initialize the buffer.</param>
        /// <param name="count">The number of vertices in the buffer.</param>
        /// <param name="flags">The CPU access flags for the buffer.</param>
        /// <param name="filename">The name of the file calling the constructor (for debugging purposes).</param>
        /// <param name="lineNumber">The line number in the file calling the constructor (for debugging purposes).</param>
        public VertexBuffer(T* vertices, uint count, CpuAccessFlags flags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            var device = Application.GraphicsDevice;
            dbgName = $"VertexBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";

            this.count = count;

            description = new(sizeof(T) * (int)this.count, BindFlags.VertexBuffer, Usage.Default, flags);
            if ((flags & CpuAccessFlags.None) != 0)
            {
                this.count = 0;
                description.Usage = Usage.Immutable;
                buffer = device.CreateBuffer(vertices, this.count, description);
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

            buffer = device.CreateBuffer(vertices, count, description);
            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexBuffer{T}"/> class with a specified capacity.
        /// </summary>
        /// <param name="capacity">The initial capacity of the buffer.</param>
        /// <param name="flags">The CPU access flags for the buffer.</param>
        /// <param name="filename">The name of the file calling the constructor (for debugging purposes).</param>
        /// <param name="lineNumber">The line number in the file calling the constructor (for debugging purposes).</param>
        public VertexBuffer(uint capacity, CpuAccessFlags flags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            var device = Application.GraphicsDevice;
            dbgName = $"VertexBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";

            this.count = capacity;

            description = new(sizeof(T) * (int)capacity, BindFlags.VertexBuffer, Usage.Default, flags);
            if (flags.HasFlag(CpuAccessFlags.Write))
            {
                description.Usage = Usage.Dynamic;
            }
            if (flags.HasFlag(CpuAccessFlags.Read))
            {
                description.Usage = Usage.Staging;
            }
            if (flags.HasFlag(CpuAccessFlags.None))
            {
                throw new InvalidOperationException("If cpu access flags are none initial data must be provided");
            }

            buffer = device.CreateBuffer((T*)null, capacity, description);
            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);
        }

        /// <summary>
        /// Occurs when the buffer is disposed.
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
        /// Gets the stride of the buffer, which is the size of one element in bytes.
        /// </summary>
        public uint Stride { get; } = (uint)sizeof(T);

        /// <summary>
        /// Gets or sets the capacity of the buffer. Setting the capacity reallocates the buffer if needed.
        /// </summary>
        public uint Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => count;
        }

        public void Resize(T* items, uint newCapacity)
        {
            MemoryManager.Unregister(buffer);
            buffer.Dispose();
            var device = Application.GraphicsDevice;
            buffer = device.CreateBuffer(items, newCapacity, description);
            count = newCapacity;
            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);
        }

        public void Resize(void* items, uint newCapacity)
        {
            Resize((T*)items, newCapacity / (uint)sizeof(T));
        }

        /// <summary>
        /// Gets the description of the buffer.
        /// </summary>
        public BufferDescription Description => buffer.Description;

        /// <summary>
        /// Gets the length of the buffer.
        /// </summary>
        public int Length => buffer.Length;

        /// <summary>
        /// Gets the dimension of the buffer resource.
        /// </summary>
        public ResourceDimension Dimension => buffer.Dimension;

        /// <summary>
        /// Gets the native pointer to the underlying buffer resource.
        /// </summary>
        public nint NativePointer => buffer.NativePointer;

        /// <summary>
        /// Gets or sets the debug name of the buffer.
        /// </summary>
        public string? DebugName { get => buffer.DebugName; set => buffer.DebugName = value; }

        /// <summary>
        /// Gets a value indicating whether the buffer has been disposed.
        /// </summary>
        public bool IsDisposed => buffer.IsDisposed;

        /// <summary>
        /// Updates the buffer in the specified graphics context if it is marked as dirty.
        /// </summary>
        /// <param name="context">The graphics context for updating the buffer.</param>
        /// <param name="items"></param>
        /// <param name="count"></param>
        /// <returns>True if the buffer was updated, false otherwise.</returns>
        public void Update(IGraphicsContext context, T* items, uint count)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(count, this.count);
            context.Write(buffer, items, (int)(count * sizeof(T)));
        }

        public void Update(IGraphicsContext context, void* data, uint size)
        {
            Update(context, (T*)data, size / (uint)sizeof(T));
        }

        /// <summary>
        /// Copies the contents of this buffer to another buffer in the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context for the copy operation.</param>
        /// <param name="buffer">The destination buffer for the copy operation.</param>
        public void CopyTo(IGraphicsContext context, IBuffer buffer)
        {
            context.CopyResource(buffer, this.buffer);
        }

        /// <summary>
        /// Binds the buffer to the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context for binding the buffer.</param>
        public void Bind(IGraphicsContext context)
        {
            context.SetVertexBuffer(buffer, (uint)sizeof(T));
        }

        /// <summary>
        /// Unbinds the buffer from the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context for unbinding the buffer.</param>
        public void Unbind(IGraphicsContext context)
        {
            context.SetVertexBuffer(null, 0);
        }

        /// <summary>
        /// Disposes of the buffer and releases associated resources.
        /// </summary>
        public void Dispose()
        {
            if (!disposedValue)
            {
                MemoryManager.Unregister(buffer);
                buffer?.Dispose();
                count = 0;

                disposedValue = true;
            }

            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Represents a vertex buffer for graphics rendering.
    /// </summary>
    public unsafe class VertexBuffer : IVertexBuffer, IBuffer
    {
        private readonly string dbgName;

        private readonly int stride;
        private IBuffer buffer;
        private BufferDescription description;
        private uint count;

        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexBuffer"/> class with a pointer to vertices.
        /// </summary>
        /// <param name="vertices">A pointer to the vertices to initialize the buffer.</param>
        /// <param name="stride">The stride (size in bytes) of each vertex.</param>
        /// <param name="count">The number of vertices in the buffer.</param>
        /// <param name="flags">The CPU access flags for the buffer.</param>
        /// <param name="filename">The name of the file calling the constructor (for debugging purposes).</param>
        /// <param name="lineNumber">The line number in the file calling the constructor (for debugging purposes).</param>
        public VertexBuffer(void* vertices, int stride, uint count, CpuAccessFlags flags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            var device = Application.GraphicsDevice;
            dbgName = $"VertexBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";

            this.stride = stride;
            this.count = count;

            description = new(stride * (int)count, BindFlags.VertexBuffer, Usage.Default, flags);
            if ((flags & CpuAccessFlags.None) != 0)
            {
                this.count = 0;
                description.Usage = Usage.Immutable;
                buffer = device.CreateBuffer(vertices, stride, count, description);
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

            buffer = device.CreateBuffer(vertices, stride, count, description);
            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexBuffer"/> class with a specified capacity.
        /// </summary>
        /// <param name="stride">The stride (size in bytes) of each vertex.</param>
        /// <param name="capacity">The initial capacity of the buffer.</param>
        /// <param name="flags">The CPU access flags for the buffer.</param>
        /// <param name="filename">The name of the file calling the constructor (for debugging purposes).</param>
        /// <param name="lineNumber">The line number in the file calling the constructor (for debugging purposes).</param>
        public VertexBuffer(int stride, uint capacity, CpuAccessFlags flags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            var device = Application.GraphicsDevice;
            dbgName = $"VertexBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";

            this.stride = stride;
            count = capacity;

            description = new(stride * (int)capacity, BindFlags.VertexBuffer, Usage.Default, flags);
            if (flags.HasFlag(CpuAccessFlags.Write))
            {
                description.Usage = Usage.Dynamic;
            }
            if (flags.HasFlag(CpuAccessFlags.Read))
            {
                description.Usage = Usage.Staging;
            }
            if (flags.HasFlag(CpuAccessFlags.None))
            {
                throw new InvalidOperationException("If cpu access flags are none initial data must be provided");
            }

            buffer = device.CreateBuffer((void*)null, stride, capacity, description);
            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);
        }

        /// <summary>
        /// Occurs when the buffer is disposed.
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
        /// Gets the number of vertices in the buffer.
        /// </summary>
        public uint Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => count;
        }

        /// <summary>
        /// Gets the stride of the buffer, which is the size of one element in bytes.
        /// </summary>
        public uint Stride => (uint)stride;

        public void Resize(void* items, uint newCapacity)
        {
            MemoryManager.Unregister(buffer);
            buffer.Dispose();
            var device = Application.GraphicsDevice;
            description = new(stride * (int)newCapacity, description.BindFlags, description.Usage, description.CPUAccessFlags);
            buffer = device.CreateBuffer(items, stride, newCapacity, description);
            count = newCapacity;
            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);
        }

        /// <summary>
        /// Gets the description of the buffer.
        /// </summary>
        public BufferDescription Description => buffer.Description;

        /// <summary>
        /// Gets the length of the buffer.
        /// </summary>
        public int Length => buffer.Length;

        /// <summary>
        /// Gets the dimension of the buffer resource.
        /// </summary>
        public ResourceDimension Dimension => buffer.Dimension;

        /// <summary>
        /// Gets the native pointer to the underlying buffer resource.
        /// </summary>
        public nint NativePointer => buffer.NativePointer;

        /// <summary>
        /// Gets or sets the debug name of the buffer.
        /// </summary>
        public string? DebugName { get => buffer.DebugName; set => buffer.DebugName = value; }

        /// <summary>
        /// Gets a value indicating whether the buffer has been disposed.
        /// </summary>
        public bool IsDisposed => buffer.IsDisposed;

        /// <summary>
        /// Updates the buffer with new data.
        /// </summary>
        /// <param name="context">The graphics context for updating the buffer.</param>
        /// <param name="items">Pointer to the vertex data.</param>
        /// <param name="count">The number of vertices to write.</param>
        public void Update(IGraphicsContext context, void* items, uint count)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(count, this.count);
            context.Write(buffer, items, (int)(count * stride));
        }

        /// <summary>
        /// Copies the contents of this buffer to another buffer in the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context for the copy operation.</param>
        /// <param name="buffer">The destination buffer for the copy operation.</param>
        public void CopyTo(IGraphicsContext context, IBuffer buffer)
        {
            context.CopyResource(buffer, this.buffer);
        }

        /// <summary>
        /// Binds the buffer to the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context for binding the buffer.</param>
        public void Bind(IGraphicsContext context)
        {
            context.SetVertexBuffer(buffer, (uint)stride);
        }

        /// <summary>
        /// Unbinds the buffer from the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context for unbinding the buffer.</param>
        public void Unbind(IGraphicsContext context)
        {
            context.SetVertexBuffer(null, 0);
        }

        /// <summary>
        /// Disposes of the buffer and releases associated resources.
        /// </summary>
        public void Dispose()
        {
            if (!disposedValue)
            {
                MemoryManager.Unregister(buffer);
                buffer?.Dispose();
                count = 0;

                disposedValue = true;
            }

            GC.SuppressFinalize(this);
        }
    }
}