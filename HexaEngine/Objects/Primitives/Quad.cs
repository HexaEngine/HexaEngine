namespace HexaEngine.Objects.Primitives
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Objects;
    using System.Numerics;

    public class Quad : Primitive<OrthoVertex>
    {
        private static int instances;
        private static VertexBuffer<OrthoVertex> VertexBuffer;
        private static IndexBuffer IndexBuffer;

        public Quad(IGraphicsDevice device) : base(device)
        {
        }

        protected override (VertexBuffer<OrthoVertex>, IndexBuffer, InstanceBuffer) InitializeMesh(IGraphicsDevice device)
        {
            if (instances == 0)
            {
                VertexBuffer = new();
                IndexBuffer = new();

                VertexBuffer.Append(new OrthoVertex(new Vector3(-1, 1, 0), new Vector2(0, 0)));
                VertexBuffer.Append(new OrthoVertex(new Vector3(-1, -1, 0), new Vector2(0, 1)));
                VertexBuffer.Append(new OrthoVertex(new Vector3(1, 1, 0), new Vector2(1, 0)));
                VertexBuffer.Append(new OrthoVertex(new Vector3(1, -1, 0), new Vector2(1, 1)));

                IndexBuffer.Append(0);
                IndexBuffer.Append(3);
                IndexBuffer.Append(1);
                IndexBuffer.Append(0);
                IndexBuffer.Append(2);
                IndexBuffer.Append(3);
            }

            instances++;
            return (VertexBuffer, IndexBuffer, null);
        }

        protected override void Dispose(bool disposing)
        {
            instances--;
            if (instances == 0)
            {
                base.Dispose(disposing);
            }
        }
    }
}