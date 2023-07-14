namespace HexaEngine.Effects.Blur
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using System.Numerics;

    public class GaussianBlur : IBlur
    {
        private readonly Quad quad;
        private readonly IGraphicsPipeline horizontal;
        private readonly IGraphicsPipeline vertical;
        private readonly ConstantBuffer<GaussianBlurParams> paramsBuffer;
        private readonly ISamplerState linearClampSampler;
        private readonly IGraphicsDevice device;
        private readonly Texture2D intermediateTex;
        private int blurXOffset;
        private bool disposedValue;

        private struct GaussianBlurParams
        {
            public Vector2 TextureDimentions;
            public float BlurXOffset;
            public float padd;
        }

        public GaussianBlur(IGraphicsDevice device, Format format, int width, int height)
        {
            quad = new(device);
            horizontal = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "effects/blur/vs.hlsl",
                PixelShader = "effects/blur/horizontal.hlsl"
            });
            vertical = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "effects/blur/vs.hlsl",
                PixelShader = "effects/blur/vertical.hlsl"
            }, new GraphicsPipelineState()
            {
                Blend = BlendDescription.AlphaBlend,
                BlendFactor = Vector4.One
            });

            paramsBuffer = new(device, CpuAccessFlags.Write);
            linearClampSampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);

            intermediateTex = new(device, format, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            this.device = device;
        }

        public int BlurXOffset { get => blurXOffset; set => blurXOffset = value; }

        public BlurType Type => BlurType.Gaussian;

        public unsafe void Blur(IGraphicsContext context, IShaderResourceView src, IRenderTargetView dst, int width, int height)
        {
            GaussianBlurParams gaussianBlurParams = default;
            gaussianBlurParams.TextureDimentions = new(width, height);
            gaussianBlurParams.BlurXOffset = blurXOffset;
            paramsBuffer.Update(context, gaussianBlurParams);

            context.SetRenderTarget(intermediateTex.RTV, null);
            context.SetViewport(new(width, height));
            context.PSSetShaderResource(0, src);
            context.PSSetSampler(0, linearClampSampler);
            context.PSSetConstantBuffer(0, paramsBuffer);
            quad.DrawAuto(context, horizontal);

            context.SetRenderTarget(dst, null);
            context.PSSetShaderResource(0, intermediateTex.SRV);
            quad.DrawAuto(context, vertical);
            context.PSSetConstantBuffer(0, null);
            context.PSSetSampler(0, null);
            context.PSSetShaderResource(0, null);
            context.SetViewport(default);
            context.SetRenderTarget(null, null);
        }

        public unsafe void Blur(IGraphicsContext context, IShaderResourceView src, IRenderTargetView dst, int srcWidth, int srcHeight, int dstWidth, int dstHeight)
        {
            GaussianBlurParams gaussianBlurParams = default;
            gaussianBlurParams.TextureDimentions = new(srcWidth, srcHeight);
            gaussianBlurParams.BlurXOffset = blurXOffset;
            paramsBuffer.Update(context, gaussianBlurParams);

            context.SetRenderTarget(intermediateTex.RTV, null);
            context.SetViewport(new(dstWidth, dstHeight));
            context.PSSetShaderResource(0, src);
            context.PSSetSampler(0, linearClampSampler);
            context.PSSetConstantBuffer(0, paramsBuffer);
            quad.DrawAuto(context, horizontal);

            context.SetRenderTarget(dst, null);
            context.PSSetShaderResource(0, intermediateTex.SRV);
            quad.DrawAuto(context, vertical);
            context.PSSetConstantBuffer(0, null);
            context.PSSetSampler(0, null);
            context.PSSetShaderResource(0, null);
            context.SetViewport(default);
            context.SetRenderTarget(null, null);
        }

        public void Resize(Format format, int width, int height)
        {
            intermediateTex.Resize(device, format, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                quad.Dispose();
                vertical.Dispose();
                horizontal.Dispose();
                paramsBuffer.Dispose();
                linearClampSampler.Dispose();
                intermediateTex.Dispose();
                disposedValue = true;
            }
        }

        ~GaussianBlur()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}