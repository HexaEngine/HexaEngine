namespace HexaEngine.Core.Graphics.Buffers
{
    using System;
    using System.Runtime.InteropServices;

    public unsafe class ConstantBuffer<T> : IDisposable where T : unmanaged
    {
        private readonly CpuAccessFlags accessFlags;
        private IBuffer buffer;
        private T* items;
        private int length;
        private int size;
        private readonly int stride;

        public ConstantBuffer(IGraphicsDevice device, int length, CpuAccessFlags accessFlags)
        {
            this.length = length;
            this.accessFlags = accessFlags;
            stride = sizeof(T);
            size = stride * length;
            items = (T*)Marshal.AllocHGlobal(size);
            for (int i = 0; i < length; i++)
            {
                items[i] = default;
            }

            if (accessFlags == CpuAccessFlags.None)
            {
                buffer = device.CreateBuffer(items, (uint)length, new(size, BindFlags.ConstantBuffer, Usage.Immutable, accessFlags));
            }
            else if (accessFlags == CpuAccessFlags.Write)
            {
                buffer = device.CreateBuffer(new(size, BindFlags.ConstantBuffer, Usage.Dynamic, accessFlags));
            }
            else if (accessFlags == CpuAccessFlags.Read)
            {
                buffer = device.CreateBuffer(new(size, BindFlags.ConstantBuffer, Usage.Staging, accessFlags));
            }
            else
            {
                buffer = device.CreateBuffer(new(size, BindFlags.ConstantBuffer, Usage.Staging, accessFlags));
            }
        }

        public ConstantBuffer(IGraphicsDevice device, T[] values, CpuAccessFlags accessFlags)
        {
            length = values.Length;
            this.accessFlags = accessFlags;
            stride = sizeof(T);
            size = stride * length;

            this.items = (T*)Marshal.AllocHGlobal(size);
            for (int i = 0; i < length; i++)
            {
                this.items[i] = values[i];
            }

            if (accessFlags == CpuAccessFlags.None)
            {
                buffer = device.CreateBuffer(values, new(size, BindFlags.ConstantBuffer, Usage.Immutable, accessFlags));
            }
            else if (accessFlags == CpuAccessFlags.Write)
            {
                buffer = device.CreateBuffer(values, new(size, BindFlags.ConstantBuffer, Usage.Dynamic, accessFlags));
            }
            else if (accessFlags == CpuAccessFlags.Read)
            {
                buffer = device.CreateBuffer(values, new(size, BindFlags.ConstantBuffer, Usage.Staging, accessFlags));
            }
            else
            {
                buffer = device.CreateBuffer(values, new(size, BindFlags.ConstantBuffer, Usage.Staging, accessFlags));
            }
        }

        public ConstantBuffer(IGraphicsDevice device, CpuAccessFlags accessFlags)
        {
            this.accessFlags = accessFlags;
            length = 1;
            size = stride = sizeof(T);

            items = (T*)Marshal.AllocHGlobal(size);
            for (int i = 0; i < length; i++)
            {
                items[i] = default;
            }

            if (accessFlags == CpuAccessFlags.None)
            {
                buffer = device.CreateBuffer(items, (uint)length, new(size, BindFlags.ConstantBuffer, Usage.Immutable, accessFlags));
            }
            else if (accessFlags == CpuAccessFlags.Write)
            {
                buffer = device.CreateBuffer(new(size, BindFlags.ConstantBuffer, Usage.Dynamic, accessFlags));
            }
            else if (accessFlags == CpuAccessFlags.Read)
            {
                buffer = device.CreateBuffer(new(size, BindFlags.ConstantBuffer, Usage.Staging, accessFlags));
            }
            else
            {
                buffer = device.CreateBuffer(new(size, BindFlags.ConstantBuffer, Usage.Staging, accessFlags));
            }
        }

        public T this[int index]
        {
            get { return items[index]; }
            set
            {
                items[index] = value;
            }
        }

        public IBuffer Buffer => buffer;

        public void Resize(IGraphicsDevice device, int length)
        {
            buffer.Dispose();
            var oldLen = this.length;
            this.length = length;
            size = stride * length;

            var old = items;

            items = (T*)Marshal.AllocHGlobal(size);
            for (int i = 0; i < length; i++)
            {
                if (i < oldLen)
                    items[i] = old[i];
                else
                    items[i] = default;
            }

            Marshal.FreeHGlobal((nint)old);

            if (accessFlags == CpuAccessFlags.None)
            {
                buffer = device.CreateBuffer(new(size, BindFlags.ConstantBuffer, Usage.Immutable, accessFlags));
            }
            else if (accessFlags == CpuAccessFlags.Write)
            {
                buffer = device.CreateBuffer(new(size, BindFlags.ConstantBuffer, Usage.Dynamic, accessFlags));
            }
            else if (accessFlags == CpuAccessFlags.Read)
            {
                buffer = device.CreateBuffer(new(size, BindFlags.ConstantBuffer, Usage.Staging, accessFlags));
            }
            else
            {
                buffer = device.CreateBuffer(new(size, BindFlags.ConstantBuffer, Usage.Staging, accessFlags));
            }
        }

        public void Update(IGraphicsContext context)
        {
            context.Write(buffer, items, size);
        }

        public void Dispose()
        {
            length = 0;
            size = 0;
            Marshal.FreeHGlobal((nint)items);
            items = null;
            buffer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}