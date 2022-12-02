namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Objects.Primitives;

    public class FXAA : IEffect
    {
        private readonly Quad quad;
        private readonly Pipeline pipeline;
        private readonly ISamplerState sampler;

        public IRenderTargetView? Output;
        public IShaderResourceView? Source;

        public FXAA(IGraphicsDevice device)
        {
            quad = new Quad(device);
            pipeline = new(device, new()
            {
                VertexShader = "effects/fxaa/vs.hlsl",
                PixelShader = "effects/fxaa/ps.hlsl"
            });
            sampler = device.CreateSamplerState(SamplerDescription.AnisotropicClamp);
        }

        public void Draw(IGraphicsContext context)
        {
            if (Output == null) return;
            context.ClearRenderTargetView(Output, default);
            context.SetRenderTarget(Output, default);
            context.PSSetShaderResource(Source, 0);
            context.PSSetSampler(sampler, 0);
            quad.DrawAuto(context, pipeline, Output.Viewport);
            context.ClearState();
        }

        public void Draw(IGraphicsContext context, Viewport viewport)
        {
            if (Output == null) return;
            context.ClearRenderTargetView(Output, default);
            context.SetRenderTarget(Output, default);
            context.PSSetShaderResource(Source, 0);
            context.PSSetSampler(sampler, 0);
            quad.DrawAuto(context, pipeline, viewport);
            context.ClearState();
        }

        public void Dispose()
        {
            quad.Dispose();
            pipeline.Dispose();
            sampler.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}