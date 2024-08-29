namespace HexaEngine.Graphics.Passes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Profiling;

    public class BRDFLUTPass : RenderPass
    {
        private ResourceRef<Texture2D> lut;
        private ResourceRef<IGraphicsPipelineState> lutPass;

        public BRDFLUTPass() : base("BRDFLUT", RenderPassType.OneHit)
        {
            AddWriteDependency(new("BRDFLUT"));
        }

        public override void Init(GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            lut = creator.CreateTexture2D("BRDFLUT", new(Format.R16G16B16A16Float, 128, 128, 1, 1, GpuAccessFlags.RW));
            lutPass = creator.CreateGraphicsPipelineState(new()
            {
                Pipeline = new()
                {
                    PixelShader = "effects/dfg/ps.hlsl",
                    VertexShader = "quad.hlsl",
                },
                State = GraphicsPipelineStateDesc.DefaultFullscreen,
            });
        }

        public override void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            context.SetRenderTarget(lut.Value?.RTV, null);
            context.SetGraphicsPipelineState(lutPass.Value);
            context.SetViewport(new(128, 128));
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipelineState(null);
            context.SetRenderTarget(null, null);
        }
    }
}