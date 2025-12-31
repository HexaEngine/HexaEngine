using Hexa.NET.DebugDraw;
using Hexa.NET.Mathematics;
using HexaEngine.Core;
using HexaEngine.Core.Graphics;
using HexaEngine.Graphics.Overlays;
using HexaEngine.Graphics.Renderers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexaEngine.Editor.Overlays
{
    public class DebugDrawOverlay : IOverlay
    {
        public int ZIndex { get; }

        public void Draw(IGraphicsContext context, Viewport viewport, Texture2D target, DepthStencil depthStencil)
        {
            if (Application.InEditorMode)
            {
                context.SetRenderTarget(target, depthStencil);
                DebugDraw.SetViewport(viewport.Offset, viewport.Size);
                DebugDrawRenderer.EndDraw();
                context.SetViewport(viewport);
                DebugDrawRenderer.BeginDraw();
            }
        }

        public void Init()
        {
        }

        public void Release()
        {
        }
    }
}