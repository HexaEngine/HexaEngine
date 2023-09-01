namespace HexaEngine.Core.Graphics.Buffers
{
    using System;
    using System.Runtime.CompilerServices;

    public unsafe class ConstantBuffer<T> : IConstantBuffer, IConstantBuffer<T>, IBuffer where T : unmanaged
    {
        private readonly string dbgName;
        private readonly IGraphicsDevice device;
        private readonly BufferDescription description;
        private IBuffer buffer;
        private T* items;
        private uint count;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantBuffer{T}"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="length">The length.</param>
        /// <param name="accessFlags">The access flags.</param>
        private ConstantBuffer(IGraphicsDevice device, CpuAccessFlags accessFlags, uint length, string filename, int lineNumber)
        {
            dbgName = $"ConstantBuffer: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            this.device = device;
            description = new(0, BindFlags.ConstantBuffer, Usage.Default, accessFlags, ResourceMiscFlag.None);

            if (accessFlags != CpuAccessFlags.None)
            {
                count = length;
                items = AllocT<T>(length);
                ZeroMemoryT(items, length);
            }

            description.Usage = accessFlags switch
            {
                CpuAccessFlags.Write => Usage.Dynamic,
                CpuAccessFlags.Read => Usage.Staging,
                CpuAccessFlags.None => Usage.Immutable,
                CpuAccessFlags.RW => Usage.Staging,
                _ => throw new ArgumentException("Invalid CpuAccessFlags", nameof(accessFlags)),
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantBuffer{T}"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="length">The length.</param>
        /// <param name="accessFlags">The access flags.</param>
        public ConstantBuffer(IGraphicsDevice device, uint length, CpuAccessFlags accessFlags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0) : this(device, accessFlags, length, filename, lineNumber)
        {
            buffer = device.CreateBuffer(items, length, description);
            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantBuffer{T}"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="values">The values.</param>
        /// <param name="accessFlags">The access flags.</param>
        public ConstantBuffer(IGraphicsDevice device, T[] values, CpuAccessFlags accessFlags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0) : this(device, accessFlags, (uint)values.Length, filename, lineNumber)
        {
            fixed (T* src = values)
            {
                if (description.CPUAccessFlags != CpuAccessFlags.None)
                {
                    int size = (int)(count * sizeof(T));
                    System.Buffer.MemoryCopy(src, items, size, size);
                }

                buffer = device.CreateBuffer(src, count, description);
                buffer.DebugName = dbgName;
            }
            MemoryManager.Register(buffer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantBuffer{T}"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="value">The values.</param>
        /// <param name="accessFlags">The access flags.</param>
        public ConstantBuffer(IGraphicsDevice device, T value, CpuAccessFlags accessFlags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0) : this(device, accessFlags, 1, filename, lineNumber)
        {
            count = 1;
            T* src = &value;
            if (description.CPUAccessFlags != CpuAccessFlags.None)
            {
                int size = (int)(count * sizeof(T));
                System.Buffer.MemoryCopy(src, items, size, size);
            }

            buffer = device.CreateBuffer(src, count, description);
            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantBuffer{T}"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="accessFlags">The access flags.</param>
        /// <exception cref="ArgumentException"></exception>
        public ConstantBuffer(IGraphicsDevice device, CpuAccessFlags accessFlags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0) : this(device, accessFlags, 1, filename, lineNumber)
        {
            buffer = device.CreateBuffer(items, 1, description);
            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantBuffer{T}"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="cpuAccessFlags">The access flags.</param>
        /// <exception cref="ArgumentException"></exception>
        public ConstantBuffer(IGraphicsDevice device, CpuAccessFlags cpuAccessFlags, bool allowSubresourceUpdate, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0) : this(device, cpuAccessFlags, 1, filename, lineNumber)
        {
            if (allowSubresourceUpdate)
            {
                description.Usage = Usage.Default;
            }

            buffer = device.CreateBuffer<T>(default, description);
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

        /// <summary>
        /// Gets or sets the <see cref="T"/> at the specified index.
        /// </summary>
        /// <_value>
        /// The <see cref="T"/>.
        /// </_value>
        /// <param dbgName="index">The index.</param>
        /// <returns></returns>
        public T this[int index]
        {
            get { return items[index]; }
            set
            {
                items[index] = value;
            }
        }

        /// <summary>
        /// Gets the buffer.
        /// </summary>
        /// <_value>
        /// The buffer.
        /// </_value>
        public IBuffer Buffer => buffer;

        /// <summary>
        /// Gets the local buffer.
        /// </summary>
        /// <_value>
        /// The local buffer.
        /// </_value>
        public T* Local => items;

        public ref T Data => ref items[0];

        public BufferDescription Description => buffer.Description;

        public int Length => buffer.Length;

        public ResourceDimension Dimension => buffer.Dimension;

        public nint NativePointer => buffer.NativePointer;

        public string? DebugName { get => buffer.DebugName; set => buffer.DebugName = value; }

        public bool IsDisposed => buffer.IsDisposed;

        /// <summary>
        /// Resizes the buffer.
        /// </summary>
        /// <param dbgName="device">The device.</param>
        /// <param dbgName="length">The length.</param>
        public void Resize(uint length)
        {
            var result = items;
            ResizeArray(&result, count, length);
            items = result;
            count = length;
            MemoryManager.Unregister(buffer);
            buffer.Dispose();
            buffer = device.CreateBuffer(items, 1, description);
            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);
        }

        public void Update(IGraphicsContext context)
        {
            if (description.Usage != Usage.Dynamic)
            {
                throw new InvalidOperationException();
            }

            context.Write(buffer, items, buffer.Description.ByteWidth);
        }

        public void Update(IGraphicsContext context, T value)
        {
            *items = value;
            if (description.Usage != Usage.Dynamic)
            {
                throw new InvalidOperationException();
            }

            context.Write(buffer, items, buffer.Description.ByteWidth);
        }

        public void Dispose()
        {
            MemoryManager.Unregister(buffer);
            count = 0;
            if (items != null)
            {
                Free(items);
            }

            items = null;
            buffer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}