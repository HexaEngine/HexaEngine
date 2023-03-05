namespace HexaEngine.Core.Graphics.Primitives
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using System;

    public interface IPrimitive : IDisposable
    {
        void Bind(IGraphicsContext context, out uint vertexCount, out uint indexCount, out int instanceCount);

        void DrawAuto(IGraphicsContext context, IGraphicsPipeline pipeline);
    }
}