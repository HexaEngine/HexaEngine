namespace HexaEngine.Graphics.Effects
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics.Graph;
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
        private readonly IGraphicsPipelineState pipeline;
        private readonly ISamplerState samplerState;
        private readonly ConstantBuffer<Vector4> paramBuffer;
        private bool disposedValue;

        public CopyEffect(IGraphicsDevice device, CopyFilter filter)
        {
            ShaderMacro[] macros = filter != CopyFilter.None ? [new("SAMPLED", 1)] : [];
            pipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "HexaEngine.Core:shaders/quad.hlsl",
                PixelShader = "HexaEngine.Core:shaders/effects/copy/ps.hlsl",
                Macros = macros
            }, GraphicsPipelineStateDesc.DefaultFullscreen);

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
            paramBuffer = new(CpuAccessFlags.Write);

            SetupState();
        }

        public CopyEffect(IGraphResourceBuilder creator, string name, CopyFilter filter)
        {
            ShaderMacro[] macros = filter != CopyFilter.None ? [new("SAMPLED", 1)] : [];

            var device = creator.Device;

            pipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "HexaEngine.Core:shaders/quad.hlsl",
                PixelShader = "HexaEngine.Core:shaders/effects/copy/ps.hlsl",
                Macros = macros
            }, GraphicsPipelineStateDesc.DefaultFullscreen);

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
            paramBuffer = new(CpuAccessFlags.Write);

            SetupState();
        }

        private void SetupState()
        {
            pipeline.Bindings.SetCBV("params", paramBuffer);
            pipeline.Bindings.SetSampler("samplerState", samplerState);
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
            pipeline.Bindings.SetSRV("sourceTex", source);
            context.SetGraphicsPipelineState(pipeline);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipelineState(null);
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

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}