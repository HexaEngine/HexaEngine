namespace HexaEngine.Core.Graphics.Specialized
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using System;
    using System.Numerics;

    public interface IRenderTarget : IDisposable
    {
        Viewport Viewport { get; }

        void ClearAndSetTarget(IGraphicsContext context, Vector4 color, DepthStencilClearFlags flags = DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, float depth = 1, byte stencil = 0);

        void ClearTarget(IGraphicsContext context, Vector4 color, DepthStencilClearFlags flags = DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, float depth = 1, byte stencil = 0);

        void SetTarget(IGraphicsContext context);
    }
}