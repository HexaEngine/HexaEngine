namespace HexaEngine.Core.Effects.Blur
{
    using HexaEngine.Core.Graphics;

    public interface IBlur : IDisposable
    {
        public BlurType Type { get; }

        void Blur(IGraphicsContext context, IShaderResourceView src, IRenderTargetView dst, int width, int height);
    }
}