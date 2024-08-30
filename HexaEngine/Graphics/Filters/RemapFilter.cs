namespace HexaEngine.Graphics.Filters
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics.Graph;
    using System.Numerics;

    public class RemapFilter : DisposableBase
    {
        private readonly IGraphicsPipelineState pso;
        private readonly ConstantBuffer<Params> paramsBuffer;
        private Vector4 min = Vector4.Zero;
        private Vector4 max = Vector4.One;

        private struct Params
        {
            public Vector4 Min;
            public Vector4 Range;
            public Vector2 TexSize;
            public Vector2 Padding;

            public Params(Vector4 min, Vector4 max, Vector2 texSize)
            {
                Min = min;
                Range = max - min;
                TexSize = texSize;
            }
        }

        public RemapFilter(IGraphicsDevice device, bool alphaBlend = false, bool additive = false, bool scissors = false)
        {
            GraphicsPipelineStateDesc stateDesc = GraphicsPipelineStateDesc.DefaultFullscreen;

            if (alphaBlend)
            {
                stateDesc = GraphicsPipelineStateDesc.DefaultAlphaBlendFullscreen;
            }

            if (additive)
            {
                stateDesc = GraphicsPipelineStateDesc.DefaultAdditiveFullscreen;
            }

            if (scissors)
            {
                stateDesc.Rasterizer.ScissorEnable = true;
            }

            pso = device.CreateGraphicsPipelineState(new()
            {
                Pipeline = new()
                {
                    VertexShader = "quad.hlsl",
                    PixelShader = "filter/remap/ps.hlsl",
                },
                State = stateDesc,
            });
            paramsBuffer = new(CpuAccessFlags.Write);
            pso.Bindings.SetCBV("RemapParams", paramsBuffer);
        }

        public RemapFilter(IGraphResourceBuilder creator, bool alphaBlend = false, bool additive = false, bool scissors = false)
        {
            var device = creator.Device;
            GraphicsPipelineStateDesc stateDesc = GraphicsPipelineStateDesc.DefaultFullscreen;

            if (alphaBlend)
            {
                stateDesc = GraphicsPipelineStateDesc.DefaultAlphaBlendFullscreen;
            }

            if (additive)
            {
                stateDesc = GraphicsPipelineStateDesc.DefaultAdditiveFullscreen;
            }

            if (scissors)
            {
                stateDesc.Rasterizer.ScissorEnable = true;
            }

            pso = device.CreateGraphicsPipelineState(new()
            {
                Pipeline = new()
                {
                    VertexShader = "quad.hlsl",
                    PixelShader = "filters/remap/ps.hlsl",
                },
                State = stateDesc,
            });
            paramsBuffer = new(CpuAccessFlags.Write);
        }

        public Vector4 Min { get => min; set => min = value; }

        public Vector4 Max { get => max; set => max = value; }

        public void Remap(IGraphicsContext context, IShaderResourceView source, IRenderTargetView destination, int width, int height)
        {
            paramsBuffer.Update(context, new(min, max, new(width, height)));
            context.SetGraphicsPipelineState(pso);
            context.SetRenderTarget(destination, null);
            context.SetViewport(new(width, height));
            pso.Bindings.SetSRV("inputTex", source);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipelineState(null);
            context.SetRenderTarget(null, null);
        }

        public void Remap(IGraphicsContext context, IShaderResourceView source, IRenderTargetView destination, int srcWidth, int srcHeight, int dstWidth, int dstHeight)
        {
            paramsBuffer.Update(context, new(min, max, new(srcWidth, srcHeight)));
            context.SetGraphicsPipelineState(pso);
            context.SetRenderTarget(destination, null);
            context.SetViewport(new(dstWidth, dstHeight));
            pso.Bindings.SetSRV("inputTex", source);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipelineState(null);
            context.SetRenderTarget(null, null);
        }

        public void Remap(IGraphicsContext context, IShaderResourceView source, IRenderTargetView destination, Point4 scissors, int width, int height)
        {
            paramsBuffer.Update(context, new(min, max, new(width, height)));
            context.SetGraphicsPipelineState(pso);
            context.SetRenderTarget(destination, null);
            context.SetViewport(new(width, height));
            context.SetScissorRect(scissors.X, scissors.Y, scissors.Z, scissors.W);
            pso.Bindings.SetSRV("inputTex", source);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipelineState(null);
            context.SetRenderTarget(null, null);
            context.SetScissorRect(0, 0, 0, 0);
        }

        public void Remap(IGraphicsContext context, IShaderResourceView source, IRenderTargetView destination, Point4 scissors, int srcWidth, int srcHeight, int dstWidth, int dstHeight)
        {
            paramsBuffer.Update(context, new(min, max, new(srcWidth, srcHeight)));
            context.SetGraphicsPipelineState(pso);
            context.SetRenderTarget(destination, null);
            context.SetViewport(new(dstWidth, dstHeight));
            context.SetScissorRect(scissors.X, scissors.Y, scissors.Z, scissors.W);
            pso.Bindings.SetSRV("inputTex", source);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipelineState(null);
            context.SetRenderTarget(null, null);
            context.SetScissorRect(0, 0, 0, 0);
        }

        protected override void DisposeCore()
        {
            pso.Dispose();
            paramsBuffer.Dispose();
        }
    }
}