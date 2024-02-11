namespace HexaEngine.Core.Graphics.Buffers
{
    using HexaEngine.Core.Graphics;
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents an unordered access view buffer for graphics rendering.
    /// </summary>
    public unsafe class UavBuffer : IBuffer, IUavBuffer
    {
        private readonly IGraphicsDevice device;
        private readonly Format format;
        private readonly bool canWrite;
        private readonly bool canRead;
        private readonly int stride;
        private readonly BufferUnorderedAccessViewFlags uavflags;
        private readonly BufferExtendedShaderResourceViewFlags srvFlags;
        private readonly string dbgName;
        private BufferDescription bufferDescription;
        private BufferDescription copyDescription;
        private IBuffer buffer;
        private IBuffer? copyBuffer;
        private IUnorderedAccessView uav;
        private IShaderResourceView srv;

        private uint length;
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="UavBuffer"/> class.
        /// </summary>
        /// <param name="device">The graphics device associated with the buffer.</param>
        /// <param name="stride">The size, in bytes, of each element in the buffer.</param>
        /// <param name="length">The length of the buffer.</param>
        /// <param name="format">The format of the buffer elements.</param>
        /// <param name="canWrite">Indicates whether the buffer allows write access.</param>
        /// <param name="canRead">Indicates whether the buffer allows read access.</param>
        /// <param name="uavflags">Flags for unordered access views of the buffer.</param>
        /// <param name="srvFlags">Flags for extended shader resource views of the buffer.</param>
        /// <param name="filename">The name of the file calling the constructor (for debugging purposes).</param>
        /// <param name="lineNumber">The line number in the file calling the constructor (for debugging purposes).</param>
        public UavBuffer(IGraphicsDevice device, int stride, uint length, Format format, bool canWrite, bool canRead, BufferUnorderedAccessViewFlags uavflags = BufferUnorderedAccessViewFlags.None, BufferExtendedShaderResourceViewFlags srvFlags = BufferExtendedShaderResourceViewFlags.None, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            this.device = device;
            this.format = format;
            this.canWrite = canWrite;
            this.canRead = canRead;
            this.uavflags = uavflags;
            this.srvFlags = srvFlags;
            this.stride = stride;
            dbgName = $"UavBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            this.length = length;
            bufferDescription = new(stride * (int)length, BindFlags.UnorderedAccess | BindFlags.ShaderResource, Usage.Default, CpuAccessFlags.None, ResourceMiscFlag.None, 0);
            buffer = device.CreateBuffer(bufferDescription);
            buffer.DebugName = dbgName;
            if (canWrite)
            {
                copyDescription = new(stride * (int)length, BindFlags.ShaderResource, Usage.Dynamic, CpuAccessFlags.Write, ResourceMiscFlag.None, 0);
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

            uav = device.CreateUnorderedAccessView(buffer, new(buffer, format, 0, (int)length, uavflags));
            uav.DebugName = dbgName + ".UAV";
            srv = device.CreateShaderResourceView(buffer, new(buffer, format, 0, (int)length, srvFlags));
            srv.DebugName = dbgName + ".SRV";
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
        /// Gets the unordered access view associated with the buffer.
        /// </summary>
        public IUnorderedAccessView UAV => uav;

        /// <summary>
        /// Gets the shader resource view associated with the buffer.
        /// </summary>
        public IShaderResourceView SRV => srv;

        /// <summary>
        /// Gets the main buffer.
        /// </summary>
        public IBuffer Buffer => buffer;

        /// <summary>
        /// Gets the optional copy buffer for write operations.
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
        /// Gets or sets the length of the buffer. Setting the length may reallocate the buffer.
        /// </summary>
        public uint Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => length;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (value == 0)
                {
                    return;
                }

                if (value == length)
                {
                    return;
                }

                if (value < length)
                {
                    return;
                }

                length = value;
                srv.Dispose();
                uav.Dispose();
                buffer.Dispose();
                copyBuffer?.Dispose();
                MemoryManager.Unregister(buffer);
                bufferDescription.ByteWidth = stride * (int)length;
                buffer = device.CreateBuffer(bufferDescription);
                buffer.DebugName = dbgName;
                MemoryManager.Register(buffer);
                if (canWrite)
                {
                    MemoryManager.Unregister(copyBuffer);
                    copyDescription.ByteWidth = stride * (int)value;
                    copyBuffer = device.CreateBuffer(copyDescription);
                    copyBuffer.DebugName = dbgName + ".CopyBuffer";
                    MemoryManager.Register(copyBuffer);
                }

                uav = device.CreateUnorderedAccessView(buffer, new(buffer, format, 0, (int)length, uavflags));
                uav.DebugName = dbgName + ".UAV";
                srv = device.CreateShaderResourceView(buffer, new(buffer, format, 0, (int)length, srvFlags));
                srv.DebugName = dbgName + ".SRV";
            }
        }

        /// <summary>
        /// Gets the description of the buffer.
        /// </summary>
        public BufferDescription Description => buffer.Description;

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
        public string? DebugName
        {
            get => buffer.DebugName;
            set => buffer.DebugName = value;
        }

        /// <summary>
        /// Gets a value indicating whether the buffer has been disposed.
        /// </summary>
        public bool IsDisposed => buffer.IsDisposed;

        /// <summary>
        /// Gets the length of the buffer.
        /// </summary>
        int IBuffer.Length => buffer.Length;

        /// <summary>
        /// Gets the size, in bytes, of each element in the buffer.
        /// </summary>
        public int Stride => stride;

        /// <summary>
        /// Clears the unordered access view associated with the buffer.
        /// </summary>
        /// <param name="context">The graphics context for the clear operation.</param>
        public void Clear(IGraphicsContext context)
        {
            context.ClearUnorderedAccessViewUint(uav, 0, 0, 0, 0);
        }

        /// <summary>
        /// Writes data to the buffer from the specified source pointer.
        /// </summary>
        /// <param name="context">The graphics context for the write operation.</param>
        /// <param name="src">The source pointer to the data to be written.</param>
        /// <param name="length">The length of the data to be written, in bytes.</param>
        /// <exception cref="InvalidOperationException">Thrown if the buffer does not allow write access.</exception>
        public void Write(IGraphicsContext context, void* src, int length)
        {
            if (copyBuffer == null)
            {
                throw new InvalidOperationException();
            }

            if (!canWrite)
            {
                throw new InvalidOperationException();
            }

            if (canRead)
            {
                context.Write(copyBuffer, src, length, MapMode.Write);
                context.CopyResource(buffer, copyBuffer);
            }
            else
            {
                context.Write(copyBuffer, src, length);
                context.CopyResource(buffer, copyBuffer);
            }
        }

        /// <summary>
        /// Reads data from the buffer to the specified destination pointer.
        /// </summary>
        /// <param name="context">The graphics context for the read operation.</param>
        /// <param name="dst">The destination pointer to store the read data.</param>
        /// <param name="length">The length of the data to be read, in bytes.</param>
        /// <exception cref="InvalidOperationException">Thrown if the buffer does not allow read access.</exception>
        public void Read(IGraphicsContext context, void* dst, int length)
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
            context.Read(copyBuffer, dst, length);
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
        /// Releases the resources held by the buffer.
        /// </summary>
        /// <param name="disposing">True if called from the Dispose method; false if called from the finalizer.</param>
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
                length = 0;
                disposedValue = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="UavBuffer"/> class.
        /// </summary>
        ~UavBuffer()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        /// <summary>
        /// Releases the resources held by the buffer.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Represents a shader unordered access view buffer for graphics rendering with generic type elements.
    /// </summary>
    /// <typeparam name="T">The type of elements in the buffer. Must be an unmanaged type.</typeparam>
    public unsafe class UavBuffer<T> : IBuffer where T : unmanaged
    {
        private const int DefaultCapacity = 64;
        private readonly IGraphicsDevice device;
        private readonly Format format;
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
        private IShaderResourceView? srv;

        private T* items;
        private uint count;
        private bool isDirty;
        private uint capacity;

        private bool disposedValue;

        /// <param name="device">The graphics device associated with the buffer.</param>
        /// <param name="cpuAccessFlags">The CPU access flags for the buffer.</param>
        /// <param name="format">The format of the buffer elements.</param>
        /// <param name="uavFlags">Flags for unordered access views of the buffer.</param>
        /// <param name="hasSRV">Indicates whether the buffer has a shader resource view.</param>
        /// <param name="srvFlags">Flags for extended shader resource views of the buffer.</param>
        /// <param name="filename">The name of the file calling the constructor (for debugging purposes).</param>
        /// <param name="lineNumber">The line number in the file calling the constructor (for debugging purposes).</param>
        public UavBuffer(IGraphicsDevice device, CpuAccessFlags cpuAccessFlags, Format format, BufferUnorderedAccessViewFlags uavFlags = BufferUnorderedAccessViewFlags.None, bool hasSRV = false, BufferExtendedShaderResourceViewFlags srvFlags = BufferExtendedShaderResourceViewFlags.None, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
            : this(device, DefaultCapacity, cpuAccessFlags, format, uavFlags, hasSRV, srvFlags, filename, lineNumber)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UavBuffer{T}"/> class with a specified initial capacity.
        /// </summary>
        /// <param name="device">The graphics device associated with the buffer.</param>
        /// <param name="initialCapacity">The initial capacity of the buffer.</param>
        /// <param name="cpuAccessFlags">The CPU access flags for the buffer.</param>
        /// <param name="format">The format of the buffer elements.</param>
        /// <param name="uavFlags">Flags for unordered access views of the buffer.</param>
        /// <param name="hasSRV">Indicates whether the buffer has a shader resource view.</param>
        /// <param name="srvFlags">Flags for extended shader resource views of the buffer.</param>
        /// <param name="filename">The name of the file calling the constructor (for debugging purposes).</param>
        /// <param name="lineNumber">The line number in the file calling the constructor (for debugging purposes).</param>
        public UavBuffer(IGraphicsDevice device, int initialCapacity, CpuAccessFlags cpuAccessFlags, Format format, BufferUnorderedAccessViewFlags uavFlags = BufferUnorderedAccessViewFlags.None, bool hasSRV = false, BufferExtendedShaderResourceViewFlags srvFlags = BufferExtendedShaderResourceViewFlags.None, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            this.device = device;
            this.format = format;
            canWrite = (cpuAccessFlags & CpuAccessFlags.Write) != 0;
            canRead = (cpuAccessFlags & CpuAccessFlags.Read) != 0;
            this.uavFlags = uavFlags;
            this.srvFlags = srvFlags;

            BindFlags bindFlags = BindFlags.UnorderedAccess;
            if (hasSRV)
            {
                bindFlags |= BindFlags.ShaderResource;
            }

            ResourceMiscFlag miscFlag = ResourceMiscFlag.None;
            if ((uavFlags & BufferUnorderedAccessViewFlags.Raw) != 0 || (srvFlags & BufferExtendedShaderResourceViewFlags.Raw) != 0)
            {
                miscFlag = ResourceMiscFlag.BufferAllowRawViews;
            }

            dbgName = $"UavBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            capacity = (uint)initialCapacity;

            items = AllocT<T>(initialCapacity);
            ZeroMemory(items, initialCapacity * sizeof(T));

            bufferDescription = new(sizeof(T) * initialCapacity, bindFlags, Usage.Default, CpuAccessFlags.None, miscFlag, 0);
            buffer = device.CreateBuffer(items, (uint)initialCapacity, bufferDescription);
            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);

            // Ensures that no additional memory is used when not needed,
            // because the internal buffer is only required if reads or writes can happen.
            if (!canRead && !canWrite)
            {
                Free(items);
                items = null;
            }

            if (canWrite)
            {
                copyDescription = new(sizeof(T) * initialCapacity, BindFlags.ShaderResource, Usage.Dynamic, CpuAccessFlags.Write, ResourceMiscFlag.None, 0);
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
            else if (canRead)
            {
                copyDescription = new(sizeof(T) * initialCapacity, BindFlags.None, Usage.Staging, CpuAccessFlags.Read, ResourceMiscFlag.None, 0);
                copyBuffer = device.CreateBuffer(copyDescription);
                copyBuffer.DebugName = dbgName + ".CopyBuffer";
                MemoryManager.Register(copyBuffer);
            }

            uav = device.CreateUnorderedAccessView(buffer, new(buffer, format, 0, initialCapacity, uavFlags));
            uav.DebugName = dbgName + ".UAV";

            if (hasSRV)
            {
                srv = device.CreateShaderResourceView(buffer, new(buffer, format, 0, initialCapacity, srvFlags));
                srv.DebugName = dbgName + ".SRV";
            }
        }

        /// <summary>
        /// Gets or sets the element at the specified index in the <see cref="UavBuffer{T}"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <exception cref="InvalidOperationException">Thrown if the Uav buffer is not capable of writing or reading. Specify the write or read flag.</exception>
        public T this[int index]
        {
            get
            {
                if (!canWrite || !canRead)
                    throw new InvalidOperationException("The Uav buffer is not capable of writing or reading, please specify the write or read flag");

                return items[index];
            }
            set
            {
                if (!canWrite || !canRead)
                    throw new InvalidOperationException("The Uav buffer is not capable of writing or reading, please specify the write or read flag");

                items[index] = value; isDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the element at the specified index in the <see cref="UavBuffer{T}"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <exception cref="InvalidOperationException">Thrown if the Uav buffer is not capable of writing or reading. Specify the write or read flag.</exception>
        public T this[uint index]
        {
            get
            {
                if (!canWrite || !canRead)
                    throw new InvalidOperationException("The Uav buffer is not capable of writing or reading, please specify the write or read flag");

                return items[index];
            }
            set
            {
                if (!canWrite || !canRead)
                    throw new InvalidOperationException("The Uav buffer is not capable of writing or reading, please specify the write or read flag");

                items[index] = value; isDirty = true;
            }
        }

        /// <summary>
        /// Gets a pointer to the elements of the <see cref="UavBuffer{T}"/>.
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
        /// Gets the unordered access view associated with the <see cref="UavBuffer{T}"/>.
        /// </summary>
        public IUnorderedAccessView UAV => uav;

        /// <summary>
        /// Gets the shader resource view associated with the <see cref="UavBuffer{T}"/>.
        /// </summary>
        public IShaderResourceView? SRV => srv;

        /// <summary>
        /// Gets the copy buffer associated with the <see cref="UavBuffer{T}"/>.
        /// </summary>
        public IBuffer? CopyBuffer => copyBuffer;

        /// <summary>
        /// Gets a value indicating whether the <see cref="UavBuffer{T}"/> allows write access.
        /// </summary>
        public bool CanWrite => canWrite;

        /// <summary>
        /// Gets a value indicating whether the <see cref="UavBuffer{T}"/> allows read access.
        /// </summary>
        public bool CanRead => canRead;

        /// <summary>
        /// Gets or sets the capacity of the <see cref="UavBuffer{T}"/>.
        /// </summary>
        /// <remarks>
        /// If no read or write access is specified, the buffer is not created, and this property has no effect.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if the specified capacity is less than the current capacity.</exception>
        /// <param name="value">The new capacity for the buffer.</param>
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

                // Null check here because if no read or write access is specified we don't need that buffer
                if (items != null)
                {
                    var tmp = AllocT<T>((int)value);
                    ZeroMemory(tmp, DefaultCapacity * sizeof(T));
                    var oldSize = count * sizeof(T);
                    var newSize = value * sizeof(T);
                    Buffer.MemoryCopy(items, tmp, newSize, oldSize > newSize ? newSize : oldSize);
                    Free(items);
                    items = tmp;
                }

                capacity = value;
                count = capacity < count ? capacity : count;

                srv?.Dispose();
                uav.Dispose();
                buffer.Dispose();

                MemoryManager.Unregister(buffer);
                bufferDescription.ByteWidth = sizeof(T) * (int)capacity;
                buffer = device.CreateBuffer(items, capacity, bufferDescription);
                buffer.DebugName = dbgName;
                MemoryManager.Register(buffer);

                // canWrite/canRead check here because if no read or write access is specified we don't need that buffer
                if (canWrite || canRead)
                {
                    MemoryManager.Unregister(copyBuffer);
                    copyBuffer?.Dispose();
                    copyDescription.ByteWidth = sizeof(T) * (int)value;
                    copyBuffer = device.CreateBuffer(copyDescription);
                    copyBuffer.DebugName = dbgName + ".CopyBuffer";
                    MemoryManager.Register(copyBuffer);
                }

                uav = device.CreateUnorderedAccessView(buffer, new(buffer, format, 0, (int)capacity, uavFlags));
                uav.DebugName = dbgName + ".UAV";

                if ((bufferDescription.BindFlags & BindFlags.ShaderResource) != 0)
                {
                    srv = device.CreateShaderResourceView(buffer, new(buffer, format, 0, (int)capacity, srvFlags));
                    srv.DebugName = dbgName + ".SRV";
                }

                isDirty = true;
            }
        }

        /// <summary>
        /// Gets the description of the <see cref="UavBuffer{T}"/>.
        /// </summary>
        public BufferDescription Description => buffer.Description;

        /// <summary>
        /// Gets the length of the <see cref="UavBuffer{T}"/>.
        /// </summary>
        public int Length => buffer.Length;

        /// <summary>
        /// Gets the count of elements in the <see cref="UavBuffer{T}"/>.
        /// </summary>
        public uint Count => count;

        /// <summary>
        /// Gets the stride of elements in the <see cref="UavBuffer{T}"/>.
        /// </summary>
        public int Stride => sizeof(T);

        /// <summary>
        /// Gets the resource dimension of the <see cref="UavBuffer{T}"/>.
        /// </summary>
        public ResourceDimension Dimension => buffer.Dimension;

        /// <summary>
        /// Gets the native pointer of the <see cref="UavBuffer{T}"/>.
        /// </summary>
        public nint NativePointer => buffer.NativePointer;

        /// <summary>
        /// Gets or sets the debug name of the <see cref="UavBuffer{T}"/>.
        /// </summary>
        public string? DebugName
        {
            get => buffer.DebugName;
            set => buffer.DebugName = value;
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="UavBuffer{T}"/> is disposed.
        /// </summary>
        public bool IsDisposed => buffer.IsDisposed;

        /// <summary>
        /// Resets the counter of elements in the <see cref="UavBuffer{T}"/>.
        /// </summary>
        /// <remarks>
        /// Throws an <see cref="InvalidOperationException"/> if the Uav buffer is not capable of writing. Please specify the write flag.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetCounter()
        {
            if (!canWrite)
                throw new InvalidOperationException("The Uav buffer is not capable of writing, please specify the write flag");

            count = 0;
        }

        /// <summary>
        /// Clears the <see cref="UavBuffer{T}"/> by resetting the counter and marking the buffer as dirty.
        /// </summary>
        /// <remarks>
        /// Throws an <see cref="InvalidOperationException"/> if the Uav buffer is not capable of writing. Please specify the write flag.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (!canWrite)
                throw new InvalidOperationException("The Uav buffer is not capable of writing, please specify the write flag");

            count = 0;
            isDirty = true;
        }

        /// <summary>
        /// Erases the contents of the <see cref="UavBuffer{T}"/> by zeroing the memory, resetting the counter, and marking the buffer as dirty.
        /// </summary>
        /// <remarks>
        /// Throws an <see cref="InvalidOperationException"/> if the Uav buffer is not capable of writing. Please specify the write flag.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Erase()
        {
            ZeroMemoryT(items, capacity);
            count = 0;
            isDirty = true;
        }

        /// <summary>
        /// Ensures that the <see cref="UavBuffer{T}"/> has the specified capacity. Grows the capacity if needed.
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

        /// <summary>
        /// Grows the capacity of the <see cref="UavBuffer{T}"/>.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
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
        /// Adds an element to the <see cref="UavBuffer{T}"/> and returns a reference to the added element.
        /// </summary>
        /// <param name="args">The element to add.</param>
        /// <returns>A reference to the added element.</returns>
        /// <remarks>
        /// Throws an <see cref="InvalidOperationException"/> if the Uav buffer is not capable of writing. Please specify the write flag.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Add(T args)
        {
            if (!canWrite)
                throw new InvalidOperationException("The Uav buffer is not capable of writing, please specify the write flag");

            var index = count;
            count++;
            EnsureCapacity(count);
            items[index] = args;
            isDirty = true;
            return ref items[index];
        }

        /// <summary>
        /// Removes an element from the <see cref="UavBuffer{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">The index of the element to remove.</param>
        /// <remarks>
        /// Throws an <see cref="InvalidOperationException"/> if the Uav buffer is not capable of writing. Please specify the write flag.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index)
        {
            if (!canWrite)
                throw new InvalidOperationException("The Uav buffer is not capable of writing, please specify the write flag");

            var size = (count - index) * sizeof(T);
            Buffer.MemoryCopy(&items[index + 1], &items[index], size, size);
            isDirty = true;
        }

        /// <summary>
        /// Removes an element from the <see cref="UavBuffer{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">The index of the element to remove.</param>
        /// <remarks>
        /// Throws an <see cref="InvalidOperationException"/> if the Uav buffer is not capable of writing. Please specify the write flag.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(uint index)
        {
            if (!canWrite)
                throw new InvalidOperationException("The Uav buffer is not capable of writing, please specify the write flag");

            var size = (count - index) * sizeof(T);
            Buffer.MemoryCopy(&items[index + 1], &items[index], size, size);
            isDirty = true;
        }

        /// <summary>
        /// Clears the unordered access view of the <see cref="UavBuffer{T}"/> with uint values using the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context to use for clearing.</param>
        /// <remarks>
        /// Throws an <see cref="InvalidOperationException"/> if the Uav buffer is not capable of writing. Please specify the write flag.
        /// </remarks>
        public void Clear(IGraphicsContext context)
        {
            if (!canWrite)
                throw new InvalidOperationException("The Uav buffer is not capable of writing, please specify the write flag");

            context.ClearUnorderedAccessViewUint(uav, 0, 0, 0, 0);
        }

        /// <summary>
        /// Updates the <see cref="UavBuffer{T}"/> with the contents of the internal buffer using the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context to use for updating.</param>
        /// <returns>True if the buffer was updated, false otherwise.</returns>
        /// <remarks>
        /// Throws an <see cref="InvalidOperationException"/> if the Uav buffer is not capable of writing. Please specify the write flag.
        /// </remarks>
        public bool Update(IGraphicsContext context)
        {
            if (copyBuffer == null)
            {
                throw new InvalidOperationException("The Uav buffer is not capable of writing, please specify the write flag");
            }

            if (!canWrite)
            {
                throw new InvalidOperationException("The Uav buffer is not capable of writing, please specify the write flag");
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
                    context.Write(copyBuffer, items, (int)(count * sizeof(T)));
                    context.CopyResource(buffer, copyBuffer);
                }

                isDirty = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Reads the contents of the <see cref="UavBuffer{T}"/> from the GPU into the internal buffer using the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context to use for reading.</param>
        /// <remarks>
        /// Throws an <see cref="InvalidOperationException"/> if the Uav buffer is not capable of reading. Please specify the read flag.
        /// </remarks>
        public void Read(IGraphicsContext context)
        {
            if (copyBuffer == null)
            {
                throw new InvalidOperationException("The Uav buffer is not capable of reading, please specify the read flag");
            }

            if (!canRead)
            {
                throw new InvalidOperationException("The Uav buffer is not capable of reading, please specify the read flag");
            }

            context.CopyResource(copyBuffer, buffer);
            context.Read(copyBuffer, items, capacity);
        }

        /// <summary>
        /// Copies the contents of the <see cref="UavBuffer{T}"/> to another buffer using the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context to use for copying.</param>
        /// <param name="buffer">The destination buffer to copy to.</param>
        public void CopyTo(IGraphicsContext context, IBuffer buffer)
        {
            context.CopyResource(buffer, this.buffer);
        }

        /// <summary>
        /// Releases the resources used by the <see cref="UavBuffer{T}"/>.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                MemoryManager.Unregister(buffer);
                MemoryManager.Unregister(copyBuffer);
                if (items != null)
                {
                    Free(items);
                    items = null;
                }
                srv?.Dispose();
                uav.Dispose();
                buffer.Dispose();
                copyBuffer?.Dispose();
                count = 0;
                capacity = 0;
                disposedValue = true;
            }
        }

        /// <summary>
        /// Releases the resources used by the <see cref="UavBuffer{T}"/>.
        /// </summary>
        ~UavBuffer()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        /// <summary>
        /// Releases the resources used by the <see cref="UavBuffer{T}"/>.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}