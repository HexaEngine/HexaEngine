using Hexa.NET.Mathematics;
using HexaEngine.Core.Graphics;

namespace HexaEngine.Graphics.Overlays
{
    public interface IOverlay
    {
        public int ZIndex { get; }

        public void Init();

        public void Draw(IGraphicsContext context, Viewport viewport, Texture2D target, DepthStencil depthStencil);

        public void Release();
    }
}
