namespace HexaEngine.Core.Graphics.Specialized
{
    using HexaEngine.Core.Graphics;
    using System;

    public interface IDepthStencil : IDisposable
    {
        void ClearDepthStencil(IGraphicsContext context, DepthStencilClearFlags flags = DepthStencilClearFlags.None, float depth = 1, byte stencil = 0);

        public IDepthStencilView DepthStencilView { get; }
    }
}