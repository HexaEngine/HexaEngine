namespace HexaEngine.Core.Graphics.Buffers
{
    using HexaEngine.Core.Graphics;
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a structured buffer with unordered access view (UAV) capabilities.
    /// </summary>
    /// <typeparam name="T">The type of elements stored in the buffer.</typeparam>
    public unsafe class StructuredUavBuffer<T> : IStructuredUavBuffer<T>, IBuffer where T : unmanaged
    {
        private const int DefaultCapacity = 64;
        private readonly IGraphicsDevice device;
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
        /// Initializes a new instance of the <see cref="StructuredUavBuffer{T}"/> class with default capacity.
        /// </summary>
        /// <param name="device">The graphics device used to create the buffer.</param>
        /// <param name="accessFlags">The CPU access flags for the buffer.</param>
        /// <param name="uavFlags">The unordered access view (UAV) flags for the buffer.</param>
        /// <param name="srvFlags">The extended shader resource view flags for the buffer.</param>
        /// <param name="filename">The name of the source file where the constructor is called.</param>
        /// <param name="lineNumber">The line number in the source file where the constructor is called.</param>
        public StructuredUavBuffer(IGraphicsDevice device, CpuAccessFlags accessFlags, BufferUnorderedAccessViewFlags uavFlags = BufferUnorderedAccessViewFlags.None, BufferExtendedShaderResourceViewFlags srvFlags = BufferExtendedShaderResourceViewFlags.None, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            this.device = device;
            canWrite = (accessFlags & CpuAccessFlags.Write) != 0;
            canRead = (accessFlags & CpuAccessFlags.Read) != 0;
            this.uavFlags = uavFlags;
            this.srvFlags = srvFlags;
            dbgName = $"StructuredUavBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            capacity = DefaultCapacity;
            items = AllocT<T>(DefaultCapacity);
            ZeroMemory(items, DefaultCapacity * sizeof(T));
            bufferDescription = new(sizeof(T) * DefaultCapacity, BindFlags.UnorderedAccess | BindFlags.ShaderResource, Usage.Default, CpuAccessFlags.None, ResourceMiscFlag.BufferStructured, sizeof(T));
            buffer = device.CreateBuffer(items, DefaultCapacity, bufferDescription);
            buffer.DebugName = dbgName;
            if (canWrite || canRead)
            {
                copyDescription = new(sizeof(T) * DefaultCapacity, BindFlags.ShaderResource, Usage.Dynamic, CpuAccessFlags.Write, ResourceMiscFlag.BufferStructured, sizeof(T));
                if (canRead)
                {
                    copyDescription.BindFlags = BindFlags.None;
                    copyDescription.Usage = Usage.Staging;
                    copyDescription.CPUAccessFlags = CpuAccessFlags.RW;
                }
                copyBuffer = device.CreateBuffer(copyDescription);
                copyBuffer.DebugName = dbgName + ".CopyBuffer";
                MemoryManager.Register(copyBuffer);
            }

            uav = device.CreateUnorderedAccessView(buffer, new(buffer, Format.Unknown, 0, (int)capacity, uavFlags));
            uav.DebugName = dbgName + ".UAV";
            srv = device.CreateShaderResourceView(buffer, new(buffer, Format.Unknown, 0, (int)capacity, srvFlags));
            srv.DebugName = dbgName + ".SRV";
            MemoryManager.Register(buffer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StructuredUavBuffer{T}"/> class with a specified initial capacity.
        /// </summary>
        /// <param name="device">The graphics device used to create the buffer.</param>
        /// <param name="initialCapacity">The initial capacity of the buffer.</param>
        /// <param name="accessFlags">The CPU access flags for the buffer.</param>
        /// <param name="uavFlags">The unordered access view (UAV) flags for the buffer.</param>
        /// <param name="srvFlags">The extended shader resource view flags for the buffer.</param>
        /// <param name="filename">The name of the source file where the constructor is called.</param>
        /// <param name="lineNumber">The line number in the source file where the constructor is called.</param>
        public StructuredUavBuffer(IGraphicsDevice device, uint initialCapacity, CpuAccessFlags accessFlags, BufferUnorderedAccessViewFlags uavFlags = BufferUnorderedAccessViewFlags.None, BufferExtendedShaderResourceViewFlags srvFlags = BufferExtendedShaderResourceViewFlags.None, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            this.device = device;
            canWrite = (accessFlags & CpuAccessFlags.Write) != 0;
            canRead = (accessFlags & CpuAccessFlags.Read) != 0;
            this.uavFlags = uavFlags;
            this.srvFlags = srvFlags;
            dbgName = $"StructuredUavBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            capacity = initialCapacity;

            if (accessFlags != CpuAccessFlags.None)
            {
                items = AllocT<T>(initialCapacity);
                ZeroMemory(items, (int)initialCapacity * sizeof(T));
            }

            bufferDescription = new(sizeof(T) * (int)initialCapacity, BindFlags.UnorderedAccess | BindFlags.ShaderResource, Usage.Default, CpuAccessFlags.None, ResourceMiscFlag.BufferStructured, sizeof(T));

            if (items != null)
            {
                buffer = device.CreateBuffer(items, initialCapacity, bufferDescription);
                buffer.DebugName = dbgName;
            }
            else
            {
                buffer = device.CreateBuffer(bufferDescription);
                buffer.DebugName = dbgName;
            }

            if (canWrite || canRead)
            {
                copyDescription = new(sizeof(T) * (int)initialCapacity, BindFlags.ShaderResource, Usage.Dynamic, CpuAccessFlags.Write, ResourceMiscFlag.BufferStructured, sizeof(T));
                if (canRead)
                {
                    copyDescription.BindFlags = BindFlags.None;
                    copyDescription.Usage = Usage.Staging;
                    copyDescription.CPUAccessFlags = CpuAccessFlags.RW;
                }
                copyBuffer = device.CreateBuffer(copyDescription);
                copyBuffer.DebugName = dbgName + ".CopyBuffer";
                MemoryManager.Register(copyBuffer);
            }

            uav = device.CreateUnorderedAccessView(buffer, new(buffer, Format.Unknown, 0, (int)initialCapacity, uavFlags));
            uav.DebugName = dbgName + ".UAV";
            srv = device.CreateShaderResourceView(buffer, new(buffer, Format.Unknown, 0, (int)initialCapacity, srvFlags));
            srv.DebugName = dbgName + ".SRV";
            MemoryManager.Register(buffer);
        }

        /// <summary>
        /// Gets or sets the value at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The value at the specified <paramref name="index"/>.</returns>
        /// <remarks>
        /// Setting a value at the specified index marks the buffer as dirty, indicating that it needs to be updated.
        /// </remarks>
        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return items[index];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                items[index] = value;
                isDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the value at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The value at the specified <paramref name="index"/>.</returns>
        /// <remarks>
        /// Setting a value at the specified index marks the buffer as dirty, indicating that it needs to be updated.
        /// </remarks>
        public T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return items[index];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                items[index] = value;
                isDirty = true;
            }
        }

        /// <summary>
        /// Gets the native pointer to the underlying array of items.
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
        /// Gets the unordered access view (UAV) associated with the buffer.
        /// </summary>
        public IUnorderedAccessView UAV => uav;

        /// <summary>
        /// Gets the shader resource view (SRV) associated with the buffer.
        /// </summary>
        public IShaderResourceView SRV => srv;

        /// <summary>
        /// Gets the copy buffer associated with the buffer.
        /// </summary>
        public IBuffer? CopyBuffer => copyBuffer;

        /// <summary>
        /// Gets a value indicating whether the buffer allows write access.
        /// </summary>
        public bool CanWrite => canWrite;

        /// <summary>
        /// Gets a value indicating whether the buffer allows read access.
        /// </summary>
        public bool CanRead => canRead;

        /// <summary>
        /// Gets or sets the capacity of the buffer. Setting a new capacity may result in resizing the buffer.
        /// </summary>
        public uint Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => capacity;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (items != null)
                {
                    var tmp = AllocT<T>((int)value);
                    ZeroMemory(tmp, DefaultCapacity * sizeof(T));
                    var oldsize = count * sizeof(T);
                    var newsize = value * sizeof(T);
                    Buffer.MemoryCopy(items, tmp, newsize, oldsize > newsize ? newsize : oldsize);
                    Free(items);
                    items = tmp;
                }

                capacity = value;
                count = capacity < count ? capacity : count;
                srv.Dispose();
                uav.Dispose();
                buffer.Dispose();
                copyBuffer?.Dispose();
                bufferDescription.ByteWidth = sizeof(T) * (int)capacity;
                MemoryManager.Unregister(buffer);
                buffer = device.CreateBuffer(items, capacity, bufferDescription);
                buffer.DebugName = dbgName;
                MemoryManager.Register(buffer);
                if (canWrite || canRead)
                {
                    MemoryManager.Unregister(copyBuffer);
                    copyDescription.ByteWidth = sizeof(T) * (int)value;
                    copyBuffer = device.CreateBuffer(copyDescription);
                    copyBuffer.DebugName = dbgName + ".CopyBuffer";
                    MemoryManager.Register(copyBuffer);
                }

                uav = device.CreateUnorderedAccessView(buffer, new(buffer, Format.Unknown, 0, (int)capacity, uavFlags));
                uav.DebugName = dbgName + ".UAV";
                srv = device.CreateShaderResourceView(buffer, new(buffer, Format.Unknown, 0, (int)capacity, srvFlags));
                srv.DebugName = dbgName + ".SRV";
                isDirty = true;
            }
        }

        /// <summary>
        /// Gets the description of the buffer.
        /// </summary>
        public BufferDescription Description => buffer.Description;

        /// <summary>
        /// Gets the length of the buffer in bytes.
        /// </summary>
        public int Length => buffer.Length;

        /// <summary>
        /// Gets the number of elements currently in the buffer.
        /// </summary>
        public uint Count => count;

        /// <summary>
        /// Gets the resource dimension of the buffer.
        /// </summary>
        public ResourceDimension Dimension => buffer.Dimension;

        /// <summary>
        /// Gets the native pointer to the underlying buffer.
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
        /// Resets the counter of items in the buffer to zero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetCounter()
        {
            count = 0;
        }

        /// <summary>
        /// Clears the buffer, setting the counter to zero and marking the buffer as dirty.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            count = 0;
            isDirty = true;
        }

        /// <summary>
        /// Erases all items in the buffer, setting the counter to zero and marking the buffer as dirty.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Erase()
        {
            ZeroMemoryT(items, capacity);
            count = 0;
            isDirty = true;
        }

        /// <summary>
        /// Ensures that the buffer has a capacity of at least the specified value.
        /// </summary>
        /// <param name="capacity">The minimum capacity to ensure for the buffer.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(uint capacity)
        {
            if (this.capacity < capacity)
            {
                Grow(capacity);
            }
        }

        /// <summary>
        /// Grows the buffer capacity to the specified value, ensuring it is at least twice the current count.
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
        /// Adds an item to the buffer and returns a reference to the added item.
        /// </summary>
        /// <param name="item">The item to add to the buffer.</param>
        /// <returns>A reference to the added item.</returns>
        public ref T Add(T item)
        {
            var index = count;
            count++;
            EnsureCapacity(count);
            items[index] = item;
            isDirty = true;
            return ref items[index];
        }

        /// <summary>
        /// Removes the specified item from the buffer.
        /// </summary>
        /// <param name="item">The item to remove from the buffer.</param>
        /// <returns>True if the item was successfully removed; otherwise, false.</returns>
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
        /// <returns>True if the item is found in the buffer; otherwise, false.</returns>
        public bool Contains(T item)
        {
            for (int i = 0; i < count; i++)
            {
                T it = items[i];
                if (item.Equals(it))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the index of the first occurrence of the specified item in the buffer.
        /// </summary>
        /// <param name="item">The item to locate in the buffer.</param>
        /// <returns>The index of the first occurrence of the item in the buffer, or -1 if the item is not found.</returns>
        public int IndexOf(T item)
        {
            for (int i = 0; i < count; i++)
            {
                T it = items[i];
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
        /// Removes the item at the specified index from the buffer.
        /// </summary>
        /// <param name="index">The index of the item to remove.</param>
        public void RemoveAt(uint index)
        {
            var size = (count - index) * sizeof(T);
            Buffer.MemoryCopy(&items[index + 1], &items[index], size, size);
            isDirty = true;
        }

        /// <summary>
        /// Clears the unordered access view of the buffer with the specified values.
        /// </summary>
        /// <param name="context">The graphics context used to perform the clear operation.</param>
        public void Clear(IGraphicsContext context)
        {
            context.ClearUnorderedAccessViewUint(uav, 0, 0, 0, 0);
        }

        /// <summary>
        /// Updates the buffer data on the graphics context. If the buffer is dirty, it writes the data to the copy buffer and copies it to the main buffer.
        /// </summary>
        /// <param name="context">The graphics context used to perform the update operation.</param>
        /// <returns>True if the buffer was updated, false otherwise.</returns>
        public bool Update(IGraphicsContext context)
        {
            if (copyBuffer == null)
            {
                throw new InvalidOperationException();
            }

            if (!canWrite)
            {
                throw new InvalidOperationException();
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
        /// Reads data from the buffer using the graphics context and copies it to the internal items array.
        /// </summary>
        /// <param name="context">The graphics context used to perform the read operation.</param>
        public void Read(IGraphicsContext context)
        {
            if (copyBuffer == null)
            {
                throw new InvalidOperationException();
            }

            if (!canRead)
            {
                throw new InvalidOperationException();
            }

            context.CopyResource(copyBuffer, buffer);
            context.Read(copyBuffer, items, capacity);
        }

        /// <summary>
        /// Copies the content of the buffer to another buffer using the graphics context.
        /// </summary>
        /// <param name="context">The graphics context used to perform the copy operation.</param>
        /// <param name="buffer">The destination buffer to copy the data to.</param>
        public void CopyTo(IGraphicsContext context, IBuffer buffer)
        {
            context.CopyResource(buffer, this.buffer);
        }

        /// <summary>
        /// Disposes of the buffer and associated resources.
        /// </summary>
        /// <param name="disposing">True if called from the Dispose method, false if called from the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                MemoryManager.Unregister(buffer);
                MemoryManager.Unregister(copyBuffer);
                srv.Dispose();
                uav.Dispose();
                buffer.Dispose();
                copyBuffer?.Dispose();

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
        /// Finalizes the object and releases resources if not already disposed.
        /// </summary>
        ~StructuredUavBuffer()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        /// <summary>
        /// Disposes of the buffer and associated resources, suppressing finalization.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}