﻿namespace HexaEngine.Graphics.Effects.Blur
{
    using HexaEngine.Core.Graphics;

    public interface IBlur : IDisposable
    {
        public BlurType Type { get; }

        void Blur(IGraphicsContext context, IShaderResourceView src, IRenderTargetView dst, float width, float height);
    }
}