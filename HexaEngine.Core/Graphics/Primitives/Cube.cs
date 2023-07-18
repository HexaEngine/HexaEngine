namespace HexaEngine.Core.Graphics.Primitives
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Meshes;
    using System;
    using System.Numerics;

    public class Cube : Primitive<Vector3, ushort>
    {
        public Cube(IGraphicsDevice device) : base(device)
        {
        }

        protected override (VertexBuffer<Vector3>, IndexBuffer<ushort>?) InitializeMesh(IGraphicsDevice device)
        {
            VertexBuffer<Vector3> vertexBuffer = new(device, new Vector3[]
            {
                new Vector3(-1.0f, 1.0f, -1.0f),
                new Vector3(-1.0f, -1.0f, -1.0f),
                new Vector3(1.0f, -1.0f, -1.0f),
                new Vector3(1.0f, 1.0f, -1.0f),
                new Vector3(-1.0f, -1.0f, 1.0f),
                new Vector3(-1.0f, 1.0f, 1.0f),
                new Vector3(1.0f, -1.0f, 1.0f),
                new Vector3(1.0f, 1.0f, 1.0f)
            }, CpuAccessFlags.None);

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

    public class Box : Primitive<Vector3, ushort>
    {
        public Box(IGraphicsDevice device) : base(device)
        {
        }

        protected override (VertexBuffer<Vector3>, IndexBuffer<ushort>?) InitializeMesh(IGraphicsDevice device)
        {
            //CreateBox(device, out VertexBuffer<MeshVertex> vertexBuffer, out IndexBuffer<uint> indexBuffer);
            //return (vertexBuffer, indexBuffer);
            VertexBuffer<Vector3> vertexBuffer = new(device, new Vector3[]
            {
                new Vector3(-1.0f, 1.0f, -1.0f),
                new Vector3(-1.0f, -1.0f, -1.0f),
                new Vector3(1.0f, -1.0f, -1.0f),
                new Vector3(1.0f, 1.0f, -1.0f),
                new Vector3(-1.0f, -1.0f, 1.0f),
                new Vector3(-1.0f, 1.0f, 1.0f),
                new Vector3(1.0f, -1.0f, 1.0f),
                new Vector3(1.0f, 1.0f, 1.0f)
            }, CpuAccessFlags.None);

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

        private static readonly Vector3[] verts =
        {
            new (-1.0f, 1.0f, -1.0f),
            new (-1.0f, -1.0f, -1.0f),
            new (1.0f, -1.0f, -1.0f),
            new (1.0f, 1.0f, -1.0f),
            new (-1.0f, -1.0f, 1.0f),
            new (-1.0f, 1.0f, 1.0f),
            new (1.0f, -1.0f, 1.0f),
            new (1.0f, 1.0f, 1.0f)
        };

        private static readonly uint[] faces =
        {
            0,1,2,2,3,0,
            4,1,0,0,5,4,
            2,6,7,7,3,2,
            4,5,7,7,6,4,
            0,3,7,7,5,0,
            1,4,2,2,4,6,
        };

        private void CreateBox(IGraphicsDevice device, out VertexBuffer<MeshVertex> vertexBuffer, out IndexBuffer<uint> indexBuffer, Vector3 size)
        {
            throw new NotImplementedException();
        }
    }
}