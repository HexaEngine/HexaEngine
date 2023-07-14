namespace HexaEngine.Effects.Blur
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using System.Numerics;

    public class BoxBlur : IBlur
    {
        private readonly Quad quad;
        private readonly IGraphicsPipeline pipeline;
        private readonly ConstantBuffer<BoxBlurParams> paramsBuffer;
        private readonly ISamplerState linearClampSampler;
        private bool disposedValue;
        private int size;

        private struct BoxBlurParams
        {
            public Vector2 TextureDimentions;
            public int Size;
            public float padd;
        }

        public BoxBlur(IGraphicsDevice device)
        {
            quad = new(device);
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "effects/blur/vs.hlsl",
                PixelShader = "effects/blur/box.hlsl"
            });
            paramsBuffer = new(device, CpuAccessFlags.Write);
            linearClampSampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
        }

        public int Size { get => size; set => size = value; }

        public BlurType Type => BlurType.Box;

        public void Blur(IGraphicsContext context, IShaderResourceView src, IRenderTargetView dst, int width, int height)
        {
            BoxBlurParams boxBlurParams = default;
            boxBlurParams.TextureDimentions = new(width, height);
            boxBlurParams.Size = size;
            paramsBuffer.Update(context, boxBlurParams);

            context.SetRenderTarget(dst, null);
            context.SetViewport(new(width, height));
            context.PSSetConstantBuffer(0, paramsBuffer);
            context.PSSetSampler(0, linearClampSampler);
            context.PSSetShaderResource(0, src);
            quad.DrawAuto(context, pipeline);
            context.PSSetShaderResource(0, null);
            context.PSSetSampler(0, null);
            context.PSSetConstantBuffer(0, null);
            context.SetViewport(default);
            context.SetRenderTarget(null, null);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                quad.Dispose();
                pipeline.Dispose();
                paramsBuffer.Dispose();
                linearClampSampler.Dispose();
                disposedValue = true;
            }
        }

        ~BoxBlur()
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