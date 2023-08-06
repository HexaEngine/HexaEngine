namespace HexaEngine.Core.Graphics.Buffers
{
    using HexaEngine.Core.Graphics;
    using System;
    using System.Runtime.CompilerServices;

    public unsafe class UavBuffer : IBuffer, IUavBuffer
    {
        private readonly IGraphicsDevice device;
        private readonly Format format;
        private readonly bool canWrite;
        private readonly bool canRead;
#pragma warning disable CS0649 // Field 'UavBuffer.stride' is never assigned to, and will always have its default value 0
        private readonly int stride;
#pragma warning restore CS0649 // Field 'UavBuffer.stride' is never assigned to, and will always have its default value 0
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

        public UavBuffer(IGraphicsDevice device, int stride, uint length, Format format, bool canWrite, bool canRead, BufferUnorderedAccessViewFlags uavflags = BufferUnorderedAccessViewFlags.None, BufferExtendedShaderResourceViewFlags srvFlags = BufferExtendedShaderResourceViewFlags.None, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            this.device = device;
            this.format = format;
            this.canWrite = canWrite;
            this.canRead = canRead;
            this.uavflags = uavflags;
            this.srvFlags = srvFlags;
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
        /// Gets the uav.
        /// </summary>
        /// <value>
        /// The uav.
        /// </value>
        public IUnorderedAccessView UAV => uav;

        /// <summary>
        /// Gets the SRV.
        /// </summary>
        /// <value>
        /// The SRV.
        /// </value>
        public IShaderResourceView SRV => srv;

        public IBuffer Buffer => buffer;

        /// <summary>
        /// Not null when CanWrite is true
        /// </summary>
        public IBuffer? CopyBuffer => copyBuffer;

        /// <summary>
        /// Gets a value indicating whether this instance can write.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can write; otherwise, <c>false</c>.
        /// </value>
        public bool CanWrite => canWrite;

        /// <summary>
        /// Gets a value indicating whether this instance can read.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can read; otherwise, <c>false</c>.
        /// </value>
        public bool CanRead => canRead;

        /// <summary>
        /// Gets or sets the capacity.
        /// </summary>
        /// <value>
        /// The capacity.
        /// </value>
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
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public BufferDescription Description => buffer.Description;

        /// <summary>
        /// Gets the dimension.
        /// </summary>
        /// <value>
        /// The dimension.
        /// </value>
        public ResourceDimension Dimension => buffer.Dimension;

        /// <summary>
        /// Gets the native pointer.
        /// </summary>
        /// <value>
        /// The native pointer.
        /// </value>
        public nint NativePointer => buffer.NativePointer;

        /// <summary>
        /// Gets or sets the dbgName of the debug.
        /// </summary>
        /// <value>
        /// The dbgName of the debug.
        /// </value>
        public string? DebugName { get => buffer.DebugName; set => buffer.DebugName = value; }

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
        /// </value>
        public bool IsDisposed => buffer.IsDisposed;

        int IBuffer.Length => buffer.Length;

        public void Clear(IGraphicsContext context)
        {
            context.ClearUnorderedAccessViewUint(uav, 0, 0, 0, 0);
        }

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
                context.Write(copyBuffer, src, length, Map.Write);
                context.CopyResource(buffer, copyBuffer);
            }
            else
            {
                context.Write(copyBuffer, src, length);
                context.CopyResource(buffer, copyBuffer);
            }
        }

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

        public void CopyTo(IGraphicsContext context, IBuffer buffer)
        {
            context.CopyResource(buffer, this.buffer);
        }

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

        ~UavBuffer()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

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

        public UavBuffer(IGraphicsDevice device, CpuAccessFlags cpuAccessFlags, Format format, BufferUnorderedAccessViewFlags uavFlags = BufferUnorderedAccessViewFlags.None, bool hasSRV = false, BufferExtendedShaderResourceViewFlags srvFlags = BufferExtendedShaderResourceViewFlags.None, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
            : this(device, DefaultCapacity, cpuAccessFlags, format, uavFlags, hasSRV, srvFlags, filename, lineNumber)
        {
        }

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
            if (canRead || canWrite)
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
                copyDescription = new(sizeof(T) * initialCapacity, BindFlags.ShaderResource, Usage.Staging, CpuAccessFlags.Read, ResourceMiscFlag.None, 0);
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

        public T* Local => items;

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
        /// Gets the uav.
        /// </summary>
        /// <value>
        /// The uav.
        /// </value>
        public IUnorderedAccessView UAV => uav;

        /// <summary>
        /// Gets the SRV.
        /// </summary>
        /// <value>
        /// The SRV.
        /// </value>
        public IShaderResourceView? SRV => srv;

        /// <summary>
        /// Not null when CanWrite is true
        /// </summary>
        public IBuffer? CopyBuffer => copyBuffer;

        /// <summary>
        /// Gets a value indicating whether this instance can write.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can write; otherwise, <c>false</c>.
        /// </value>
        public bool CanWrite => canWrite;

        /// <summary>
        /// Gets a value indicating whether this instance can read.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can read; otherwise, <c>false</c>.
        /// </value>
        public bool CanRead => canRead;

        /// <summary>
        /// Gets or sets the capacity.
        /// </summary>
        /// <value>
        /// The capacity.
        /// </value>
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
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public BufferDescription Description => buffer.Description;

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public int Length => buffer.Length;

        /// <summary>
        /// Get the item count / counter of the buffer.
        /// </summary>
        public uint Count => count;

        /// <summary>
        /// Gets the dimension.
        /// </summary>
        /// <value>
        /// The dimension.
        /// </value>
        public ResourceDimension Dimension => buffer.Dimension;

        /// <summary>
        /// Gets the native pointer.
        /// </summary>
        /// <value>
        /// The native pointer.
        /// </value>
        public nint NativePointer => buffer.NativePointer;

        /// <summary>
        /// Gets or sets the dbgName of the debug.
        /// </summary>
        /// <value>
        /// The dbgName of the debug.
        /// </value>
        public string? DebugName { get => buffer.DebugName; set => buffer.DebugName = value; }

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
        /// </value>
        public bool IsDisposed => buffer.IsDisposed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetCounter()
        {
            if (!canWrite)
                throw new InvalidOperationException("The Uav buffer is not capable of writing, please specify the write flag");

            count = 0;
        }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(int index)
        {
            if (!canWrite)
                throw new InvalidOperationException("The Uav buffer is not capable of writing, please specify the write flag");

            var size = (count - index) * sizeof(T);
            Buffer.MemoryCopy(&items[index + 1], &items[index], size, size);
            isDirty = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(uint index)
        {
            if (!canWrite)
                throw new InvalidOperationException("The Uav buffer is not capable of writing, please specify the write flag");

            var size = (count - index) * sizeof(T);
            Buffer.MemoryCopy(&items[index + 1], &items[index], size, size);
            isDirty = true;
        }

        public void Clear()
        {
            if (!canWrite)
                throw new InvalidOperationException("The Uav buffer is not capable of writing, please specify the write flag");

            for (int i = 0; i < count; i++)
            {
                items[i] = default;
            }
            count = 0;
        }

        public void Clear(IGraphicsContext context)
        {
            context.ClearUnorderedAccessViewUint(uav, 0, 0, 0, 0);
        }

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
                    context.Write(copyBuffer, items, (int)(count * sizeof(T)), Map.Write);
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

        public void CopyTo(IGraphicsContext context, IBuffer buffer)
        {
            context.CopyResource(buffer, this.buffer);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                MemoryManager.Unregister(buffer);
                MemoryManager.Unregister(copyBuffer);
                if (items != null)
                {
                    Free(items);
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

        ~UavBuffer()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}