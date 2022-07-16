namespace HexaEngine.Graphics
{
    using HexaEngine.Core.Graphics;
    using System;
    using System.Runtime.InteropServices;

    public unsafe class ConstantBuffer<T> : IConstantBuffer<T> where T : unmanaged
    {
        protected IBuffer buffer;
        private bool disposedValue;
        private readonly bool isDynamic;
        private readonly ShaderBinding[] bindings;

        public ConstantBuffer(IGraphicsDevice device, ref T value, bool isDynamic, ShaderStage stage, int index)
        {
            this.isDynamic = isDynamic;
            if (isDynamic)
            {
                buffer = device.CreateBuffer(value, new(sizeof(T), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write));
            }
            else
            {
                buffer = device.CreateBuffer(value, new(sizeof(T), BindFlags.ConstantBuffer, Usage.Default));
            }
            buffer.DebugName = nameof(ConstantBuffer<T>);
            bindings = new ShaderBinding[] { new(stage, index) };
        }

        public ConstantBuffer(IGraphicsDevice device, ref T value, bool isDynamic, params ShaderBinding[] bindings)
        {
            this.isDynamic = isDynamic;
            if (isDynamic)
            {
                buffer = device.CreateBuffer(value, new(sizeof(T), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write));
            }
            else
            {
                buffer = device.CreateBuffer(value, new(sizeof(T), BindFlags.ConstantBuffer, Usage.Default));
            }
            buffer.DebugName = nameof(ConstantBuffer<T>);
            this.bindings = bindings;
        }

        public ConstantBuffer(IGraphicsDevice device, T[] value, bool isDynamic, ShaderStage stage, int index)
        {
            this.isDynamic = isDynamic;
            if (isDynamic)
            {
                buffer = device.CreateBuffer(value, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            }
            else
            {
                buffer = device.CreateBuffer(value, BindFlags.ConstantBuffer, Usage.Default);
            }
            buffer.DebugName = nameof(ConstantBuffer<T>);
            bindings = new ShaderBinding[] { new(stage, index) };
        }

        public ConstantBuffer(IGraphicsDevice device, T[] value, bool isDynamic, params ShaderBinding[] bindings)
        {
            this.isDynamic = isDynamic;
            if (isDynamic)
            {
                buffer = device.CreateBuffer(value, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            }
            else
            {
                buffer = device.CreateBuffer(value, BindFlags.ConstantBuffer, Usage.Default);
            }
            buffer.DebugName = nameof(ConstantBuffer<T>);
            this.bindings = bindings;
        }

        public ConstantBuffer(IGraphicsDevice device, ShaderStage stage, int index)
        {
            isDynamic = true;
            var size = Marshal.SizeOf<T>();
            buffer = device.CreateBuffer(new(size, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write));
            buffer.DebugName = nameof(ConstantBuffer<T>);
            bindings = new ShaderBinding[] { new(stage, index) };
        }

        public ConstantBuffer(IGraphicsDevice device, int count, ShaderStage stage, int index)
        {
            isDynamic = true;
            var size = Marshal.SizeOf<T>();
            buffer = device.CreateBuffer(new(size * count, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write));
            buffer.DebugName = nameof(ConstantBuffer<T>);
            bindings = new ShaderBinding[] { new(stage, index) };
        }

        public ConstantBuffer(IGraphicsDevice device, params ShaderBinding[] bindings)
        {
            this.bindings = bindings;
            isDynamic = true;
            var size = Marshal.SizeOf<T>();
            buffer = device.CreateBuffer(new(size, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write));
            buffer.DebugName = nameof(ConstantBuffer<T>);
        }

        public ConstantBuffer(IGraphicsDevice device, int count, params ShaderBinding[] bindings)
        {
            this.bindings = bindings;
            isDynamic = true;
            var size = Marshal.SizeOf<T>();
            buffer = device.CreateBuffer(new(size * count, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write));
            buffer.DebugName = nameof(ConstantBuffer<T>);
        }

        public void Write(IGraphicsContext context, T value)
        {
            if (isDynamic)
            {
                context.Write(buffer, value);
            }
        }

        public void Write(IGraphicsContext context, T[] value)
        {
            if (isDynamic)
                context.Write(buffer, value);
        }

        public void Bind(IGraphicsContext context)
        {
            foreach (var binding in bindings)
            {
                context.SetConstantBuffer(buffer, binding.Stage, binding.Slot);
            }
        }

        ~ConstantBuffer()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                buffer.Dispose();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}