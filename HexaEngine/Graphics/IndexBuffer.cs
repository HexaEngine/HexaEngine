namespace HexaEngine.Graphics
{
    using HexaEngine.Core.Graphics;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public unsafe class IndexBuffer
    {
        private bool isDirty;
        private readonly List<int> indices;
        private int indexCount;
        private IBuffer indexBuffer;
        private bool disposedValue;
        private readonly string name;

        public IndexBuffer([CallerFilePath] string name = "")
        {
            this.name = name;
            indices = new List<int>();
            isDirty = true;
        }

        public IndexBuffer(IGraphicsDevice device, IEnumerable<int> vertices, [CallerFilePath] string name = "")
        {
            this.name = name;
            indices = new List<int>(vertices);
            ResizeBuffers(device);
            UpdateBuffers(device.Context);
        }

        public IndexBuffer(IGraphicsDevice device, int capacity, [CallerFilePath] string name = "")
        {
            this.name = name;
            indices = new List<int>(capacity);
            ResizeBuffers(device);
        }

        public int Capacity { get; private set; }
        public int Count => indexCount;

        public void Append(params int[] indices)
        {
            this.indices.AddRange(indices);
            isDirty = true;
        }

        public void Bind(IGraphicsContext context)
        {
            Bind(context, 0);
        }

        public void Bind(IGraphicsContext context, int offset)
        {
            if (isDirty)
            {
                ResizeBuffers(context.Device);
                UpdateBuffers(context);

                isDirty = false;
            }

            context.SetIndexBuffer(indexBuffer, Format.R32UInt, offset);
        }

        private void UpdateBuffers(IGraphicsContext context)
        {
            context.Write(indexBuffer, indices.ToArray());
            indexCount = indices.Count;
        }

        private void ResizeBuffers(IGraphicsDevice device)
        {
            if (indices.Count <= Capacity)
            {
                return;
            }
            indexBuffer?.Dispose();
            indexBuffer = device.CreateBuffer(indices.ToArray(), BindFlags.IndexBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            indexBuffer.DebugName = "IB: " + name;
            Capacity = indices.Count;
        }

        public void Clear()
        {
            indices.Clear();
            isDirty = true;
        }

        public void FlushMemory()
        {
            indices.Clear();
        }

        public void Dispose()
        {
            if (!disposedValue)
            {
                indices.Clear();
                indexBuffer?.Dispose();
                indexBuffer = null;

                disposedValue = true;
            }
        }
    }
}