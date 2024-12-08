﻿namespace HexaEngine.Core.Graphics.Buffers
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
        private const int DefaultCapacity = 8;

        private readonly string dbgName;

        private IBuffer buffer;
        private BufferDescription description;

        private T* items;
        private uint count;
        private uint capacity;

        private bool isDirty;

        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexBuffer{T}"/> class with default capacity.
        /// </summary>
        /// <param name="flags">The CPU access flags for the buffer.</param>
        /// <param name="filename">The name of the file calling the constructor (for debugging purposes).</param>
        /// <param name="lineNumber">The line number in the file calling the constructor (for debugging purposes).</param>
        public VertexBuffer(CpuAccessFlags flags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            var device = Application.GraphicsDevice;
            dbgName = $"VertexBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";

            capacity = DefaultCapacity;

            description = new(sizeof(T) * DefaultCapacity, BindFlags.VertexBuffer, Usage.Default, flags);
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

            items = AllocT<T>(DefaultCapacity);
            ZeroMemoryT(items, (uint)DefaultCapacity);
            buffer = device.CreateBuffer(description);
            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexBuffer{T}"/> class with an array of vertices.
        /// </summary>
        /// <param name="vertices">An array of vertices to initialize the buffer.</param>
        /// <param name="flags">The CPU access flags for the buffer.</param>
        /// <param name="filename">The name of the file calling the constructor (for debugging purposes).</param>
        /// <param name="lineNumber">The line number in the file calling the constructor (for debugging purposes).</param>
        public VertexBuffer(T[] vertices, CpuAccessFlags flags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            var device = Application.GraphicsDevice;
            dbgName = $"VertexBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";

            capacity = (uint)vertices.Length;
            count = capacity;

            description = new(sizeof(T) * (int)capacity, BindFlags.VertexBuffer, Usage.Default, flags);
            if ((flags & CpuAccessFlags.None) != 0)
            {
                capacity = 0;
                description.Usage = Usage.Immutable;
                fixed (T* ptr = vertices)
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

            items = AllocCopyT(vertices);
            buffer = device.CreateBuffer(items, capacity, description);
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

            capacity = count;
            this.count = capacity;

            description = new(sizeof(T) * (int)capacity, BindFlags.VertexBuffer, Usage.Default, flags);
            if ((flags & CpuAccessFlags.None) != 0)
            {
                capacity = 0;
                description.Usage = Usage.Immutable;
                buffer = device.CreateBuffer(vertices, capacity, description);
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

            items = AllocCopyT(vertices, count);
            buffer = device.CreateBuffer(items, capacity, description);
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

            this.capacity = capacity;

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

            items = AllocT<T>(capacity);
            ZeroMemoryT(items, (uint)capacity);
            buffer = device.CreateBuffer(items, capacity, description);
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
        /// Gets the number of items in the buffer.
        /// </summary>
        public uint Count => count;

        /// <summary>
        /// Gets the stride of the buffer, which is the size of one element in bytes.
        /// </summary>
        public uint Stride { get; } = (uint)sizeof(T);

        /// <summary>
        /// Gets or sets the capacity of the buffer. Setting the capacity reallocates the buffer if needed.
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
                MemoryManager.Unregister(buffer);
                buffer.Dispose();
                var device = Application.GraphicsDevice;
                buffer = device.CreateBuffer(items, capacity, description);
                buffer.DebugName = dbgName;
                MemoryManager.Register(buffer);
            }
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
        /// The pointer to the internal buffer.
        /// </summary>
        void* IVertexBuffer.Items => items;

        /// <summary>
        /// The pointer to the internal buffer.
        /// </summary>
        public T* Items => items;

        /// <summary>
        /// Gets a value indicating whether the buffer has been disposed.
        /// </summary>
        public bool IsDisposed => buffer.IsDisposed;

        /// <summary>
        /// Gets or sets the element at the specified index.
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
        /// Resets the item count to zero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetCounter()
        {
            count = 0;
        }

        /// <summary>
        /// Clears the buffer, setting the item count to zero and marking it as dirty.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            count = 0;
            isDirty = true;
        }

        /// <summary>
        /// Erases the buffer by zeroing out its memory, setting the item count to zero, and marking it as dirty.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Erase()
        {
            ZeroMemoryT(items, capacity);
            count = 0;
            isDirty = true;
        }

        /// <summary>
        /// Ensures that the buffer has at least the specified capacity.
        /// </summary>
        /// <param name="capacity">The desired capacity of the buffer.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(uint capacity)
        {
            if (this.capacity < capacity)
            {
                Grow(capacity);
            }
        }

        /// <summary>
        /// Grows the buffer to the specified capacity.
        /// </summary>
        /// <param name="capacity">The desired capacity of the buffer.</param>
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
        /// Adds a single vertex to the buffer.
        /// </summary>
        /// <param name="vertex">The vertex to add.</param>
        public void Add(T vertex)
        {
            uint index = count;
            count++;
            EnsureCapacity(count);
            items[index] = vertex;
            isDirty = true;
        }

        /// <summary>
        /// Adds an array of vertices to the buffer.
        /// </summary>
        /// <param name="vertices">The array of vertices to add.</param>
        public void Add(params T[] vertices)
        {
            uint index = count;
            count += (uint)vertices.Length;
            EnsureCapacity(count);

            for (int i = 0; i < vertices.Length; i++)
            {
                items[index + i] = vertices[i];
            }

            isDirty = true;
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
        /// Updates the buffer in the specified graphics context if it is marked as dirty.
        /// </summary>
        /// <param name="context">The graphics context for updating the buffer.</param>
        /// <returns>True if the buffer was updated, false otherwise.</returns>
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
        /// Flushes the memory associated with the buffer.
        /// </summary>
        public void FlushMemory()
        {
            Free(items);
            items = null;
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

    /// <summary>
    /// Represents a vertex buffer for graphics rendering.
    /// </summary>
    public unsafe class VertexBuffer : IVertexBuffer, IBuffer
    {
        private const int DefaultCapacity = 8;

        private readonly string dbgName;

        private readonly int stride;
        private IBuffer buffer;
        private BufferDescription description;

        private void* items;
        private uint count;
        private uint capacity;

        private bool isDirty;

        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexBuffer{T}"/> class with default capacity.
        /// </summary>
        /// <param name="stride"></param>
        /// <param name="flags">The CPU access flags for the buffer.</param>
        /// <param name="filename">The name of the file calling the constructor (for debugging purposes).</param>
        /// <param name="lineNumber">The line number in the file calling the constructor (for debugging purposes).</param>
        public VertexBuffer(int stride, CpuAccessFlags flags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            var device = Application.GraphicsDevice;
            dbgName = $"VertexBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";

            capacity = DefaultCapacity;

            description = new(stride * DefaultCapacity, BindFlags.VertexBuffer, Usage.Default, flags);
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

            items = Alloc(stride * DefaultCapacity);
            ZeroMemory(items, (uint)DefaultCapacity * stride);
            buffer = device.CreateBuffer(description);
            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);
            this.stride = stride;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexBuffer{T}"/> class with a pointer to vertices.
        /// </summary>
        /// <param name="vertices">A pointer to the vertices to initialize the buffer.</param>
        /// <param name="stride"></param>
        /// <param name="count">The number of vertices in the buffer.</param>
        /// <param name="transferOwnership"></param>
        /// <param name="flags">The CPU access flags for the buffer.</param>
        /// <param name="filename">The name of the file calling the constructor (for debugging purposes).</param>
        /// <param name="lineNumber">The line number in the file calling the constructor (for debugging purposes).</param>
        public VertexBuffer(void* vertices, int stride, uint count, bool transferOwnership, CpuAccessFlags flags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            var device = Application.GraphicsDevice;
            dbgName = $"VertexBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";

            capacity = count;
            this.stride = stride;
            this.count = capacity;

            description = new(stride * (int)capacity, BindFlags.VertexBuffer, Usage.Default, flags);
            if ((flags & CpuAccessFlags.None) != 0)
            {
                capacity = 0;
                description.Usage = Usage.Immutable;
                buffer = device.CreateBuffer(vertices, stride, capacity, description);
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

            if (transferOwnership)
            {
                items = vertices;
            }
            else
            {
                items = AllocCopy(vertices, count * (uint)stride);
            }

            buffer = device.CreateBuffer(items, stride, capacity, description);
            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexBuffer{T}"/> class with a specified capacity.
        /// </summary>
        /// <param name="stride"></param>
        /// <param name="capacity">The initial capacity of the buffer.</param>
        /// <param name="flags">The CPU access flags for the buffer.</param>
        /// <param name="filename">The name of the file calling the constructor (for debugging purposes).</param>
        /// <param name="lineNumber">The line number in the file calling the constructor (for debugging purposes).</param>
        public VertexBuffer(int stride, uint capacity, CpuAccessFlags flags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            var device = Application.GraphicsDevice;
            dbgName = $"VertexBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";

            this.stride = stride;
            this.capacity = capacity;

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

            items = Alloc((int)capacity * stride);
            ZeroMemory(items, capacity * stride);
            buffer = device.CreateBuffer(items, stride, capacity, description);
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
        /// Gets the number of items in the buffer.
        /// </summary>
        public uint Count => count;

        /// <summary>
        /// Gets the stride of the buffer, which is the size of one element in bytes.
        /// </summary>
        public uint Stride => (uint)stride;

        /// <summary>
        /// Gets or sets the capacity of the buffer. Setting the capacity reallocates the buffer if needed.
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

                var tmp = Alloc((int)value * stride);
                var oldsize = count * stride;
                var newsize = value * stride;
                Buffer.MemoryCopy(items, tmp, newsize, oldsize > newsize ? newsize : oldsize);
                Free(items);
                items = tmp;
                capacity = value;
                count = capacity < count ? capacity : count;
                MemoryManager.Unregister(buffer);
                buffer.Dispose();
                var device = Application.GraphicsDevice;
                buffer = device.CreateBuffer(items, stride, capacity, description);
                buffer.DebugName = dbgName;
                MemoryManager.Register(buffer);
            }
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
        /// The pointer to the internal buffer.
        /// </summary>
        public void* Items => items;

        /// <summary>
        /// Resets the item count to zero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetCounter()
        {
            count = 0;
        }

        /// <summary>
        /// Clears the buffer, setting the item count to zero and marking it as dirty.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            count = 0;
            isDirty = true;
        }

        /// <summary>
        /// Erases the buffer by zeroing out its memory, setting the item count to zero, and marking it as dirty.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Erase()
        {
            ZeroMemory(items, capacity * stride);
            count = 0;
            isDirty = true;
        }

        /// <summary>
        /// Ensures that the buffer has at least the specified capacity.
        /// </summary>
        /// <param name="capacity">The desired capacity of the buffer.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(uint capacity)
        {
            if (this.capacity < capacity)
            {
                Grow(capacity);
            }
        }

        /// <summary>
        /// Grows the buffer to the specified capacity.
        /// </summary>
        /// <param name="capacity">The desired capacity of the buffer.</param>
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
        /// Adds a single vertex to the buffer.
        /// </summary>
        /// <param name="vertex">The vertex to add.</param>
        public void Add(void* vertex)
        {
            uint index = count;
            count++;
            EnsureCapacity(count);
            Memcpy(vertex, (byte*)items + index * stride, stride);
            isDirty = true;
        }

        /// <summary>
        /// Adds an array of vertices to the buffer.
        /// </summary>
        /// <param name="vertices">The array of vertices to add.</param>
        /// <param name="vertexCount"></param>
        public void Add(void* vertices, int vertexCount)
        {
            uint index = count;
            count += (uint)vertexCount;
            EnsureCapacity(count);

            Memcpy(vertices, (byte*)items + index * stride, stride * vertexCount);

            isDirty = true;
        }

        /// <summary>
        /// Removes the item at the specified index from the buffer.
        /// </summary>
        /// <param name="index">The index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            var size = (count - index) * stride;
            Buffer.MemoryCopy(&((byte*)items)[(index + 1) * stride], &((byte*)items)[index * stride], size, size);
            isDirty = true;
        }

        /// <summary>
        /// Updates the buffer in the specified graphics context if it is marked as dirty.
        /// </summary>
        /// <param name="context">The graphics context for updating the buffer.</param>
        /// <returns>True if the buffer was updated, false otherwise.</returns>
        public bool Update(IGraphicsContext context)
        {
            if (isDirty)
            {
                context.Write(buffer, items, (int)(count * stride));
                isDirty = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Flushes the memory associated with the buffer.
        /// </summary>
        public void FlushMemory()
        {
            Free(items);
            items = null;
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