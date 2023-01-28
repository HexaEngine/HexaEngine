namespace HexaEngine.Core.Scenes.Managers.Forward
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;

    public class PSMPipeline : IGraphicsPipeline
    {
        private IGraphicsPipeline pipeline;
        public IBuffer? View;

        public GraphicsPipelineDesc Description => pipeline.Description;

        public string DebugName => pipeline.DebugName;

        public GraphicsPipelineState State { get => pipeline.State; set => pipeline.State = value; }

        public PSMPipeline(IGraphicsDevice device)
        {
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "forward/psm/vs.hlsl",
                HullShader = "forward/psm/hs.hlsl",
                DomainShader = "forward/psm/ds.hlsl",
                PixelShader = "forward/psm/ps.hlsl",
            },
            new GraphicsPipelineState()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.PatchListWith3ControlPoints,
            });
        }

        public void BeginDraw(IGraphicsContext context, Viewport viewport)
        {
            pipeline.BeginDraw(context, viewport);
            context.DSSetConstantBuffer(View, 1);
        }

        public void EndDraw(IGraphicsContext context)
        {
            pipeline.EndDraw(context);
        }

        public void DrawIndexedInstanced(IGraphicsContext context, Viewport viewport, uint indexCount, uint instanceCount, uint indexOffset, int vertexOffset, uint instanceOffset)
        {
            pipeline.DrawIndexedInstanced(context, viewport, indexCount, instanceCount, indexOffset, vertexOffset, instanceOffset);
        }

        public void DrawIndexedInstancedIndirect(IGraphicsContext context, Viewport viewport, IBuffer args, uint stride)
        {
            pipeline.DrawIndexedInstancedIndirect(context, viewport, args, stride);
        }

        public void DrawInstanced(IGraphicsContext context, Viewport viewport, IBuffer args, uint stride)
        {
            pipeline.DrawInstanced(context, viewport, args, stride);
        }

        public void DrawInstanced(IGraphicsContext context, Viewport viewport, uint vertexCount, uint instanceCount, uint vertexOffset, uint instanceOffset)
        {
            pipeline.DrawInstanced(context, viewport, vertexCount, instanceCount, vertexOffset, instanceOffset);
        }

        public void Recompile()
        {
            pipeline.Recompile();
        }

        public void Dispose()
        {
            pipeline.Dispose();
        }
    }
}