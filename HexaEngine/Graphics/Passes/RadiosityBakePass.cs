namespace HexaEngine.Graphics.Passes
{
    using HexaEngine.Components.Renderer;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Scenes;
    using System.Threading.Tasks;

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