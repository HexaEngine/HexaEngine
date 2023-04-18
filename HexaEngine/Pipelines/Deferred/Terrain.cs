#nullable disable

namespace HexaEngine.Pipelines.Deferred
{
    using HexaEngine.Core.Graphics;

    public class Terrain : IGraphicsPipeline
    {
        private IGraphicsPipeline pipeline;

        public Terrain(IGraphicsDevice device)
        {
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "deferred/terrain/vs.hlsl",
                HullShader = "deferred/terrain/hs.hlsl",
                DomainShader = "deferred/terrain/ds.hlsl",
                PixelShader = "deferred/terrain/ps.hlsl"
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