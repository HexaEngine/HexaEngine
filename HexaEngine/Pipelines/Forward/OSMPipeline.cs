namespace HexaEngine.Pipelines.Forward
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public unsafe class OSMPipeline : IGraphicsPipeline
    {
        private readonly IGraphicsPipeline pipeline;
        public ConstantBuffer<Matrix4x4>? View;
        public IBuffer? Light;

        public OSMPipeline(IGraphicsDevice device)
        {
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "forward/osm/vs.hlsl",
                HullShader = "forward/osm/hs.hlsl",
                DomainShader = "forward/osm/ds.hlsl",
                GeometryShader = "forward/osm/gs.hlsl",
                PixelShader = "forward/osm/ps.hlsl",
            },
            new GraphicsPipelineState()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.PatchListWith3ControlPoints,
            },
            new ShaderMacro[]
            {
                new("INSTANCED", 1)
            });
        }

        public GraphicsPipelineDesc Description => pipeline.Description;

        public string DebugName => pipeline.DebugName;

        public GraphicsPipelineState State { get => pipeline.State; set => pipeline.State = value; }

        public void BeginDraw(IGraphicsContext context, Viewport viewport)
        {
            pipeline.BeginDraw(context, viewport);
            context.GSSetConstantBuffer(View?.Buffer, 0);
            context.PSSetConstantBuffer(Light, 0);
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