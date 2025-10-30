using Hexa.NET.Mathematics;
using HexaEngine.Core.Graphics;
using HexaEngine.Graphics.Overlays;
using HexaEngine.Graphics.Renderers;
using System.Numerics;

namespace HexaEngine.UI
{
    public class UISystemRenderer : OverlayRendererBase
    {
        private UIRenderer renderer = null!;

        public override int ZIndex { get; } = 1000;

        protected override void Draw(IGraphicsContext context, Viewport viewport, Texture2D target, DepthStencil depthStencil)
        {
            var ui = UISystem.Current;
            if (ui != null)
            {
                var commandList = ui.CommandList;
                float L = 0;
                float R = viewport.Width;
                float T = 0;
                float B = viewport.Height;
                Matrix4x4 mvp = new
                    (
                     2.0f / (R - L), 0.0f, 0.0f, 0.0f,
                     0.0f, 2.0f / (T - B), 0.0f, 0.0f,
                     0.0f, 0.0f, 0.5f, 0.0f,
                     (R + L) / (L - R), (T + B) / (B - T), 0.5f, 1.0f
                );

                context.SetRenderTarget(target, depthStencil);
                renderer?.RenderDrawData(context, viewport, mvp, commandList);
            }
        }

        protected override void Init()
        {
            renderer = new();
        }

        protected override void Release()
        {
            renderer.Release();
        }
    }
}
