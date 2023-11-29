namespace HexaEngine.Graphics.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public enum CopyFilter
    {
        None = 0,
        Point,
        Bilinear,
        Trilinear,
        Anisotropic,
    }

    public class CopyEffect : IDisposable
    {
        private readonly IGraphicsPipeline pipeline;
        private readonly ISamplerState samplerState;
        private readonly ConstantBuffer<Vector4> paramBuffer;
        private bool disposedValue;

        public CopyEffect(IGraphicsDevice device, CopyFilter filter)
        {
            ShaderMacro[] macros = filter != CopyFilter.None ? new ShaderMacro[] { new("SAMPLED", 1) } : Array.Empty<ShaderMacro>();
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/copy/ps.hlsl",
                State = GraphicsPipelineState.Default,
                Macros = macros
            });

            SamplerStateDescription description = SamplerStateDescription.PointClamp;

            switch (filter)
            {
                case CopyFilter.Point:
                    description = new(Filter.MinMagMipPoint, TextureAddressMode.Clamp);
                    break;

                case CopyFilter.Bilinear:
                    description = new(Filter.MinMagLinearMipPoint, TextureAddressMode.Clamp);
                    break;

                case CopyFilter.Trilinear:
                    description = new(Filter.MinMagMipLinear, TextureAddressMode.Clamp);
                    break;

                case CopyFilter.Anisotropic:
                    description = new(Filter.Anisotropic, TextureAddressMode.Clamp);
                    break;
            }

            samplerState = device.CreateSamplerState(description);
            paramBuffer = new(device, CpuAccessFlags.Write);
        }

        public void Copy(IGraphicsContext context, IShaderResourceView source, IRenderTargetView destination, Vector2 srcSize)
        {
            Copy(context, source, destination, new Viewport(Vector2.Zero, srcSize));
        }

        public void Copy(IGraphicsContext context, IShaderResourceView source, IRenderTargetView destination, Vector2 srcOffset, Vector2 srcSize)
        {
            Copy(context, source, destination, new Viewport(srcOffset, srcSize));
        }

        public void Copy(IGraphicsContext context, IShaderResourceView source, IRenderTargetView destination, Viewport srcViewport)
        {
            Copy(context, source, destination, srcViewport, srcViewport);
        }

        public void Copy(IGraphicsContext context, IShaderResourceView source, IRenderTargetView destination, Vector2 srcOffset, Vector2 srcSize, Vector2 dstOffset, Vector2 dstSize)
        {
            Copy(context, source, destination, new Viewport(srcOffset, srcSize), new Viewport(dstOffset, dstSize));
        }

        public void Copy(IGraphicsContext context, IShaderResourceView source, IRenderTargetView destination, Viewport srcViewport, Viewport dstViewport)
        {
            Vector4 vector = new(srcViewport.X, srcViewport.Y, srcViewport.Width - srcViewport.X, srcViewport.Height - srcViewport.Y);
            paramBuffer.Update(context, vector);
            context.SetRenderTarget(destination, null);
            context.SetViewport(dstViewport);
            context.PSSetShaderResource(0, source);
            context.PSSetSampler(0, samplerState);
            context.PSSetConstantBuffer(0, paramBuffer);
            context.SetGraphicsPipeline(pipeline);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipeline(null);
            context.PSSetShaderResource(0, null);
            context.SetViewport(default);
            context.SetRenderTarget(null, null);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                pipeline.Dispose();
                samplerState.Dispose();
                paramBuffer.Dispose();
                disposedValue = true;
            }
        }

        ~CopyEffect()
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