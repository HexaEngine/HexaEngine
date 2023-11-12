namespace HexaEngine.Core.Graphics.Primitives
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Meshes;
    using System.Numerics;

    /// <summary>
    /// Represents a simple plane in 3D space.
    /// </summary>
    public sealed class Plane : IPrimitive
    {
        private readonly VertexBuffer<MeshVertex> vertexBuffer;
        private readonly IndexBuffer<ushort> indexBuffer;
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="Plane"/> class.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="size">The size of the plane.</param>
        public Plane(IGraphicsDevice device, float size)
        {
            CreatePlane(device, out vertexBuffer, out indexBuffer, size);
        }

        /// <summary>
        /// Creates a plane mesh.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="vertexBuffer">The vertex buffer.</param>
        /// <param name="indexBuffer">The index buffer.</param>
        /// <param name="size">The size of the plane.</param>
        public static void CreatePlane(IGraphicsDevice device, out VertexBuffer<MeshVertex> vertexBuffer, out IndexBuffer<ushort> indexBuffer, float size = 1)
        {
            vertexBuffer = new(device, new MeshVertex[]
            {
                new(new(-1 * size, 1 * size, 0), new Vector2(0,0), new(0,0,-1), new(1,0,0), new(0,1,0)),
                new(new(-1 * size, -1 * size, 0), new Vector2(0,1), new(0,0,-1), new(1,0,0), new(0,1,0)),
                new(new(1 * size, 1 * size, 0), new Vector2(1,0), new(0,0,-1), new(1,0,0), new(0,1,0)),
                new(new(1 * size, -1 * size, 0), new Vector2(1,1), new(0,0,-1), new(1,0,0), new(0,1,0))
            }, CpuAccessFlags.None);

            indexBuffer = new(device, new ushort[]
            {
                0, 3, 1,
                0, 2, 3
            }, CpuAccessFlags.None);
        }

        /// <inheritdoc/>
        public void DrawAuto(IGraphicsContext context, IGraphicsPipeline pipeline)
        {
            context.SetGraphicsPipeline(pipeline);
            context.SetVertexBuffer(vertexBuffer, vertexBuffer.Stride);
            if (indexBuffer != null)
            {
                indexBuffer.Bind(context);
                context.DrawIndexedInstanced(indexBuffer.Count, 1, 0, 0, 0);
                context.SetIndexBuffer(null, 0, 0);
            }
            else
            {
                context.DrawInstanced(vertexBuffer.Count, 1, 0, 0);
            }
            context.SetVertexBuffer(null, 0);
            context.SetGraphicsPipeline(null);
        }

        /// <inheritdoc/>
        public void DrawAuto(IGraphicsContext context)
        {
            context.SetVertexBuffer(vertexBuffer, vertexBuffer.Stride);
            if (indexBuffer != null)
            {
                indexBuffer.Bind(context);
                context.DrawIndexedInstanced(indexBuffer.Count, 1, 0, 0, 0);
                context.SetIndexBuffer(null, 0, 0);
            }
            else
            {
                context.DrawInstanced(vertexBuffer.Count, 1, 0, 0);
            }
            context.SetVertexBuffer(null, 0);
            context.SetGraphicsPipeline(null);
        }

        /// <inheritdoc/>
        public void Bind(IGraphicsContext context, out uint vertexCount, out uint indexCount, out int instanceCount)
        {
            context.SetVertexBuffer(vertexBuffer, vertexBuffer.Stride);
            vertexCount = vertexBuffer.Count;
            indexBuffer.Bind(context);
            indexCount = indexBuffer?.Count ?? 0;
            instanceCount = 1;
        }

        /// <inheritdoc/>
        public void Unbind(IGraphicsContext context)
        {
            context.SetVertexBuffer(null, 0);
            context.SetIndexBuffer(null, 0, 0);
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                vertexBuffer?.Dispose();
                indexBuffer?.Dispose();
                disposedValue = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Plane"/> class.
        /// </summary>
        ~Plane()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}