namespace HexaEngine.Graphics.Effects.Blur
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public class GaussianBlur : IBlur
    {
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

        public GaussianBlur(IGraphicsDevice device, Format format, int width, int height, bool alphaBlend = false, bool additive = false, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            horizontal = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/blur/horizontal.hlsl"
            }, GraphicsPipelineState.DefaultFullscreen);
            vertical = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/blur/vertical.hlsl"
            }, new GraphicsPipelineState()
            {
                Blend = additive ? BlendDescription.Additive : alphaBlend ? BlendDescription.AlphaBlend : BlendDescription.Opaque,
                BlendFactor = Vector4.One,
                Topology = PrimitiveTopology.TriangleStrip
            });

            paramsBuffer = new(device, CpuAccessFlags.Write, filename + "-GaussianBlur", lineNumber);
            linearClampSampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);

            intermediateTex = new(device, format, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW, ResourceMiscFlag.None, filename, lineNumber);
            this.device = device;
        }

        public int BlurXOffset { get => blurXOffset; set => blurXOffset = value; }

        public BlurType Type => BlurType.Gaussian;

        public unsafe void Blur(IGraphicsContext context, IShaderResourceView src, IRenderTargetView dst, float width, float height)
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
            context.SetGraphicsPipeline(horizontal);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipeline(null);

            context.SetRenderTarget(dst, null);
            context.PSSetShaderResource(0, intermediateTex.SRV);
            context.SetGraphicsPipeline(vertical);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipeline(null);
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
            context.SetGraphicsPipeline(horizontal);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipeline(null);

            context.SetRenderTarget(dst, null);
            context.PSSetShaderResource(0, intermediateTex.SRV);
            context.SetGraphicsPipeline(vertical);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipeline(null);
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