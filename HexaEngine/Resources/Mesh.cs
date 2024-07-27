namespace HexaEngine.Resources
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.IO.Binary.Meshes;

    public class Mesh : ResourceInstance
    {
        private readonly IMeshData data;
        private readonly ILODData lodData;
        private readonly IVertexBuffer vertexBuffer;
        private readonly IIndexBuffer indexBuffer;
        private readonly uint vertexCount;
        private readonly uint indexCount;
        private readonly BoundingBox boundingBox;
        private readonly BoundingSphere boundingSphere;
        private readonly uint stride;
        private readonly Format indexFormat;

        public Mesh(IResourceFactory factory, ResourceGuid id, MeshDesc desc) : base(factory, id)
        {
            data = desc.MeshData;
            lodData = desc.LODData;
            boundingBox = lodData.Box;
            boundingSphere = lodData.Sphere;
            indexBuffer = lodData.CreateIndexBuffer(desc.IndexDynamic ? CpuAccessFlags.Write : CpuAccessFlags.None);
            vertexBuffer = lodData.CreateVertexBuffer(desc.VertexDynamic ? CpuAccessFlags.Write : CpuAccessFlags.None);
            stride = vertexBuffer.Stride;
            indexCount = lodData.IndexCount;
            vertexCount = lodData.VertexCount;
            indexFormat = indexBuffer.Format switch
            {
                IndexFormat.UInt16 => Format.R16UInt,
                IndexFormat.UInt32 => Format.R32UInt,
                _ => Format.Unknown
            };
        }

        public IMeshData Data => data;

        public ILODData LODData => lodData;

        public IVertexBuffer VertexBuffer => vertexBuffer;

        public IIndexBuffer IndexBuffer => indexBuffer;

        public uint VertexCount => vertexCount;

        public uint IndexCount => indexCount;

        public BoundingBox BoundingBox => boundingBox;

        public BoundingSphere BoundingSphere => boundingSphere;

        public uint VertexStride => vertexBuffer?.Stride ?? unchecked((uint)-1);

        public uint IndexStride => (indexBuffer?.Format ?? IndexFormat.Unknown) switch
        {
            IndexFormat.UInt16 => sizeof(ushort),
            IndexFormat.UInt32 => sizeof(uint),
            _ => unchecked((uint)-1)
        };

        public InputElementDescription[] InputElements => data.InputElements;

        public void BeginDraw(IGraphicsContext context)
        {
            context.SetIndexBuffer(indexBuffer, indexFormat, 0);
            context.SetVertexBuffer(vertexBuffer, stride);
        }

        public void BeginDraw(IGraphicsContext context, int indexBufferOffset)
        {
            context.SetIndexBuffer(indexBuffer, indexFormat, indexBufferOffset);
            context.SetVertexBuffer(vertexBuffer, stride);
        }

        public void EndDraw(IGraphicsContext context)
        {
            context.SetIndexBuffer(null, Format.Unknown, 0);
            context.SetVertexBuffer(null, 0);
        }

        protected override void ReleaseResources()
        {
            vertexBuffer?.Dispose();
            indexBuffer?.Dispose();
        }

        public override string ToString()
        {
            return $"{Id}, {data.Name}";
        }
    }
}