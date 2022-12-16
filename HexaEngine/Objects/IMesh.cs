namespace HexaEngine.Objects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using System;
    using System.Numerics;

    public interface IPrimitive : IDisposable
    {
        void Bind(IGraphicsContext context, out int vertexCount, out int indexCount, out int instanceCount);

        void DrawAuto(IGraphicsContext context, GraphicsPipeline pipeline, Viewport viewport);
    }
}