namespace HexaEngine.Core.Graphics.Buffers
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a constant buffer in graphics programming, which is a region of memory used to store constant data that can be accessed by shaders.
    /// </summary>
    /// <typeparam name="T">The type of elements in the constant buffer.</typeparam>
    public unsafe class ConstantBuffer<T> : IConstantBuffer, IConstantBuffer<T>, IBuffer where T : unmanaged
    {
        private readonly string dbgName;
        private readonly IGraphicsDevice device;
        private readonly BufferDescription description;
        private IBuffer buffer;
        private T* items;
        private uint count;

#nullable disable
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
#nullable restore

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantBuffer{T}"/> class with specified parameters.
        /// </summary>
        /// <param name="device">The graphics device used to create the constant buffer.</param>
        /// <param name="length">The length of the constant buffer (number of elements of type <typeparamref name="T"/>).</param>
        /// <param name="accessFlags">The CPU access flags determining how the buffer can be accessed by the CPU.</param>
        /// <param name="filename">The name of the file where the constructor is called.</param>
        /// <param name="lineNumber">The line number in the file where the constructor is called.</param>
        public ConstantBuffer(IGraphicsDevice device, uint length, CpuAccessFlags accessFlags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0) : this(device, accessFlags, length, filename, lineNumber)
        {
            buffer = device.CreateBuffer(items, length, description);
            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantBuffer{T}"/> class with the specified values.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="values">The array of values to initialize the buffer with.</param>
        /// <param name="accessFlags">The CPU access flags.</param>
        /// <param name="filename">The path of the source file invoking this constructor (automatically set by the compiler).</param>
        /// <param name="lineNumber">The line number in the source file where this constructor is invoked (automatically set by the compiler).</param>
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
        /// Initializes a new instance of the <see cref="ConstantBuffer{T}"/> class with a single value.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="value">The value to initialize the buffer with.</param>
        /// <param name="accessFlags">The CPU access flags.</param>
        /// <param name="filename">The path of the source file invoking this constructor (automatically set by the compiler).</param>
        /// <param name="lineNumber">The line number in the source file where this constructor is invoked (automatically set by the compiler).</param>
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
        /// Initializes a new instance of the <see cref="ConstantBuffer{T}"/> class with default values.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="accessFlags">The CPU access flags.</param>
        /// <param name="filename">The path of the source file invoking this constructor (automatically set by the compiler).</param>
        /// <param name="lineNumber">The line number in the source file where this constructor is invoked (automatically set by the compiler).</param>
        public ConstantBuffer(IGraphicsDevice device, CpuAccessFlags accessFlags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0) : this(device, accessFlags, 1, filename, lineNumber)
        {
            buffer = device.CreateBuffer(items, 1, description);
            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantBuffer{T}"/> class with the specified CPU access flags and subresource update option.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="cpuAccessFlags">The CPU access flags.</param>
        /// <param name="allowSubresourceUpdate">Specifies whether subresource updates are allowed.</param>
        /// <param name="filename">The path of the source file invoking this constructor (automatically set by the compiler).</param>
        /// <param name="lineNumber">The line number in the source file where this constructor is invoked (automatically set by the compiler).</param>
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

        /// <summary>
        /// Event that is triggered when the constant buffer is disposed.
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
        /// Gets or sets the element at the specified index in the constant buffer.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        public T this[int index]
        {
            get { return items[index]; }
            set
            {
                items[index] = value;
            }
        }

        /// <summary>
        /// Gets the underlying buffer associated with the constant buffer.
        /// </summary>
        public IBuffer Buffer => buffer;

        /// <summary>
        /// Gets a pointer to the first element in the constant buffer.
        /// </summary>
        public T* Local => items;

        /// <summary>
        /// Gets a reference to the first element in the constant buffer.
        /// </summary>
        public ref T Data => ref items[0];

        /// <summary>
        /// Gets the description of the constant buffer.
        /// </summary>
        public BufferDescription Description => buffer.Description;

        /// <summary>
        /// Gets the length of the constant buffer (number of elements of type <typeparamref name="T"/>).
        /// </summary>
        public int Length => buffer.Length;

        /// <summary>
        /// Gets the resource dimension of the constant buffer.
        /// </summary>
        public ResourceDimension Dimension => buffer.Dimension;

        /// <summary>
        /// Gets the native pointer to the underlying data of the constant buffer.
        /// </summary>
        public nint NativePointer => buffer.NativePointer;

        /// <summary>
        /// Gets or sets the debug name of the constant buffer.
        /// </summary>
        public string? DebugName { get => buffer.DebugName; set => buffer.DebugName = value; }

        /// <summary>
        /// Gets a value indicating whether the constant buffer has been disposed.
        /// </summary>
        public bool IsDisposed => buffer.IsDisposed;

        /// <summary>
        /// Resizes the constant buffer to the specified length.
        /// </summary>
        /// <param name="length">The new length of the constant buffer.</param>
        public void Resize(uint length)
        {
            var result = items;
            items = ReAllocT(items, length);
            items = result;
            count = length;
            MemoryManager.Unregister(buffer);
            buffer.Dispose();
            buffer = device.CreateBuffer(items, 1, description);
            buffer.DebugName = dbgName;
            MemoryManager.Register(buffer);
        }

        /// <summary>
        /// Updates the constant buffer with the current data using the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context used to update the constant buffer.</param>
        public void Update(IGraphicsContext context)
        {
            if (description.Usage != Usage.Dynamic)
            {
                throw new InvalidOperationException();
            }

            context.Write(buffer, items, buffer.Description.ByteWidth);
        }

        /// <summary>
        /// Updates a single element in the constant buffer with the specified value using the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context used to update the constant buffer.</param>
        /// <param name="value">The value to update the constant buffer with.</param>
        public void Update(IGraphicsContext context, T value)
        {
            *items = value;
            if (description.Usage != Usage.Dynamic)
            {
                throw new InvalidOperationException();
            }

            context.Write(buffer, items, buffer.Description.ByteWidth);
        }

        /// <summary>
        /// Disposes of the constant buffer and releases associated resources.
        /// </summary>
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