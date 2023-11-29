namespace HexaEngine.Graphics.Passes
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics.Graph;

    public class PostProcessPrePass : DrawPass
    {
        public PostProcessPrePass() : base("PostProcessPrePass")
        {
            AddReadDependency(new("#DepthStencil"));
        }

        public override void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            //postProcessing.PrePassDraw(context);
        }
    }
}