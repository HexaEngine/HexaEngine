#nullable disable

namespace HexaEngine.Pipelines.Deferred
{
    using HexaEngine.Core.Graphics;

    public class Grid : IGraphicsPipeline
    {
        private IGraphicsPipeline pipeline;

        public Grid(IGraphicsDevice device)
        {
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "deferred/grid/vs.hlsl",
                PixelShader = "deferred/grid/ps.hlsl"
            },
            new GraphicsPipelineState()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.LineList,
            },
            new ShaderMacro[]
            {
                new("INSTANCED", 1)
            });
        }

        public GraphicsPipelineDesc Description => pipeline.Description;

        public string DebugName => pipeline.DebugName;

        public GraphicsPipelineState State { get => pipeline.State; set => pipeline.State = value; }

        public bool IsInitialized => pipeline.IsInitialized;

        public bool IsValid => pipeline.IsValid;

        public void BeginDraw(IGraphicsContext context)
        {
            pipeline.BeginDraw(context);
        }

        public void Dispose()
        {
            pipeline.Dispose();
        }

        public void EndDraw(IGraphicsContext context)
        {
            pipeline.EndDraw(context);
        }

        public void Recompile()
        {
            pipeline.Recompile();
        }
    }
}