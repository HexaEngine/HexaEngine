namespace HexaEngine.Pipelines.Forward
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;

    public class CSMPipeline : IGraphicsPipeline
    {
        private IGraphicsPipeline pipeline;
        public IBuffer? View;

        public GraphicsPipelineDesc Description => pipeline.Description;

        public string DebugName => pipeline.DebugName;

        public GraphicsPipelineState State { get => pipeline.State; set => pipeline.State = value; }

        public CSMPipeline(IGraphicsDevice device)
        {
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "forward/csm/vs.hlsl",
                HullShader = "forward/csm/hs.hlsl",
                DomainShader = "forward/csm/ds.hlsl",
                GeometryShader = "forward/csm/gs.hlsl",
                PixelShader = "forward/csm/ps.hlsl",
            }, new GraphicsPipelineState()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullNone,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.PatchListWith3ControlPoints,
            },
            new ShaderMacro[]
            {
                new("INSTANCED", 1)
            });
        }

        public void BeginDraw(IGraphicsContext context, Viewport viewport)
        {
            pipeline.BeginDraw(context, viewport);
            context.GSSetConstantBuffer(View, 0);
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