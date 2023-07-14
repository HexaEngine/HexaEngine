#nullable disable

namespace HexaEngine.Rendering.Passes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Rendering;
    using HexaEngine.Rendering.Graph;
    using HexaEngine.Scenes;
    using System.Numerics;

    public class GBufferPass : RenderPass
    {
        public GBufferPass() : base("GBuffer")
        {
            AddWriteDependency(new("#DepthStencil"));
            AddWriteDependency(new("GBuffer"));
        }

        private readonly bool forceForward = true;

        public override void Init(ResourceCreator creator, PipelineCreator pipelineCreator, IGraphicsDevice device)
        {
            var viewport = creator.GetViewport();
            creator.CreateGBuffer("GBuffer", new((int)viewport.Width, (int)viewport.Height, 4,
                Format.R16G16B16A16Float,   // BaseColor(RGB)   Material ID(A)
                Format.R8G8B8A8UNorm,       // Normal(XYZ)      Roughness(W)
                Format.R8G8B8A8UNorm,       // Metallic         Reflectance             AO      Material Data
                Format.R8G8B8A8UNorm        // Emission(XYZ)    Emission Strength(W)
                ));
        }

        public override unsafe void Execute(IGraphicsContext context, ResourceCreator creator)
        {
            var gbuffer = creator.GetGBuffer("GBuffer");

            context.ClearRenderTargetViews(gbuffer.Count, gbuffer.PRTVs, Vector4.Zero);

            if (forceForward)
                return;

            var current = SceneManager.Current;
            if (current == null)
            {
                return;
            }

            var renderers = current.RenderManager;

            var depthStencil = creator.GetDepthStencilBuffer("#DepthStencil");

            context.SetRenderTargets(gbuffer.Count, gbuffer.PRTVs, depthStencil.DSV);

            context.SetViewport(gbuffer.Viewport);
            renderers.Draw(context, RenderQueueIndex.Geometry, RenderPath.Deferred);
            context.ClearState();
        }
    }
}