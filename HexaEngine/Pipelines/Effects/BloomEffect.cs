namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;

    public class BloomEffect : IEffect
    {
        private RenderTexture[] mipChain;
        private IVertexShader? vs;
        private IPixelShader? psDownsample;
        private IPixelShader? psUpsample;
        private IInputLayout? layout;
        private IRasterizerState? rasterizerState;
        private IDepthStencilState? depthStencilState;
        private IBlendState? blendState;
        private IBuffer downsampleCB;
        private IBuffer upsampleCB;

        public BloomEffect(IGraphicsDevice device)
        {
        }

        public void Reload()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Draw(IGraphicsContext context)
        {
        }

        public void DrawSettings()
        {
            throw new NotImplementedException();
        }
    }
}