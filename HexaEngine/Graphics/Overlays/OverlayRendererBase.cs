using Hexa.NET.Mathematics;
using HexaEngine.Core.Graphics;

namespace HexaEngine.Graphics.Overlays
{
    public abstract class OverlayRendererBase : IOverlay
    {
        private bool initialized;

        public abstract int ZIndex { get; }

        void IOverlay.Draw(IGraphicsContext context, Viewport viewport, Texture2D target, DepthStencil depthStencil)
        {
            if (!initialized) return;
            Draw(context, viewport, target, depthStencil);
        }

        void IOverlay.Init()
        {
            if (initialized) return;
            Init();
            initialized = true;
        }

        void IOverlay.Release()
        {
            if (!initialized) return;
            Release();
            initialized = false;
        }

        protected abstract void Init();

        protected abstract void Release();

        protected abstract void Draw(IGraphicsContext context, Viewport viewport, Texture2D target, DepthStencil depthStencil);
    }
}
