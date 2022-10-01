#nullable disable

namespace HexaEngine.Graphics
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using System;
    using System.Collections.Generic;

    public unsafe class VertexBuffer : IDisposable
    {
        private bool isDirty;
        private List<Vertex> vertices;
        private int vertexCount;
        private IBuffer vertexBuffer;
        private bool disposedValue;

        public VertexBuffer()
        {
            vertices = new List<Vertex>();
            isDirty = true;
        }

        public VertexBuffer(IGraphicsDevice device, IEnumerable<Vertex> vertices)
        {
            this.vertices = new List<Vertex>(vertices);
            ResizeBuffers(device);
            UpdateBuffers(device.Context);
        }

        public VertexBuffer(IGraphicsDevice device, int capacity)
        {
            vertices = new List<Vertex>(capacity);
            ResizeBuffers(device);
        }

        public int Count => vertexCount;

        public int Capacity { get; private set; }

        private unsafe void ResizeBuffers(IGraphicsDevice device)
        {
            if (vertices.Count <= Capacity)
            {
                return;
            }
            vertexBuffer?.Dispose();
            vertexBuffer = device.CreateBuffer(vertices.ToArray(), BindFlags.VertexBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            Capacity = vertices.Count;
        }

        private unsafe void UpdateBuffers(IGraphicsContext context)
        {
            context.Write(vertexBuffer, vertices.ToArray());
            vertexCount = vertices.Count;
        }

        public void Append(params Vertex[] vertices)
        {
            this.vertices.AddRange(vertices);
            isDirty = true;
        }

        public void Clear()
        {
            vertices.Clear();
            isDirty = true;
        }

        public void Bind(IGraphicsContext context)
        {
            Bind(context, 0);
        }

        public void FlushMemory(IGraphicsContext context)
        {
            ResizeBuffers(context.Device);
            UpdateBuffers(context);
            vertices.Clear();
            vertices = null;
            isDirty = false;
        }

        public void Bind(IGraphicsContext context, int slot)
        {
            Bind(context, slot, 0);
        }

        public void Bind(IGraphicsContext context, int slot, int offset)
        {
            if (isDirty)
            {
                ResizeBuffers(context.Device);
                UpdateBuffers(context);

                isDirty = false;
            }

            context.SetVertexBuffer(slot, vertexBuffer, sizeof(Vertex), offset);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                vertexBuffer?.Dispose();
                vertexBuffer = null;

                disposedValue = true;
            }
        }

        ~VertexBuffer()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public unsafe class VertexBuffer<T> : IDisposable where T : unmanaged
    {
        private bool isDirty;
        private List<T> vertices;
        private int vertexCount;
        private IBuffer vertexBuffer;

        private bool disposedValue;

        public VertexBuffer()
        {
            vertices = new List<T>();
            isDirty = true;
        }

        public VertexBuffer(IGraphicsDevice device, IEnumerable<T> vertices)
        {
            this.vertices = new List<T>(vertices);
            ResizeBuffers(device);
            UpdateBuffers(device.Context);
        }

        public VertexBuffer(IGraphicsDevice device, int capacity)
        {
            vertices = new List<T>(capacity);
            ResizeBuffers(device);
        }

        public int Count => vertexCount;

        public int Capacity { get; private set; }

        public void FlushMemory(IGraphicsDevice device)
        {
            ResizeBuffers(device);
            UpdateBuffers(device.Context);
            vertices.Clear();
            vertices = null;

            isDirty = false;
        }

        private unsafe void ResizeBuffers(IGraphicsDevice device)
        {
            if (vertices.Count <= Capacity)
            {
                return;
            }
            vertexBuffer?.Dispose();
            vertexBuffer = device.CreateBuffer(vertices.ToArray(), BindFlags.VertexBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            Capacity = vertices.Count;
        }

        private unsafe void UpdateBuffers(IGraphicsContext context)
        {
            context.Write(vertexBuffer, vertices.ToArray());
            vertexCount = vertices.Count;
        }

        public void Append(params T[] vertices)
        {
            this.vertices.AddRange(vertices);
            isDirty = true;
        }

        public void Clear()
        {
            vertices.Clear();
            isDirty = true;
        }

        public void Bind(IGraphicsContext context)
        {
            Bind(context, 0);
        }

        public void Bind(IGraphicsContext context, int slot)
        {
            Bind(context, slot, 0);
        }

        public void Bind(IGraphicsContext context, int slot, int offset)
        {
            if (isDirty)
            {
                ResizeBuffers(context.Device);
                UpdateBuffers(context);

                isDirty = false;
            }

            context.SetVertexBuffer(slot, vertexBuffer, sizeof(T), offset);
        }

        public T[] GetVertices() => vertices.ToArray();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                vertexBuffer?.Dispose();
                vertexBuffer = null;

                disposedValue = true;
            }
        }

        ~VertexBuffer()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}