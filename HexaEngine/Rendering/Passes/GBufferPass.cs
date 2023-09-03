namespace HexaEngine.Rendering.Passes
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graph;
    using HexaEngine.Rendering;
    using HexaEngine.Rendering.Graph;
    using HexaEngine.Scenes;
    using System.Numerics;

    public class GBufferPass : RenderPass
    {
        private ResourceRef<GBuffer> gbuffer;
        private ResourceRef<Texture2D> lightBuffer;
        private ResourceRef<DepthStencil> depthStencil;

        public GBufferPass() : base("GBuffer")
        {
            AddWriteDependency(new("#DepthStencil"));
            AddWriteDependency(new("GBuffer"));
        }

        private readonly bool forceForward = true;

        public override void Init(GraphResourceBuilder creator, GraphPipelineBuilder pipelineCreator, IGraphicsDevice device, ICPUProfiler? profiler)
        {
            var viewport = creator.Viewport;
            gbuffer = creator.CreateGBuffer("GBuffer", new((int)viewport.Width, (int)viewport.Height, 4,
                Format.R16G16B16A16Float,   // BaseColor(RGB)   Material ID(A)
                Format.R8G8B8A8UNorm,       // normal(XYZ)      Roughness(W)
                Format.R8G8B8A8UNorm,       // Metallic         Reflectance             AO      Material Data
                Format.R8G8B8A8UNorm        // Emission(XYZ)    Emission Strength(W)
                ));
            lightBuffer = creator.GetTexture2D("LightBuffer");
            depthStencil = creator.GetDepthStencilBuffer("#DepthStencil");
        }

        public override unsafe void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            var gbuffer = this.gbuffer.Value;
            var lightBuffer = this.lightBuffer.Value;
            context.ClearRenderTargetViews(gbuffer.Count, gbuffer.PRTVs, Vector4.Zero);
            context.ClearRenderTargetView(lightBuffer.RTV, Vector4.Zero);

            if (forceForward)
                return;

            var current = SceneManager.Current;
            if (current == null)
            {
                return;
            }

            var renderers = current.RenderManager;

            var depthStencil = this.depthStencil.Value;

            context.SetRenderTargets(gbuffer.Count, gbuffer.PRTVs, depthStencil.DSV);

            context.SetViewport(gbuffer.Viewport);
            renderers.Draw(context, RenderQueueIndex.Geometry, RenderPath.Deferred);
            context.ClearState();
        }
    }
}