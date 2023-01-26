namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.Scenes.Managers;
    using HexaEngine.Windows;

    public class RenderManager
    {
        public readonly RenderQueue RenderQueue = new();
        public readonly PrepassManager PrepassManager;
        public readonly CullingManager CullingManager;
        public readonly LightManager LightManager;
        public readonly PostFxManager PostFxManager;
        private readonly TextureArray gbuffers;
        private readonly Window window;
        private readonly Scene scene;

        public RenderManager(Window window, Scene scene, IGraphicsDevice device)
        {
            LightManager = new(device, scene.InstanceManager);
            this.window = window;
            this.scene = scene;
        }

        public unsafe void Draw(IGraphicsContext context)
        {
            CullingManager.DoCulling(context, PrepassManager.DepthMipChain);

            context.SetRenderTargets(gbuffers.PRTVs, gbuffers.Count, null);
            var opaque = RenderQueue.GetQueue(RenderQueueIndex.Opaque);
            while (opaque.TryDequeue(out var item))
            {
                item.Drawable.Draw(context, RenderQueueIndex.Opaque);
            }

            // LightManager.DeferredPass(context);
        }
    }
}