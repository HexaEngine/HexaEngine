namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using System.Numerics;

    public class BloomEffect : IEffect
    {
        private RenderTexture[] mipChain;
        private IVertexShader? vs;
        private IPixelShader? psDownsample;
        private IPixelShader? psUpsample;
        private IInputLayout? layout;
        private IRasterizerState rasterizerState;
        private IDepthStencilState depthStencilState;
        private IBlendState blendState;
        private IBuffer downsampleCB;
        private IBuffer upsampleCB;

        private struct ParamsDownsample
        {
            public Vector2 SrcResolution;
            public Vector2 Padd;

            public ParamsDownsample(Vector2 srcResolution, Vector2 padd)
            {
                SrcResolution = srcResolution;
                Padd = padd;
            }
        }

        private struct ParamsUpsample
        {
            public float FilterRadius;
            public Vector3 Padd;

            public ParamsUpsample(float filterRadius, Vector3 padd)
            {
                FilterRadius = filterRadius;
                Padd = padd;
            }
        }

        public BloomEffect(IGraphicsDevice device)
        {
            mipChain = new RenderTexture[8];
            device.CompileFromFile("", "", "", out var bvs);
            if (bvs != null)
                vs = device.CreateVertexShader(bvs);
            if (bvs != null)
                layout = device.CreateInputLayout(bvs);
            device.CompileFromFile("", "", "", out var bps1);
            if (bps1 != null)
                psDownsample = device.CreatePixelShader(bps1);
            device.CompileFromFile("", "", "", out var bps2);
            if (bps2 != null)
                psUpsample = device.CreatePixelShader(bps2);
            rasterizerState = device.CreateRasterizerState(RasterizerDescription.CullBack);
            depthStencilState = device.CreateDepthStencilState(DepthStencilDescription.None);
            blendState = device.CreateBlendState(BlendDescription.Opaque);
            downsampleCB = device.CreateBuffer(new ParamsDownsample(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            upsampleCB = device.CreateBuffer(new ParamsUpsample(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
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
            context.VSSetShader(vs);

            context.SetInputLayout(layout);

            context.SetBlendState(blendState);
            context.SetRasterizerState(rasterizerState);
            context.SetDepthStencilState(depthStencilState);

            context.PSSetShader(psUpsample);
            context.SetConstantBuffer(upsampleCB, ShaderStage.Pixel, 0);

            context.PSSetShader(psDownsample);
            context.SetConstantBuffer(downsampleCB, ShaderStage.Pixel, 0);
        }

        public void DrawSettings()
        {
            throw new NotImplementedException();
        }
    }
}