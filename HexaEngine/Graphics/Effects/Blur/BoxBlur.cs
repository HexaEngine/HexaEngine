namespace HexaEngine.Graphics.Effects.Blur
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics.Graph;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public class BoxBlur : IBlur
    {
        private readonly IGraphicsPipelineState pso;
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
            pso = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/blur/box.hlsl",
            }, GraphicsPipelineStateDesc.DefaultFullscreen);

            paramsBuffer = new(CpuAccessFlags.Write, filename + "-BoxBlur", lineNumber);
            linearClampSampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
        }

        public BoxBlur(IGraphResourceBuilder creator, string name)
        {
            var device = creator.Device;

            pso = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/blur/box.hlsl",
            }, GraphicsPipelineStateDesc.DefaultFullscreen);

            paramsBuffer = new(CpuAccessFlags.Write, name + "_BOX_BLUR_CONSTANT_BUFFER");
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
            context.SetPipelineState(pso);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetPipelineState(null);
            context.PSSetShaderResource(0, null);
            context.PSSetSampler(0, null);
            context.PSSetConstantBuffer(0, null);
            context.SetViewport(default);
            context.SetRenderTarget(null, null);
        }

        public void Blur(IGraphicsContext context, IShaderResourceView src, IRenderTargetView dst, float srcWidth, float srcHeight, float dstWidth, float dstHeight)
        {
            BoxBlurParams boxBlurParams = default;
            boxBlurParams.TextureDimentions = new(srcWidth, srcHeight);
            boxBlurParams.Size = size;
            paramsBuffer.Update(context, boxBlurParams);

            context.SetRenderTarget(dst, null);
            context.SetViewport(new(dstWidth, dstHeight));
            context.PSSetConstantBuffer(0, paramsBuffer);
            context.PSSetSampler(0, linearClampSampler);
            context.PSSetShaderResource(0, src);
            context.SetPipelineState(pso);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetPipelineState(null);
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
                pso.Dispose();
                paramsBuffer.Dispose();
                linearClampSampler.Dispose();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
        }
    }
}