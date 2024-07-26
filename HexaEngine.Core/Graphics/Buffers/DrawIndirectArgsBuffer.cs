namespace HexaEngine.Core.Graphics.Buffers
{
    using System;
    using System.Runtime.CompilerServices;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;

    /// <summary>
    /// Represents a buffer for draw indirect arguments in graphics rendering.
    /// </summary>
    /// <typeparam name="T">The type of elements stored in the buffer.</typeparam>
    public unsafe class DrawIndirectArgsBuffer<T> : IBuffer where T : unmanaged
    {
        private const int DefaultCapacity = 64;
        private readonly string dbgName;

        private BufferDescription description;

        private IBuffer buffer;

        private T* items;
        private uint count;
        private bool isDirty;
        private uint capacity;

        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawIndirectArgsBuffer{T}"/> class with default capacity and specified CPU access flags.
        /// </summary>
        /// <param name="cpuAccessFlags">The CPU access flags for the buffer.</param>
        /// <param name="filename">The name of the source file where the constructor is called (optional).</param>
        /// <param name="lineNumber">The line number in the source file where the constructor is called (optional).</param>
        public DrawIndirectArgsBuffer(CpuAccessFlags cpuAccessFlags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"DrawIndirectArgsBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            description = new(sizeof(T) * DefaultCapacity, BindFlags.ShaderResource, Usage.Default, cpuAccessFlags, ResourceMiscFlag.DrawIndirectArguments, sizeof(T));
            if (cpuAccessFlags.HasFlag(CpuAccessFlags.Write))
            {
                description.Usage = Usage.Dynamic;
            }
            if (cpuAccessFlags.HasFlag(CpuAccessFlags.Read))
            {
                description.Usage = Usage.Staging;
            }

            var device = Application.GraphicsDevice;

            if (cpuAccessFlags != CpuAccessFlags.None)
            {
                capacity = DefaultCapacity;
                items = AllocT<T>(capacity);
                ZeroMemoryT(items, capacity);
            }

            buffer = device.CreateBuffer(items, capacity, description);
            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawIndirectArgsBuffer{T}"/> class with specified initial capacity and CPU access flags.
        /// </summary>
        /// <param name="initialCapacity">The initial capacity of the buffer.</param>
        /// <param name="cpuAccessFlags">The CPU access flags for the buffer.</param>
        /// <param name="filename">The name of the source file where the constructor is called (optional).</param>
        /// <param name="lineNumber">The line number in the source file where the constructor is called (optional).</param>
        public DrawIndirectArgsBuffer(uint initialCapacity, CpuAccessFlags cpuAccessFlags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"DrawIndirectArgsBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            description = new(sizeof(T) * DefaultCapacity, BindFlags.ShaderResource, Usage.Default, cpuAccessFlags, ResourceMiscFlag.DrawIndirectArguments, sizeof(T));
            if (cpuAccessFlags.HasFlag(CpuAccessFlags.Write))
            {
                description.Usage = Usage.Dynamic;
            }
            if (cpuAccessFlags.HasFlag(CpuAccessFlags.Read))
            {
                description.Usage = Usage.Staging;
            }

            var device = Application.GraphicsDevice;
            if (cpuAccessFlags != CpuAccessFlags.None)
            {
                capacity = initialCapacity;
                items = AllocT<T>(capacity);
                ZeroMemoryT(items, capacity);
            }

            buffer = device.CreateBuffer(items, capacity, description);
            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawIndirectArgsBuffer{T}"/> class with a single initial data element and specified CPU access flags.
        /// </summary>
        /// <param name="initialData">The initial data to populate the buffer.</param>
        /// <param name="cpuAccessFlags">The CPU access flags for the buffer.</param>
        /// <param name="filename">The name of the source file where the constructor is called (optional).</param>
        /// <param name="lineNumber">The line number in the source file where the constructor is called (optional).</param>
        public DrawIndirectArgsBuffer(T initialData, CpuAccessFlags cpuAccessFlags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"DrawIndirectArgsBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            description = new(sizeof(T) * DefaultCapacity, BindFlags.ShaderResource, Usage.Default, cpuAccessFlags, ResourceMiscFlag.DrawIndirectArguments, sizeof(T));
            if (cpuAccessFlags.HasFlag(CpuAccessFlags.Write))
            {
                description.Usage = Usage.Dynamic;
            }
            if (cpuAccessFlags.HasFlag(CpuAccessFlags.Read))
            {
                description.Usage = Usage.Staging;
            }

            var device = Application.GraphicsDevice;
            if (cpuAccessFlags != CpuAccessFlags.None)
            {
                capacity = 1;
                items = AllocT<T>(capacity);
                items[0] = initialData;
                buffer = device.CreateBuffer(items, capacity, description);
            }
            else
            {
                buffer = device.CreateBuffer(&initialData, 1, description);
            }
            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawIndirectArgsBuffer{T}"/> class with an array of initial data elements, count, and specified CPU access flags.
        /// </summary>
        /// <param name="device">The graphics device associated with this buffer.</param>
        /// <param name="initialData">An array of initial data to populate the buffer.</param>
        /// <param name="count">The number of elements in the initial data array.</param>
        /// <param name="cpuAccessFlags">The CPU access flags for the buffer.</param>
        /// <param name="filename">The name of the source file where the constructor is called (optional).</param>
        /// <param name="lineNumber">The line number in the source file where the constructor is called (optional).</param>
        public DrawIndirectArgsBuffer(T* initialData, uint count, CpuAccessFlags cpuAccessFlags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"DrawIndirectArgsBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            description = new(sizeof(T) * DefaultCapacity, BindFlags.ShaderResource, Usage.Default, cpuAccessFlags, ResourceMiscFlag.DrawIndirectArguments, sizeof(T));
            if (cpuAccessFlags.HasFlag(CpuAccessFlags.Write))
            {
                description.Usage = Usage.Dynamic;
            }
            if (cpuAccessFlags.HasFlag(CpuAccessFlags.Read))
            {
                description.Usage = Usage.Staging;
            }

            var device = Application.GraphicsDevice;
            if (cpuAccessFlags != CpuAccessFlags.None)
            {
                capacity = count;
                items = AllocCopyT(initialData, count);
                buffer = device.CreateBuffer(items, capacity, description);
            }
            else
            {
                buffer = device.CreateBuffer(initialData, count, description);
            }
            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);
        }

        /// <summary>
        /// Occurs when the device child is disposed.
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
        /// Gets the underlying buffer used to store draw indirect arguments.
        /// </summary>
        public IBuffer Buffer => buffer;

        /// <summary>
        /// Gets or sets the capacity of the draw indirect arguments buffer.
        /// Adjusting the capacity reallocates the internal buffer if needed.
        /// </summary>
        /// <remarks>
        /// When setting the capacity to a smaller value, the existing data in the buffer remains intact, but extra capacity is freed.
        /// </remarks>
        public uint Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => capacity;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (value <= 0)
                {
                    return;
                }

                if (value == capacity)
                {
                    return;
                }

                if (value < capacity)
                {
                    return;
                }

                items = ReAllocT(items, value);
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
        /// Gets the description of the draw indirect arguments buffer.
        /// </summary>
        public BufferDescription Description => buffer.Description;

        /// <summary>
        /// Gets the length of the draw indirect arguments buffer.
        /// </summary>
        public int Length => buffer.Length;

        /// <summary>
        /// Gets the resource dimension of the draw indirect arguments buffer.
        /// </summary>
        public ResourceDimension Dimension => buffer.Dimension;

        /// <summary>
        /// Gets or sets the debug name of the draw indirect arguments buffer.
        /// </summary>
        public string? DebugName { get => buffer.DebugName; set => buffer.DebugName = value; }

        /// <summary>
        /// Indicates whether the draw indirect arguments buffer has been disposed.
        /// </summary>
        public bool IsDisposed => buffer.IsDisposed;

        /// <summary>
        /// Gets the native pointer to the draw indirect arguments buffer.
        /// </summary>
        public nint NativePointer => buffer.NativePointer;

        /// <summary>
        /// Resets the count of draw indirect arguments to zero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetCounter()
        {
            count = 0;
        }

        /// <summary>
        /// Clears the draw indirect arguments buffer, setting all elements to zero and resetting the count to zero.
        /// </summary>
        public void Clear()
        {
            MemsetT(items, 0, capacity);
            count = 0;
        }

        /// <summary>
        /// Ensures that the draw indirect arguments buffer has the specified capacity.
        /// If the current capacity is less than the specified value, the buffer is resized.
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
        /// Increases the capacity of the draw indirect arguments buffer.
        /// </summary>
        /// <param name="capacity">The new capacity for the buffer.</param>
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
        /// Adds a draw indirect argument to the buffer.
        /// </summary>
        /// <param name="args">The draw indirect argument to add.</param>
        public void Add(T args)
        {
            uint index = count;
            count++;
            EnsureCapacity(count);
            items[index] = args;
            isDirty = true;
        }

        /// <summary>
        /// Removes a draw indirect argument from the buffer at the specified index.
        /// </summary>
        /// <param name="index">The index of the draw indirect argument to remove.</param>
        public void Remove(int index)
        {
            var size = (count - index) * sizeof(T);
            System.Buffer.MemoryCopy(&items[index + 1], &items[index], size, size);
            isDirty = true;
        }

        /// <summary>
        /// Updates the contents of the buffer with the current draw indirect arguments.
        /// </summary>
        /// <param name="context">The graphics context used for the update operation.</param>
        /// <returns>True if the buffer was updated, false if no update was needed.</returns>
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
        /// Copies the contents of this buffer to another resource.
        /// </summary>
        /// <param name="context">The graphics context used for the copy operation.</param>
        /// <param name="resource">The destination resource to copy to.</param>
        public void CopyTo(IGraphicsContext context, IResource resource)
        {
            context.CopyResource(resource, buffer);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                MemoryManager.Unregister(buffer);
                if (items != null)
                {
                    Free(items);
                }
                buffer.Dispose();
                count = 0;
                capacity = 0;
                disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}