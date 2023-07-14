#nullable disable

namespace HexaEngine.Rendering.Passes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Culling;
    using HexaEngine.Rendering.Graph;

    public class ObjectCullPass : ComputePass
    {
        public ObjectCullPass() : base("ObjectCull")
        {
            AddReadDependency(new("HiZBuffer"));
        }

        public override void Execute(IGraphicsContext context, ResourceCreator creator)
        {
            CullingManager.DoCulling(context, creator.GetTexture2D("HiZBuffer").SRV);
        }
    }
}