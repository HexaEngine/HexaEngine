namespace HexaEngine.Graphics.Passes
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics.Graph;

    public class BRDFLUTPass : RenderPass
    {
        public BRDFLUTPass() : base("BRDFLUT", RenderPassType.OneHit)
        {
            AddWriteDependency(new("BRDFLUT"));
        }

        public override void Init(GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            creator.CreateTexture2D("BRDFLUT", new(Format.R16G16B16A16Float, 128, 128, 1, 1, BindFlags.ShaderResource | BindFlags.RenderTarget));
        }

        public override void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
        }
    }
}