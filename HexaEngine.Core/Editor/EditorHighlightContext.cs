using HexaEngine.Core.Graphics;
using HexaEngine.Core.Graphics.Buffers;
using System.Numerics;

namespace HexaEngine.Core.Editor
{
    public abstract class EditorHighlightContext
    {
        public abstract IGraphicsContext GraphicsContext { get; }

        public abstract void Begin(InputElementDescription[] inputElements, PrimitiveTopology topology);

        public abstract void End();

        public abstract void SetTransform(Matrix4x4 transform, bool transpose = true);

        public virtual void DrawInstanced(uint vertexCount, uint instanceCount = 1, uint vertexOffset = 0, uint instanceOffset = 0)
        {
            GraphicsContext.DrawInstanced(vertexCount, instanceCount, vertexOffset, instanceOffset);
        }

        public virtual void DrawIndexedInstanced(uint indexCount, uint instanceCount = 1, uint indexOffset = 0, int vertexOffset = 0, uint instanceOffset = 0)
        {
            GraphicsContext.DrawIndexedInstanced(indexCount, instanceCount, indexOffset, vertexOffset, instanceOffset);
        }

        public void SetVertexBuffer(IBuffer vertexBuffer, uint stride)
        {
            GraphicsContext.SetVertexBuffer(vertexBuffer, stride);
        }

        public void SetVertexBuffer(IVertexBuffer vertexBuffer)
        {
            SetVertexBuffer(vertexBuffer, vertexBuffer.Stride);
        }

        public void SetIndexBuffer(IBuffer indexBuffer, Format format, int offset)
        {
            GraphicsContext.SetIndexBuffer(indexBuffer, format, offset);
        }

        public void SetIndexBuffer(IIndexBuffer indexBuffer, int offset)
        {
            Format format = indexBuffer.Format switch
            {
                IndexFormat.UInt16 => Format.R16UInt,
                IndexFormat.UInt32 => Format.R32UInt,
                _ => throw new NotSupportedException("Unsupported index format"),
            };
            SetIndexBuffer(indexBuffer, format, offset);
        }
    }
}