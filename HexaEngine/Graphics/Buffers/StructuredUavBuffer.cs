namespace HexaEngine.Graphics.Buffers
{
    using HexaEngine.Core.Graphics;
    using System;
    using System.Runtime.CompilerServices;

    public unsafe class StructuredUavBuffer<T> : IBuffer where T : unmanaged
    {
        private const int DefaultCapacity = 64;
        private readonly IGraphicsDevice device;
        private readonly bool canWrite;
        private readonly BufferUnorderedAccessViewFlags flags;
        private IBuffer buffer;
        private IBuffer? copyBuffer;
        private IUnorderedAccessView uav;
        private IShaderResourceView srv;

        private T* items;
        private uint count;
        private bool isDirty;
        private uint capacity;

        private bool disposedValue;

        public StructuredUavBuffer(IGraphicsDevice device, bool canWrite, BufferUnorderedAccessViewFlags flags = BufferUnorderedAccessViewFlags.None)
        {
            this.device = device;
            this.canWrite = canWrite;
            this.flags = flags;
            capacity = DefaultCapacity;
            items = Alloc<T>(DefaultCapacity);
            Zero(items, DefaultCapacity * sizeof(T));
            buffer = device.CreateBuffer(items, DefaultCapacity, new(sizeof(T) * DefaultCapacity, BindFlags.UnorderedAccess | BindFlags.ShaderResource, Usage.Default, CpuAccessFlags.None, ResourceMiscFlag.BufferStructured, sizeof(T)));
            if (canWrite)
                copyBuffer = device.CreateBuffer(new(sizeof(T) * DefaultCapacity, BindFlags.ShaderResource, Usage.Dynamic, CpuAccessFlags.Write, ResourceMiscFlag.BufferStructured, sizeof(T)));
            uav = device.CreateUnorderedAccessView(buffer, new(buffer, Format.Unknown, 0, DefaultCapacity, flags));
            srv = device.CreateShaderResourceView(buffer);
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

        public IUnorderedAccessView UAV => uav;

        public IShaderResourceView SRV => srv;

        /// <summary>
        /// Only set if CanWrite is true
        /// </summary>
        public IBuffer? CopyBuffer => copyBuffer;

        public bool CanWrite => canWrite;

        public uint Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => capacity;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (value == 0) return;
                if (value == capacity) return;
                if (value < capacity) return;
                var tmp = Alloc<T>((int)value);
                Zero(tmp, DefaultCapacity * sizeof(T));
                var oldsize = count * sizeof(T);
                var newsize = value * sizeof(T);
                System.Buffer.MemoryCopy(items, tmp, newsize, oldsize > newsize ? newsize : oldsize);
                Free(items);
                items = tmp;
                capacity = value;
                count = capacity < count ? capacity : count;
                srv.Dispose();
                uav.Dispose();
                buffer.Dispose();
                copyBuffer?.Dispose();
                buffer = device.CreateBuffer(items, capacity, new(sizeof(T) * (int)capacity, BindFlags.UnorderedAccess | BindFlags.ShaderResource, Usage.Default, CpuAccessFlags.None, ResourceMiscFlag.BufferStructured, sizeof(T)));
                if (canWrite)
                    copyBuffer = device.CreateBuffer(new(sizeof(T) * (int)value, BindFlags.ShaderResource, Usage.Dynamic, CpuAccessFlags.Write, ResourceMiscFlag.BufferStructured, sizeof(T)));
                uav = device.CreateUnorderedAccessView(buffer, new(buffer, Format.Unknown, 0, (int)capacity, flags));
                srv = device.CreateShaderResourceView(buffer);
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
        /// Gets or sets the name of the debug.
        /// </summary>
        /// <value>
        /// The name of the debug.
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

            if (newcapacity < capacity) newcapacity = capacity;

            Capacity = newcapacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T args)
        {
            var index = count;
            count++;
            EnsureCapacity(count);
            items[index] = args;
            isDirty = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(int index)
        {
            var size = (count - index) * sizeof(T);
            System.Buffer.MemoryCopy(&items[index + 1], &items[index], size, size);
            isDirty = true;
        }

        public void Clear()
        {
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
            if (copyBuffer == null) throw new InvalidOperationException();
            if (isDirty)
            {
                context.Write(copyBuffer, items, (int)(count * sizeof(T)));
                context.CopyResource(buffer, copyBuffer);
                isDirty = false;
                return true;
            }
            return false;
        }

        public void CopyTo(IGraphicsContext context, IBuffer buffer)
        {
            context.CopyResource(buffer, this.buffer);
        }

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

        ~StructuredUavBuffer()
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