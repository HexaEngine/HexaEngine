namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Resources;
    using HexaEngine.Pipelines.Deferred;
    using System.Collections.Concurrent;

    public class PrepassManager
    {
        private readonly IGraphicsDevice device;
        private readonly DepthBuffer depthBuffer;
        private readonly DepthMipChain depthMipChain;

        public PrepassManager(IGraphicsDevice device, int width, int height)
        {
            this.device = device;
            depthBuffer = new(device, width, height, Format.R32Typeless);
            depthMipChain = new(device, width, height);
        }

        public DepthBuffer DepthBuffer => depthBuffer;

        public DepthMipChain DepthMipChain => depthMipChain;

        public unsafe void DoPrepass(IGraphicsContext context, List<IDrawable> drawables)
        {
        }
    }
}