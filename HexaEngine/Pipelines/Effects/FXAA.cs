#nullable disable

namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Objects.Primitives;
    using HexaEngine.Resources;

    public class FXAA : IEffect
    {
        private Quad quad;
        private IGraphicsPipeline pipeline;
        private ISamplerState sampler;

        public IRenderTargetView Output;
        public IShaderResourceView Source;

        public async Task Initialize(IGraphicsDevice device, int width, int height)
        {
            Source = ResourceManager.AddTextureSRV("FXAA", TextureDescription.CreateTexture2DWithRTV(width, height, 1));

            quad = new Quad(device);
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "effects/fxaa/vs.hlsl",
                PixelShader = "effects/fxaa/ps.hlsl"
            });
            sampler = device.CreateSamplerState(SamplerDescription.AnisotropicClamp);

            Output = await ResourceManager.GetResourceAsync<IRenderTargetView>("SwapChain.RTV");
        }

        public void BeginResize()
        {
            ResourceManager.RequireUpdate("FXAA");
        }

        public void EndResize(int width, int height)
        {
            Output = ResourceManager.GetResource<IRenderTargetView>("SwapChain.RTV");
            Source = ResourceManager.UpdateTextureSRV("FXAA", TextureDescription.CreateTexture2DWithRTV(width, height, 1));
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
            ResourceManager.RemoveResource("FXAA");
            GC.SuppressFinalize(this);
        }
    }
}