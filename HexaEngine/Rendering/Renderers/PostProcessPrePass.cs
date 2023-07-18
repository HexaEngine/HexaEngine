#nullable disable

namespace HexaEngine.Rendering.Renderers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Rendering.Graph;

    public class PostProcessPrePass : DrawPass
    {
        public PostProcessPrePass() : base("PostProcessPrePass")
        {
            AddReadDependency(new("#DepthStencil"));
        }

        public override void Execute(IGraphicsContext context, GraphResourceBuilder creator)
        {
            //postProcessing.PrePassDraw(context);
        }
    }
}