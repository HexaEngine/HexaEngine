namespace HexaEngine.Graphics.Passes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Profiling;
    using HexaEngine.Scenes;
    using System.Numerics;

    public class GBufferPass : RenderPass<GBufferPass>
    {
        private ResourceRef<GBuffer> gbuffer = null!;
        private ResourceRef<Texture2D> lightBuffer = null!;
        private ResourceRef<DepthStencil> depthStencil = null!;

        public GBufferPass(Windows.RendererFlags flags)
        {
            forceForward = (flags & Windows.RendererFlags.ForceForward) != 0;
        }

        private readonly bool forceForward = false;

        public override void BuildDependencies(GraphDependencyBuilder builder)
        {
            builder
                .RunAfter<DepthPrePass>()
                .RunAfter<ObjectCullPass>()
                .RunAfter<LightCullPass>();
        }

        public override void Init(GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            var viewport = creator.Viewport;
            gbuffer = creator.CreateGBuffer("GBuffer", new((int)viewport.Width, (int)viewport.Height, 4,
                Format.R16G16B16A16Float,   // BaseColor(RGB)   Material ID(A)
                Format.R16G16B16A16Float,   // Normal(XYZ)      Roughness(W)
                Format.R16G16B16A16Float,   // Metallic         Reflectance             AO      Material Data
                Format.R8G8B8A8UNorm        // Emission(XYZ)    Emission Strength(W)
                ));
            lightBuffer = creator.GetTexture2D("LightBuffer");
            depthStencil = creator.GetDepthStencilBuffer("#DepthStencil");
        }

        public override unsafe void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            var gbuffer = this.gbuffer.Value;
            var lightBuffer = this.lightBuffer.Value;
            context.ClearRenderTargetViews(gbuffer!.Count, gbuffer.PRTVs, Vector4.Zero);
            context.ClearRenderTargetView(lightBuffer!.RTV!, Vector4.Zero);

            if (forceForward)
            {
                return;
            }

            var current = SceneManager.Current;
            if (current == null)
            {
                return;
            }

            var renderers = current.RenderManager;

            var depthStencil = this.depthStencil.Value;

            context.SetRenderTargets(gbuffer.Count, gbuffer.PRTVs, depthStencil!.DSV);

            context.SetViewport(gbuffer.Viewport);

            profiler?.Begin("LightDeferred.Geometry");
            RenderSystem.ExecuteGroup(renderers.GeometryQueue, context, profiler, "LightDeferred", RenderPath.Deferred);
            profiler?.End("LightDeferred.Geometry");

            profiler?.Begin("LightDeferred.AlphaTest");
            RenderSystem.ExecuteGroup(renderers.AlphaTestQueue, context, profiler, "LightDeferred", RenderPath.Deferred);
            profiler?.End("LightDeferred.AlphaTest");

            context.ClearState();
        }
    }
}