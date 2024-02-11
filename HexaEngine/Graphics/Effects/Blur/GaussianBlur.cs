namespace HexaEngine.Graphics.Effects.Blur
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.PostFx;
    using Silk.NET.Core.Win32Extras;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public class GaussianBlur : IBlur
    {
        private readonly IGraphicsPipelineState horizontal;
        private readonly IGraphicsPipelineState vertical;
        private readonly ConstantBuffer<GaussianBlurParams> paramsBuffer;
        private readonly ISamplerState linearClampSampler;
        private readonly IGraphicsDevice device;
        private readonly ResourceRef<Texture2D> intermediateTex;
        private bool disposedValue;

        private struct GaussianBlurParams
        {
            public Vector2 TextureDimentions;
            public Vector2 padd;
        }

        public GaussianBlur(IGraphResourceBuilder creator, Format format, int width, int height, bool alphaBlend = false, bool additive = false, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            device = creator.Device;

            horizontal = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/blur/horizontal.hlsl",
            }, GraphicsPipelineStateDesc.DefaultFullscreen);

            vertical = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/blur/vertical.hlsl",
            }, new()
            {
                Blend = additive ? BlendDescription.Additive : alphaBlend ? BlendDescription.AlphaBlend : BlendDescription.Opaque,
                BlendFactor = Vector4.One,
                Topology = PrimitiveTopology.TriangleStrip
            });

            paramsBuffer = new(device, CpuAccessFlags.Write, filename + "_GAUSSIAN_BLUR_CONSTANT_BUFFER", lineNumber);
            linearClampSampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);

            intermediateTex = creator.CreateTexture2D(filename + "_GAUSSIAN_BLUR_BUFFER", new(format, width, height, 1, 1, GpuAccessFlags.RW), ResourceCreationFlags.None);
        }

        public GaussianBlur(IGraphResourceBuilder creator, string name, Format format, int width, int height, bool alphaBlend = false, bool additive = false)
        {
            device = creator.Device;

            horizontal = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/blur/horizontal.hlsl",
            }, GraphicsPipelineStateDesc.DefaultFullscreen);

            vertical = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/blur/vertical.hlsl",
            }, new()
            {
                Blend = additive ? BlendDescription.Additive : alphaBlend ? BlendDescription.AlphaBlend : BlendDescription.Opaque,
                BlendFactor = Vector4.One,
                Topology = PrimitiveTopology.TriangleStrip
            });

            paramsBuffer = new(device, CpuAccessFlags.Write, name + "_GAUSSIAN_BLUR_CONSTANT_BUFFER");
            linearClampSampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);

            intermediateTex = creator.CreateTexture2D(name + "_GAUSSIAN_BLUR_BUFFER", new(format, width, height, 1, 1, GpuAccessFlags.RW), ResourceCreationFlags.None);
        }

        public GaussianBlur(PostFxGraphResourceBuilder creator, string name, Format format, bool alphaBlend = false, bool additive = false)
        {
            device = creator.Device;

            horizontal = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/blur/horizontal.hlsl",
            }, GraphicsPipelineStateDesc.DefaultFullscreen);

            vertical = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/blur/vertical.hlsl",
            }, new()
            {
                Blend = additive ? BlendDescription.Additive : alphaBlend ? BlendDescription.AlphaBlend : BlendDescription.Opaque,
                BlendFactor = Vector4.One,
                Topology = PrimitiveTopology.TriangleStrip
            });

            paramsBuffer = new(device, CpuAccessFlags.Write, name + "_GAUSSIAN_BLUR_CONSTANT_BUFFER");
            linearClampSampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);

            intermediateTex = creator.CreateTexture2D(name + "_GAUSSIAN_BLUR_BUFFER", new(format, creator.Width, creator.Height, 1, 1, GpuAccessFlags.RW), ResourceCreationFlags.None);
        }

        public GaussianBlur(PostFxGraphResourceBuilder creator, string name, bool alphaBlend = false, bool additive = false)
        {
            device = creator.Device;

            horizontal = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/blur/horizontal.hlsl",
            }, GraphicsPipelineStateDesc.DefaultFullscreen);

            vertical = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/blur/vertical.hlsl",
            }, new()
            {
                Blend = additive ? BlendDescription.Additive : alphaBlend ? BlendDescription.AlphaBlend : BlendDescription.Opaque,
                BlendFactor = Vector4.One,
                Topology = PrimitiveTopology.TriangleStrip
            });

            paramsBuffer = new(device, CpuAccessFlags.Write, name + "_GAUSSIAN_BLUR_CONSTANT_BUFFER");
            linearClampSampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);

            intermediateTex = creator.CreateBuffer(name + "_GAUSSIAN_BLUR_BUFFER");
        }

        public BlurType Type => BlurType.Gaussian;

        public unsafe void Blur(IGraphicsContext context, IShaderResourceView src, IRenderTargetView dst, float width, float height)
        {
            GaussianBlurParams gaussianBlurParams = default;
            gaussianBlurParams.TextureDimentions = new(width, height);
            paramsBuffer.Update(context, gaussianBlurParams);

            context.SetRenderTarget(intermediateTex.Value.RTV, null);
            context.SetViewport(new(width, height));
            context.PSSetShaderResource(0, src);
            context.PSSetSampler(0, linearClampSampler);
            context.PSSetConstantBuffer(0, paramsBuffer);
            context.SetPipelineState(horizontal);
            context.DrawInstanced(4, 1, 0, 0);

            context.SetRenderTarget(dst, null);
            context.PSSetShaderResource(0, intermediateTex.Value.SRV);
            context.SetPipelineState(vertical);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetPipelineState(null);
            context.PSSetConstantBuffer(0, null);
            context.PSSetSampler(0, null);
            context.PSSetShaderResource(0, null);
            context.SetViewport(default);
            context.SetRenderTarget(null, null);
        }

        public unsafe void Blur(IGraphicsContext context, IShaderResourceView src, IRenderTargetView dst, float srcWidth, float srcHeight, float dstWidth, float dstHeight)
        {
            GaussianBlurParams gaussianBlurParams = default;
            gaussianBlurParams.TextureDimentions = new(srcWidth, srcHeight);
            paramsBuffer.Update(context, gaussianBlurParams);

            context.SetRenderTarget(intermediateTex.Value.RTV, null);
            context.SetViewport(new(dstWidth, dstHeight));
            context.PSSetShaderResource(0, src);
            context.PSSetSampler(0, linearClampSampler);
            context.PSSetConstantBuffer(0, paramsBuffer);
            context.SetPipelineState(horizontal);
            context.DrawInstanced(4, 1, 0, 0);

            context.SetRenderTarget(dst, null);
            context.PSSetShaderResource(0, intermediateTex.Value.SRV);
            context.SetPipelineState(vertical);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetPipelineState(null);
            context.PSSetConstantBuffer(0, null);
            context.PSSetSampler(0, null);
            context.PSSetShaderResource(0, null);
            context.SetViewport(default);
            context.SetRenderTarget(null, null);
        }

        public void Resize(Format format, int width, int height)
        {
            intermediateTex.Value.Resize(device, format, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                vertical.Dispose();
                horizontal.Dispose();
                paramsBuffer.Dispose();
                linearClampSampler.Dispose();
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