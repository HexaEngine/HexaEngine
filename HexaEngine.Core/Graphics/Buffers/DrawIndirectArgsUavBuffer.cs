namespace HexaEngine.Core.Graphics.Buffers
{
    using System;
    using System.Runtime.CompilerServices;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;

    /// <summary>
    /// Represents a buffer designed for unordered access (UAV) and shader resource view (SRV) usage
    /// that can hold draw indirect arguments of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of draw indirect arguments that this buffer can hold.</typeparam>
    public unsafe class DrawIndirectArgsUavBuffer<T> : IBuffer where T : unmanaged
    {
        private const int DefaultCapacity = 64;
        private readonly bool canWrite;
        private readonly bool canRead;
        private readonly BufferUnorderedAccessViewFlags uavFlags;
        private readonly BufferExtendedShaderResourceViewFlags srvFlags;
        private readonly string dbgName;
        private BufferDescription bufferDescription;
        private BufferDescription copyDescription;
        private IBuffer buffer;
        private IBuffer? copyBuffer;
        private IUnorderedAccessView uav;
        private IShaderResourceView srv;

        private T* items;
        private uint count;
        private bool isDirty;
        private uint capacity;

        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawIndirectArgsUavBuffer{T}"/> class with default capacity and specified access flags.
        /// </summary>
        /// <param name="accessFlags">The CPU access flags for the buffer.</param>
        /// <param name="uavFlags">The unordered access view (UAV) flags for the buffer.</param>
        /// <param name="srvFlags">The shader resource view (SRV) flags for the buffer.</param>
        /// <param name="filename">The file name of the caller (automatically generated).</param>
        /// <param name="lineNumber">The line number in the source code of the caller (automatically generated).</param>
        public DrawIndirectArgsUavBuffer(CpuAccessFlags accessFlags, BufferUnorderedAccessViewFlags uavFlags = BufferUnorderedAccessViewFlags.None, BufferExtendedShaderResourceViewFlags srvFlags = BufferExtendedShaderResourceViewFlags.None, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            var device = Application.GraphicsDevice;
            canWrite = (accessFlags & CpuAccessFlags.Write) != 0;
            canRead = (accessFlags & CpuAccessFlags.Read) != 0;
            this.uavFlags = uavFlags;
            this.srvFlags = srvFlags;
            dbgName = $"DrawIndirectArgsUavBuffer: {filename}, Line:{lineNumber}";
            capacity = DefaultCapacity;
            items = AllocT<T>(DefaultCapacity);
            ZeroMemory(items, DefaultCapacity * sizeof(T));
            bufferDescription = new(sizeof(T) * DefaultCapacity, BindFlags.UnorderedAccess | BindFlags.ShaderResource, Usage.Default, CpuAccessFlags.None, ResourceMiscFlag.DrawIndirectArguments, sizeof(T));
            buffer = device.CreateBuffer(items, DefaultCapacity, bufferDescription);
            buffer.DebugName = dbgName;
            if (canWrite || canRead)
            {
                copyDescription = new(sizeof(T) * DefaultCapacity, BindFlags.ShaderResource, Usage.Dynamic, CpuAccessFlags.Write, ResourceMiscFlag.DrawIndirectArguments, sizeof(T));
                if (canRead)
                {
                    copyDescription.BindFlags = BindFlags.None;
                    copyDescription.Usage = Usage.Staging;
                    copyDescription.CPUAccessFlags = CpuAccessFlags.RW;
                }
                copyBuffer = device.CreateBuffer(copyDescription);
                copyBuffer.DebugName = dbgName + ".CopyBuffer";
            }

            uav = device.CreateUnorderedAccessView(buffer, new(buffer, Format.R32UInt, 0, (int)capacity * sizeof(T) / sizeof(uint), uavFlags));
            uav.DebugName = dbgName + ".UAV";
            srv = device.CreateShaderResourceView(buffer, new(buffer, Format.R32UInt, 0, (int)capacity * sizeof(T) / sizeof(uint), srvFlags));
            srv.DebugName = dbgName + ".SRV";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawIndirectArgsUavBuffer{T}"/> class with a specified initial capacity and access flags.
        /// </summary>
        /// <param name="initialCapacity">The initial capacity of the buffer.</param>
        /// <param name="accessFlags">The CPU access flags for the buffer.</param>
        /// <param name="uavFlags">The unordered access view (UAV) flags for the buffer.</param>
        /// <param name="srvFlags">The shader resource view (SRV) flags for the buffer.</param>
        /// <param name="filename">The file name of the caller (automatically generated).</param>
        /// <param name="lineNumber">The line number in the source code of the caller (automatically generated).</param>
        public DrawIndirectArgsUavBuffer(uint initialCapacity, CpuAccessFlags accessFlags, BufferUnorderedAccessViewFlags uavFlags = BufferUnorderedAccessViewFlags.None, BufferExtendedShaderResourceViewFlags srvFlags = BufferExtendedShaderResourceViewFlags.None, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            var device = Application.GraphicsDevice;
            canWrite = (accessFlags & CpuAccessFlags.Write) != 0;
            canRead = (accessFlags & CpuAccessFlags.Read) != 0;
            this.uavFlags = uavFlags;
            this.srvFlags = srvFlags;
            dbgName = $"DrawIndirectArgsUavBuffer: {filename}, Line:{lineNumber}";
            capacity = initialCapacity;
            items = AllocT<T>(initialCapacity);
            ZeroMemory(items, (int)initialCapacity * sizeof(T));
            bufferDescription = new(sizeof(T) * (int)initialCapacity, BindFlags.UnorderedAccess | BindFlags.ShaderResource, Usage.Default, CpuAccessFlags.None, ResourceMiscFlag.DrawIndirectArguments, sizeof(T));
            buffer = device.CreateBuffer(items, initialCapacity, bufferDescription);
            buffer.DebugName = dbgName;
            if (canWrite || canRead)
            {
                copyDescription = new(sizeof(T) * (int)initialCapacity, BindFlags.ShaderResource, Usage.Dynamic, CpuAccessFlags.Write, ResourceMiscFlag.DrawIndirectArguments, sizeof(T));
                if (canRead)
                {
                    copyDescription.BindFlags = BindFlags.None;
                    copyDescription.Usage = Usage.Staging;
                    copyDescription.CPUAccessFlags = CpuAccessFlags.RW;
                }
                copyBuffer = device.CreateBuffer(copyDescription);
                copyBuffer.DebugName = dbgName + ".CopyBuffer";
            }

            uav = device.CreateUnorderedAccessView(buffer, new(buffer, Format.R32UInt, 0, (int)capacity * sizeof(T) / sizeof(uint), uavFlags));
            uav.DebugName = dbgName + ".UAV";
            srv = device.CreateShaderResourceView(buffer, new(buffer, Format.R32UInt, 0, (int)capacity * sizeof(T) / sizeof(uint), srvFlags));
            srv.DebugName = dbgName + ".SRV";
        }

        /// <summary>
        /// Gets or sets an element at the specified index.
        /// </summary>
        /// <param name="index">The index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        public T this[int index]
        {
            get { return items[index]; }
            set { items[index] = value; isDirty = true; }
        }

        /// <summary>
        /// Gets or sets an element at the specified index.
        /// </summary>
        /// <param name="index">The index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        public T this[uint index]
        {
            get { return items[index]; }
            set { items[index] = value; isDirty = true; }
        }

        /// <summary>
        /// Gets a pointer to the local items.
        /// </summary>
        public T* Local => items;

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
        /// Gets the unordered access view (UAV) associated with this buffer.
        /// </summary>
        public IUnorderedAccessView UAV => uav;

        /// <summary>
        /// Gets the shader resource view (SRV) associated with this buffer.
        /// </summary>
        public IShaderResourceView SRV => srv;

        /// <summary>
        /// Gets the copy buffer associated with this buffer, if it can be read or written.
        /// </summary>
        public IBuffer? CopyBuffer => copyBuffer;

        /// <summary>
        /// Gets a value indicating whether this buffer can be written to.
        /// </summary>
        public bool CanWrite => canWrite;

        /// <summary>
        /// Gets a value indicating whether this buffer can be read from.
        /// </summary>
        public bool CanRead => canRead;

        /// <summary>
        /// Gets or sets the capacity of the buffer.
        /// </summary>
        /// <remarks>
        /// This property allows you to control the capacity of the buffer. Setting a new capacity will resize the buffer if needed.
        /// </remarks>
        public uint Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => capacity;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (value == 0)
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

                var tmp = AllocT<T>((int)value);
                ZeroMemory(tmp, DefaultCapacity * sizeof(T));
                var oldsize = count * sizeof(T);
                var newsize = value * sizeof(T);
                Buffer.MemoryCopy(items, tmp, newsize, oldsize > newsize ? newsize : oldsize);
                Free(items);
                items = tmp;
                capacity = value;
                count = capacity < count ? capacity : count;
                srv.Dispose();
                uav.Dispose();
                buffer.Dispose();
                copyBuffer?.Dispose();
                bufferDescription.ByteWidth = sizeof(T) * (int)capacity;
                var device = Application.GraphicsDevice;
                buffer = device.CreateBuffer(items, capacity, bufferDescription);
                buffer.DebugName = dbgName;
                if (canWrite || canRead)
                {
                    copyDescription.ByteWidth = sizeof(T) * (int)value;
                    copyBuffer = device.CreateBuffer(copyDescription);
                    copyBuffer.DebugName = dbgName + ".CopyBuffer";
                }

                uav = device.CreateUnorderedAccessView(buffer, new(buffer, Format.R32UInt, 0, (int)capacity * sizeof(T) / sizeof(uint), uavFlags));
                uav.DebugName = dbgName + ".UAV";
                srv = device.CreateShaderResourceView(buffer, new(buffer, Format.R32UInt, 0, (int)capacity * sizeof(T) / sizeof(uint), srvFlags));
                srv.DebugName = dbgName + ".SRV";
                isDirty = true;
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
        /// Gets the number of elements in the buffer.
        /// </summary>
        public uint Count => count;

        /// <summary>
        /// Gets the resource dimension of the buffer.
        /// </summary>
        public ResourceDimension Dimension => buffer.Dimension;

        /// <summary>
        /// Gets the native pointer associated with the buffer.
        /// </summary>
        public nint NativePointer => buffer.NativePointer;

        /// <summary>
        /// Gets or sets the debug name for the buffer.
        /// </summary>
        public string? DebugName { get => buffer.DebugName; set => buffer.DebugName = value; }

        /// <summary>
        /// Gets a value indicating whether the buffer is disposed.
        /// </summary>
        public bool IsDisposed => buffer.IsDisposed;

        /// <summary>
        /// Resets the counter to zero, effectively clearing the buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetCounter()
        {
            count = 0;
        }

        /// <summary>
        /// Clears the buffer by setting all elements to zero.
        /// </summary>
        public void Clear()
        {
            Memset(items, 0, capacity);
            count = 0;
        }

        /// <summary>
        /// Clears the buffer using an unordered access view with zeros.
        /// </summary>
        /// <param name="context">The graphics context used for clearing.</param>
        public void Clear(IGraphicsContext context)
        {
            context.ClearUnorderedAccessViewUint(uav, 0, 0, 0, 0);
            Clear();
        }

        /// <summary>
        /// Ensures that the buffer has at least the specified capacity.
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
        /// Expands the buffer's capacity to the specified value.
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
        /// Adds an element to the buffer and returns a reference to it.
        /// </summary>
        /// <param name="args">The element to add to the buffer.</param>
        /// <returns>A reference to the added element in the buffer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Add(T args)
        {
            var index = count;
            count++;
            EnsureCapacity(count);
            items[index] = args;
            isDirty = true;
            return ref items[index];
        }

        /// <summary>
        /// Removes an element from the buffer at the specified index.
        /// </summary>
        /// <param name="index">The index of the element to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(int index)
        {
            var size = (count - index) * sizeof(T);
            Buffer.MemoryCopy(&items[index + 1], &items[index], size, size);
            isDirty = true;
        }

        /// <summary>
        /// Removes an element from the buffer at the specified index.
        /// </summary>
        /// <param name="index">The index of the element to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(uint index)
        {
            var size = (count - index) * sizeof(T);
            Buffer.MemoryCopy(&items[index + 1], &items[index], size, size);
            isDirty = true;
        }

        /// <summary>
        /// Updates the buffer with its current data in the graphics context.
        /// </summary>
        /// <param name="context">The graphics context used for updating the buffer.</param>
        /// <returns>True if the buffer was updated, false if it was already up to date.</returns>
        public bool Update(IGraphicsContext context)
        {
            if (copyBuffer == null)
            {
                throw new InvalidOperationException("Copy buffer is not available.");
            }

            if (!canWrite)
            {
                throw new InvalidOperationException("The buffer is not writable.");
            }

            if (isDirty)
            {
                if (canRead)
                {
                    context.Write(copyBuffer, items, (int)(count * sizeof(T)), MapMode.Write);
                    context.CopyResource(buffer, copyBuffer);
                }
                else
                {
                    context.Write(copyBuffer, items, (int)(count * sizeof(T)), MapMode.WriteDiscard);
                    context.CopyResource(buffer, copyBuffer);
                }

                isDirty = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Reads data from the buffer into its local items using the graphics context.
        /// </summary>
        /// <param name="context">The graphics context used for reading from the buffer.</param>
        public void Read(IGraphicsContext context)
        {
            if (copyBuffer == null)
            {
                throw new InvalidOperationException("Copy buffer is not available.");
            }

            if (!canRead)
            {
                throw new InvalidOperationException("The buffer is not readable.");
            }

            context.CopyResource(copyBuffer, buffer);
            context.Read(copyBuffer, items, capacity);
        }

        /// <summary>
        /// Copies the data from this buffer to another resource using the graphics context.
        /// </summary>
        /// <param name="context">The graphics context used for copying the data.</param>
        /// <param name="resource">The destination resource to copy the data to.</param>
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
                srv.Dispose();
                uav.Dispose();
                buffer.Dispose();
                copyBuffer?.Dispose();
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