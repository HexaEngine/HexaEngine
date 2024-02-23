namespace HexaEngine.Core.IO.Binary.Meshes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Mathematics;

    public interface ILODData
    {
        uint LODLevel { get; }

        BoundingBox Box { get; }

        BoundingSphere Sphere { get; }

        uint IndexCount { get; }

        uint VertexCount { get; }

        IIndexBuffer CreateIndexBuffer(IGraphicsDevice device, CpuAccessFlags accessFlags = CpuAccessFlags.None);

        IVertexBuffer CreateVertexBuffer(IGraphicsDevice device, CpuAccessFlags accessFlags = CpuAccessFlags.None);

        bool WriteIndexBuffer(IGraphicsContext context, IIndexBuffer ib);

        bool WriteVertexBuffer(IGraphicsContext context, IVertexBuffer vb);
    }
}