namespace HexaEngine.Core.Graphics.Buffers
{
    using HexaEngine.Core.Graphics;
    using System.Runtime.CompilerServices;

    public unsafe class IndexBuffer<T> : IIndexBuffer<T>, IBuffer where T : unmanaged
    {
        private const int DefaultCapacity = 8;

        private readonly IGraphicsDevice device;
        private readonly string dbgName;

        private IBuffer buffer;
        private BufferDescription description;

        private Format format;

        private T* items;
        private uint count;
        private uint capacity;

        private bool isDirty;

        private bool disposedValue;

        public IndexBuffer(IGraphicsDevice device, CpuAccessFlags flags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (typeof(T) != typeof(uint) && typeof(T) != typeof(ushort))
                throw new("Index buffers can only be type of uint or ushort");

            this.device = device;
            dbgName = $"IndexBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";

            items = Alloc<T>(DefaultCapacity);
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

            format = typeof(T) == typeof(uint) ? Format.R32UInt : Format.R16UInt;

            buffer = device.CreateBuffer(description);
            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);
        }

        public IndexBuffer(IGraphicsDevice device, T[] indices, CpuAccessFlags flags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (typeof(T) != typeof(uint) && typeof(T) != typeof(ushort))
                throw new("Index buffers can only be type of uint or ushort");

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

            format = typeof(T) == typeof(uint) ? Format.R32UInt : Format.R16UInt;

            items = AllocCopy(indices);
            buffer = device.CreateBuffer(items, capacity, description);
            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);
        }

        public IndexBuffer(IGraphicsDevice device, T* indices, uint count, CpuAccessFlags flags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (typeof(T) != typeof(uint) && typeof(T) != typeof(ushort))
                throw new("Index buffers can only be type of uint or ushort");

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

            format = typeof(T) == typeof(uint) ? Format.R32UInt : Format.R16UInt;

            items = AllocCopy(indices, count);
            buffer = device.CreateBuffer(items, capacity, description);
            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);
        }

        public IndexBuffer(IGraphicsDevice device, uint capacity, CpuAccessFlags flags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (typeof(T) != typeof(uint) && typeof(T) != typeof(ushort))
                throw new("Index buffers can only be type of uint or ushort");

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

            format = typeof(T) == typeof(uint) ? Format.R32UInt : Format.R16UInt;

            items = Alloc<T>(capacity);
            ZeroMemoryT(items, capacity);
            buffer = device.CreateBuffer(items, capacity, description);
            buffer.DebugName = dbgName;
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

        public uint Count => count;

        public uint Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => capacity;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                var tmp = Alloc<T>((int)value);
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

        public BufferDescription Description => buffer.Description;

        public int Length => buffer.Length;

        public ResourceDimension Dimension => buffer.Dimension;

        public nint NativePointer => buffer.NativePointer;

        public string? DebugName { get => buffer.DebugName; set => buffer.DebugName = value; }

        public bool IsDisposed => buffer.IsDisposed;

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

            if (newcapacity < capacity)
            {
                newcapacity = capacity;
            }

            Capacity = newcapacity;
        }

        public void Add(T value)
        {
            uint index = count;
            count++;
            EnsureCapacity(count);

            items[index] = value;

            isDirty = true;
        }

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

        public void RemoveAt(int index)
        {
            var size = (count - index) * sizeof(uint);
            Buffer.MemoryCopy(&items[index + 1], &items[index], size, size);
            isDirty = true;
        }

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

        public void Clear()
        {
            count = 0;
            isDirty = true;
        }

        public void FlushMemory()
        {
            Free(items);
        }

        public void Bind(IGraphicsContext context)
        {
            context.SetIndexBuffer(buffer, format, 0);
        }

        public void Bind(IGraphicsContext context, int offset)
        {
            context.SetIndexBuffer(buffer, format, offset);
        }

        public void Unbind(IGraphicsContext context)
        {
            context.SetIndexBuffer(null, Format.Unknown, 0);
        }

        public void CopyTo(IGraphicsContext context, IBuffer buffer)
        {
            context.CopyResource(buffer, this.buffer);
        }

        public void Dispose()
        {
            if (!disposedValue)
            {
                MemoryManager.Unregister(buffer);
                buffer?.Dispose();
                capacity = 0;
                count = 0;
                Free(items);

                disposedValue = true;
            }

            GC.SuppressFinalize(this);
        }
    }
}