namespace HexaEngine.Graphics.Passes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Profiling;

    public class RadiosityBakePass : RenderPass
    {
        public RadiosityBakePass() : base("RadiosityBakePass", RenderPassType.Trigger)
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