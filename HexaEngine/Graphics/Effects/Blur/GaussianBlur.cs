namespace HexaEngine.Graphics.Effects.Blur
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Mathematics;
    using HexaEngine.PostFx;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public enum GaussianRadius
    {
        Radius3x3 = 3,
        Radius5x5 = 5,
        Radius7x7 = 7,
    }

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

        public GaussianBlur(IGraphResourceBuilder creator, Format format, int width, int height, GaussianRadius radius = GaussianRadius.Radius3x3, bool alphaBlend = false, bool additive = false, bool scissors = false, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            Format = format;
            Width = width;
            Height = height;
            device = creator.Device;

            RasterizerDescription rasterizerDescription = scissors ? RasterizerDescription.CullBackScissors : RasterizerDescription.CullBack;

            horizontal = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/blur/horizontal.hlsl",
                Macros = [new ShaderMacro("GAUSSIAN_RADIUS", (int)radius)]
            }, new()
            {
                Rasterizer = rasterizerDescription,
                Topology = PrimitiveTopology.TriangleStrip
            });

            vertical = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/blur/vertical.hlsl",
                Macros = [new ShaderMacro("GAUSSIAN_RADIUS", (int)radius)]
            }, new()
            {
                Rasterizer = rasterizerDescription,
                Blend = additive ? BlendDescription.Additive : alphaBlend ? BlendDescription.AlphaBlend : BlendDescription.Opaque,
                BlendFactor = Vector4.One,
                Topology = PrimitiveTopology.TriangleStrip
            });

            paramsBuffer = new(CpuAccessFlags.Write, filename + "_GAUSSIAN_BLUR_CONSTANT_BUFFER", lineNumber);
            linearClampSampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);

            intermediateTex = creator.CreateTexture2D(filename + "_GAUSSIAN_BLUR_BUFFER", new(format, width, height, 1, 1, GpuAccessFlags.RW), ResourceCreationFlags.Shared);
        }

        public GaussianBlur(IGraphResourceBuilder creator, string name, Format format, int width, int height, GaussianRadius radius = GaussianRadius.Radius3x3, bool alphaBlend = false, bool additive = false, bool scissors = false)
        {
            Format = format;
            Width = width;
            Height = height;
            device = creator.Device;

            RasterizerDescription rasterizerDescription = scissors ? RasterizerDescription.CullBackScissors : RasterizerDescription.CullBack;

            horizontal = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/blur/horizontal.hlsl",
                Macros = [new ShaderMacro("GAUSSIAN_RADIUS", (int)radius)]
            }, new()
            {
                Rasterizer = rasterizerDescription,
                Topology = PrimitiveTopology.TriangleStrip
            });

            vertical = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/blur/vertical.hlsl",
                Macros = [new ShaderMacro("GAUSSIAN_RADIUS", (int)radius)]
            }, new()
            {
                Rasterizer = rasterizerDescription,
                Blend = additive ? BlendDescription.Additive : alphaBlend ? BlendDescription.AlphaBlend : BlendDescription.Opaque,
                BlendFactor = Vector4.One,
                Topology = PrimitiveTopology.TriangleStrip
            });

            paramsBuffer = new(CpuAccessFlags.Write, name + "_GAUSSIAN_BLUR_CONSTANT_BUFFER");
            linearClampSampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);

            intermediateTex = creator.CreateTexture2D(name + "_GAUSSIAN_BLUR_BUFFER", new(format, width, height, 1, 1, GpuAccessFlags.RW), ResourceCreationFlags.Shared);
        }

        public GaussianBlur(PostFxGraphResourceBuilder creator, string name, Format format, GaussianRadius radius = GaussianRadius.Radius3x3, bool alphaBlend = false, bool additive = false, bool scissors = false)
        {
            device = creator.Device;

            RasterizerDescription rasterizerDescription = scissors ? RasterizerDescription.CullBackScissors : RasterizerDescription.CullBack;

            horizontal = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/blur/horizontal.hlsl",
                Macros = [new ShaderMacro("GAUSSIAN_RADIUS", (int)radius)]
            }, new()
            {
                Rasterizer = rasterizerDescription,
                Topology = PrimitiveTopology.TriangleStrip
            });

            vertical = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/blur/vertical.hlsl",
                Macros = [new ShaderMacro("GAUSSIAN_RADIUS", (int)radius)]
            }, new()
            {
                Rasterizer = rasterizerDescription,
                Blend = additive ? BlendDescription.Additive : alphaBlend ? BlendDescription.AlphaBlend : BlendDescription.Opaque,
                BlendFactor = Vector4.One,
                Topology = PrimitiveTopology.TriangleStrip
            });

            paramsBuffer = new(CpuAccessFlags.Write, name + "_GAUSSIAN_BLUR_CONSTANT_BUFFER");
            linearClampSampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);

            intermediateTex = creator.CreateTexture2D(name + "_GAUSSIAN_BLUR_BUFFER", new(format, creator.Width, creator.Height, 1, 1, GpuAccessFlags.RW), ResourceCreationFlags.Shared);
        }

        public GaussianBlur(PostFxGraphResourceBuilder creator, string name, GaussianRadius radius = GaussianRadius.Radius3x3, bool alphaBlend = false, bool additive = false, bool scissors = false)
        {
            device = creator.Device;

            RasterizerDescription rasterizerDescription = scissors ? RasterizerDescription.CullBackScissors : RasterizerDescription.CullBack;

            horizontal = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/blur/horizontal.hlsl",
                Macros = [new ShaderMacro("GAUSSIAN_RADIUS", (int)radius)]
            }, new()
            {
                Rasterizer = rasterizerDescription,
                Topology = PrimitiveTopology.TriangleStrip
            });

            vertical = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/blur/vertical.hlsl",
                Macros = [new ShaderMacro("GAUSSIAN_RADIUS", (int)radius)]
            }, new()
            {
                Rasterizer = rasterizerDescription,
                Blend = additive ? BlendDescription.Additive : alphaBlend ? BlendDescription.AlphaBlend : BlendDescription.Opaque,
                BlendFactor = Vector4.One,
                Topology = PrimitiveTopology.TriangleStrip
            });

            paramsBuffer = new(CpuAccessFlags.Write, name + "_GAUSSIAN_BLUR_CONSTANT_BUFFER");
            linearClampSampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);

            intermediateTex = creator.CreateBuffer(name + "_GAUSSIAN_BLUR_BUFFER", creationFlags: ResourceCreationFlags.Shared);
        }

        public BlurType Type => BlurType.Gaussian;

        public Format Format { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

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

            context.SetRenderTarget(null, null);
            context.PSSetShaderResource(0, null);

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
            context.SetViewport(new(intermediateTex.Value.Width, intermediateTex.Value.Height));
            context.PSSetShaderResource(0, src);
            context.PSSetSampler(0, linearClampSampler);
            context.PSSetConstantBuffer(0, paramsBuffer);
            context.SetPipelineState(horizontal);
            context.DrawInstanced(4, 1, 0, 0);

            context.SetRenderTarget(null, null);
            context.PSSetShaderResource(0, null);

            context.SetViewport(new(dstWidth, dstHeight));
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

        public unsafe void Blur(IGraphicsContext context, IShaderResourceView src, IRenderTargetView dst, float width, float height, Point4 scissors)
        {
            GaussianBlurParams gaussianBlurParams = default;
            gaussianBlurParams.TextureDimentions = new(width, height);
            paramsBuffer.Update(context, gaussianBlurParams);

            context.SetScissorRect(scissors.X, scissors.Y, scissors.Z, scissors.W);
            context.SetRenderTarget(intermediateTex.Value.RTV, null);
            context.SetViewport(new(width, height));
            context.PSSetShaderResource(0, src);
            context.PSSetSampler(0, linearClampSampler);
            context.PSSetConstantBuffer(0, paramsBuffer);
            context.SetPipelineState(horizontal);
            context.DrawInstanced(4, 1, 0, 0);

            context.SetRenderTarget(null, null);
            context.PSSetShaderResource(0, null);

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
            context.SetScissorRect(0, 0, 0, 0);
        }

        public void Resize(Format format, int width, int height)
        {
            Format = format;
            Width = width;
            Height = height;
            intermediateTex.Value.Resize(format, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
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

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}