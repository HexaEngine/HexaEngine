namespace HexaEngine.Graphics.Passes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Profiling;

    public class RadiosityBakePass : RenderPass<RadiosityBakePass>
    {
        public RadiosityBakePass() : base(RenderPassType.Trigger)
        {
        }

        public override void Init(GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
        }

        public override void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
        }

        public override void Release()
        {
        }
    }
}