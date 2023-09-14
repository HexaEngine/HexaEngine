namespace HexaEngine.Graphics.Effects.Blur
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public class BoxBlur : IBlur
    {
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

        public BoxBlur(IGraphicsDevice device, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/blur/box.hlsl"
            }, GraphicsPipelineState.DefaultFullscreen);
            paramsBuffer = new(device, CpuAccessFlags.Write, filename + "-BoxBlur", lineNumber);
            linearClampSampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
        }

        public int Size { get => size; set => size = value; }

        public BlurType Type => BlurType.Box;

        public void Blur(IGraphicsContext context, IShaderResourceView src, IRenderTargetView dst, float width, float height)
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
            context.SetGraphicsPipeline(pipeline);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipeline(null);
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