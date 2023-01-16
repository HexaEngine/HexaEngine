using HexaEngine.Core.Graphics;

namespace HexaEngine.Graphics.Buffers
{
    using System;
    using System.Runtime.CompilerServices;

    public unsafe class ConstantBuffer<T> : IConstantBuffer, IConstantBuffer<T> where T : unmanaged
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
        /// <param dbgName="device">The device.</param>
        /// <param dbgName="length">The length.</param>
        /// <param dbgName="accessFlags">The access flags.</param>
        public ConstantBuffer(IGraphicsDevice device, uint length, CpuAccessFlags accessFlags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"CB: {filename}, Line:{lineNumber}";
            this.device = device;
            description = new(0, BindFlags.ConstantBuffer, Usage.Default, accessFlags, ResourceMiscFlag.None);
            count = length;
            items = Alloc<T>(length);
            ZeroRange(items, length);

            description.Usage = accessFlags switch
            {
                CpuAccessFlags.Write => Usage.Dynamic,
                CpuAccessFlags.Read => Usage.Staging,
                CpuAccessFlags.None => Usage.Immutable,
                CpuAccessFlags.RW => Usage.Staging,
                _ => throw new ArgumentException("Invalid CpuAccessFlags", nameof(accessFlags)),
            };
            buffer = device.CreateBuffer(items, length, description);
            buffer.DebugName = dbgName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantBuffer{T}"/> class.
        /// </summary>
        /// <param dbgName="device">The device.</param>
        /// <param dbgName="values">The values.</param>
        /// <param dbgName="accessFlags">The access flags.</param>
        public ConstantBuffer(IGraphicsDevice device, T[] values, CpuAccessFlags accessFlags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"CB: {filename}, Line:{lineNumber}";
            this.device = device;
            description = new(0, BindFlags.ConstantBuffer, Usage.Default, accessFlags, ResourceMiscFlag.None);
            count = (uint)values.Length;
            items = Alloc<T>(count);

            fixed (T* src = values)
            {
                int size = (int)(count * sizeof(T));
                System.Buffer.MemoryCopy(src, items, size, size);
            }

            description.Usage = accessFlags switch
            {
                CpuAccessFlags.Write => Usage.Dynamic,
                CpuAccessFlags.Read => Usage.Staging,
                CpuAccessFlags.None => Usage.Immutable,
                CpuAccessFlags.RW => Usage.Staging,
                _ => throw new ArgumentException("Invalid CpuAccessFlags", nameof(accessFlags)),
            };
            buffer = device.CreateBuffer(items, count, description);
            buffer.DebugName = dbgName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantBuffer{T}"/> class.
        /// </summary>
        /// <param dbgName="device">The device.</param>
        /// <param dbgName="accessFlags">The access flags.</param>
        public ConstantBuffer(IGraphicsDevice device, CpuAccessFlags accessFlags, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"CB: {filename}, Line:{lineNumber}";
            this.device = device;
            description = new(0, BindFlags.ConstantBuffer, Usage.Default, accessFlags, ResourceMiscFlag.None);
            count = 1;
            items = Alloc<T>(1);
            ZeroRange(items, 1);

            description.Usage = accessFlags switch
            {
                CpuAccessFlags.Write => Usage.Dynamic,
                CpuAccessFlags.Read => Usage.Staging,
                CpuAccessFlags.None => Usage.Immutable,
                CpuAccessFlags.RW => Usage.Staging,
                _ => throw new ArgumentException("Invalid CpuAccessFlags", nameof(accessFlags)),
            };
            buffer = device.CreateBuffer(items, 1, description);
            buffer.DebugName = dbgName;
        }

        /// <summary>
        /// Gets or sets the <see cref="T"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="T"/>.
        /// </value>
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
        /// <value>
        /// The buffer.
        /// </value>
        public IBuffer Buffer => buffer;

        /// <summary>
        /// Gets the local buffer.
        /// </summary>
        /// <value>
        /// The local buffer.
        /// </value>
        public T* Local => items;

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

            buffer.Dispose();
            buffer = device.CreateBuffer(items, 1, description);
            buffer.DebugName = dbgName;
        }

        public void Update(IGraphicsContext context)
        {
            if (description.Usage != Usage.Dynamic) throw new InvalidOperationException();
            context.Write(buffer, items, buffer.Description.ByteWidth);
        }

        public void Dispose()
        {
            count = 0;
            if (items != null)
                Free(items);
            items = null;
            buffer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}