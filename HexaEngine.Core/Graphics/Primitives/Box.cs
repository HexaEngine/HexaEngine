namespace HexaEngine.Core.Graphics.Primitives
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using System.Numerics;

    public class Cube : Primitive<Vector3, ushort>
    {
        public Cube(IGraphicsDevice device) : base(device)
        {
        }

        protected override (VertexBuffer<Vector3>, IndexBuffer<ushort>?) InitializeMesh(IGraphicsDevice device)
        {
            VertexBuffer<Vector3> vertexBuffer = new(device, CpuAccessFlags.None, new Vector3[]
            {
                new Vector3(-1.0f, 1.0f, -1.0f),
                new Vector3(-1.0f, -1.0f, -1.0f),
                new Vector3(1.0f, -1.0f, -1.0f),
                new Vector3(1.0f, 1.0f, -1.0f),
                new Vector3(-1.0f, -1.0f, 1.0f),
                new Vector3(-1.0f, 1.0f, 1.0f),
                new Vector3(1.0f, -1.0f, 1.0f),
                new Vector3(1.0f, 1.0f, 1.0f)
            });

            IndexBuffer<ushort> indexBuffer = new(device, new ushort[]
            {
                0,1,2,2,3,0,
                4,1,0,0,5,4,
                2,6,7,7,3,2,
                4,5,7,7,6,4,
                0,3,7,7,5,0,
                1,4,2,2,4,6,
            }, CpuAccessFlags.None);

            return (vertexBuffer, indexBuffer);
        }
    }
}