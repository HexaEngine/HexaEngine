namespace HexaEngine.PostFx
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;

    public interface IPostFxPass
    {
        void Dispose();

        void Execute(IGraphicsContext context);

        void SetInput(IShaderResourceView view, ITexture2D resource);

        void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport);
    }
}