namespace HexaEngine.Core.Graphics.Buffers
{
    using System;
    using System.Runtime.InteropServices;

    public unsafe class ConstantBuffer<T> : IDisposable where T : unmanaged
    {
        private readonly CpuAccessFlags accessFlags;
        private IBuffer buffer;
        private T* values;
        private int length;
        private int size;
        private readonly int stride;

        public ConstantBuffer(IGraphicsDevice device, int length, CpuAccessFlags accessFlags)
        {
            this.length = length;
            this.accessFlags = accessFlags;
            stride = sizeof(T);
            size = stride * length;
            values = (T*)Marshal.AllocHGlobal(size);
            for (int i = 0; i < length; i++)
            {
                values[i] = default;
            }

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

        public ConstantBuffer(IGraphicsDevice device, T[] values, CpuAccessFlags accessFlags)
        {
            length = values.Length;
            this.accessFlags = accessFlags;
            stride = sizeof(T);
            size = stride * length;

            this.values = (T*)Marshal.AllocHGlobal(size);
            for (int i = 0; i < length; i++)
            {
                this.values[i] = values[i];
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

            values = (T*)Marshal.AllocHGlobal(size);
            for (int i = 0; i < length; i++)
            {
                values[i] = default;
            }

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

        public T this[int index]
        {
            get { return values[index]; }
            set { values[index] = value; }
        }

        public IBuffer Buffer => buffer;

        public void Resize(IGraphicsDevice device, int length)
        {
            buffer.Dispose();
            var oldLen = this.length;
            this.length = length;
            size = stride * length;

            var old = values;

            values = (T*)Marshal.AllocHGlobal(size);
            for (int i = 0; i < length; i++)
            {
                if (i < oldLen)
                    values[i] = old[i];
                else
                    values[i] = default;
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
            context.Write(buffer, values, size);
        }

        public void Dispose()
        {
            length = 0;
            size = 0;
            Marshal.FreeHGlobal((nint)values);
            values = null;
            buffer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}