namespace HexaEngine.Core.Graphics.Primitives
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Mathematics;

    public class Quad : Primitive<OrthoVertex>
    {
#nullable disable
        private static int instances;
        private static VertexBuffer<OrthoVertex> VertexBuffer;
        private static IndexBuffer IndexBuffer;
#nullable enable

        public Quad(IGraphicsDevice device) : base(device)
        {
        }

        protected override (VertexBuffer<OrthoVertex>, IndexBuffer?) InitializeMesh(IGraphicsDevice device)
        {
            if (instances == 0)
            {
                VertexBuffer = new(device, CpuAccessFlags.None, new OrthoVertex[]
                {
                    new OrthoVertex(new(-1, 1, 0), new(0, 0)),
                    new OrthoVertex(new(-1, -1, 0), new(0, 1)),
                    new OrthoVertex(new(1, 1, 0), new(1, 0)),
                    new OrthoVertex(new(1, -1, 0), new(1, 1))
                });
                IndexBuffer = new(device, CpuAccessFlags.None, new uint[] { 0, 3, 1, 0, 2, 3 });
            }

            instances++;
            return (VertexBuffer, IndexBuffer);
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