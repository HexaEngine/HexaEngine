namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Mathematics;

    public interface IGraphicsPipeline : IDisposable
    {
        GraphicsPipelineDesc Description { get; }

        string DebugName { get; }

        GraphicsPipelineState State { get; set; }

        void BeginDraw(IGraphicsContext context, Viewport viewport);

        void EndDraw(IGraphicsContext context);

        void DrawIndexedInstanced(IGraphicsContext context, Viewport viewport, uint indexCount, uint instanceCount, uint indexOffset, int vertexOffset, uint instanceOffset);

        void DrawIndexedInstancedIndirect(IGraphicsContext context, Viewport viewport, IBuffer args, uint stride);

        void DrawInstanced(IGraphicsContext context, Viewport viewport, IBuffer args, uint stride);

        void DrawInstanced(IGraphicsContext context, Viewport viewport, uint vertexCount, uint instanceCount, uint vertexOffset, uint instanceOffset);

        void Recompile();
    }
}