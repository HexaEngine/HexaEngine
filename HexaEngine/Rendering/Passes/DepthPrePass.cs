#nullable disable

namespace HexaEngine.Rendering.Passes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Rendering;
    using HexaEngine.Rendering.Graph;
    using HexaEngine.Scenes;
    using System;

    public class DepthPrePass : RenderPass
    {
        public DepthPrePass() : base("PrePass")
        {
            AddWriteDependency(new("#DepthStencil"));
        }

        public override void Execute(IGraphicsContext context, GraphResourceBuilder creator)
        {
            var current = SceneManager.Current;
            if (current == null)
            {
                return;
            }

            var renderers = current.RenderManager;

            var depthStencil = creator.GetDepthStencilBuffer("#DepthStencil");
            context.ClearDepthStencilView(depthStencil.DSV, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);
            context.SetRenderTarget(null, depthStencil.DSV);
            context.SetViewport(depthStencil.Viewport);

            var backgroundQueue = renderers.BackgroundQueue;

            renderers.DrawDepth(context, RenderQueueIndex.Geometry | RenderQueueIndex.Transparency);
            context.ClearState();
        }
    }
}