namespace HexaEngine.Graphics.Passes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.PostFx;
    using HexaEngine.Profiling;
    using HexaEngine.Scenes;
    using System.Numerics;

    public class PostProcessPrePass : RenderPass<PostProcessPrePass>
    {
        private ResourceRef<Texture2D> AOBuffer = null!;

        public PostProcessPrePass()
        {
        }

        public override void BuildDependencies(GraphDependencyBuilder builder)
        {
            builder
                .RunAfter<GBufferPass>()
                .RunAfter<DepthPrePass>();
        }

        public override void Init(GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            AOBuffer = creator.GetTexture2D("#AOBuffer");
        }

        public override void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            context.ClearRenderTargetView(AOBuffer.Value!, Vector4.One);
            var postProcessingManager = PostProcessingManager.Current!;
            postProcessingManager.Enabled = (SceneRenderer.Current.DrawFlags & SceneDrawFlags.NoPostProcessing) == 0;
            postProcessingManager.Viewport = creator.Viewport;
            postProcessingManager.PrePassDraw(context, creator);
        }
    }
}