namespace HexaEngine.Effects.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.PostFx;
    using HexaEngine.Rendering.Graph;
    using HexaEngine.Scenes;
    using System.Threading.Tasks;

    public class SSR : PostFxBase
    {
        private IGraphicsDevice device;
        private IGraphicsPipeline pipelineSSR;

        private ISamplerState pointClampSampler;
        private ISamplerState linearClampSampler;
        private ISamplerState linearBorderSampler;

        public IRenderTargetView Output;
        public Viewport Viewport;
        public IShaderResourceView Input;

        public override string Name { get; } = "SSR";

        public override PostFxFlags Flags { get; } = PostFxFlags.None;

        public override async Task Initialize(IGraphicsDevice device, PostFxDependencyBuilder builder, int width, int height, ShaderMacro[] macros)
        {
            builder
                .RunBefore("Compose")
                .RunAfter("TAA")
                .RunAfter("HBAO")
                .RunAfter("MotionBlur")
                .RunAfter("DepthOfField")
                .RunAfter("GodRays")
                .RunAfter("VolumetricClouds")
                .RunBefore("SSGI")
                .RunBefore("LensFlare")
                .RunBefore("Bloom")
                .RunBefore("AutoExposure");

            this.device = device;

            pointClampSampler = device.CreateSamplerState(SamplerStateDescription.PointClamp);
            linearClampSampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
            linearBorderSampler = device.CreateSamplerState(SamplerStateDescription.LinearBorder);

            pipelineSSR = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/ssr/ps.hlsl",
            }, GraphicsPipelineState.DefaultFullscreen, macros);
        }

        public override void Draw(IGraphicsContext context, GraphResourceBuilder creator)
        {
            if (Output == null)
                return;

            var depth = creator.GetDepthStencilBuffer("#DepthStencil");
            var camera = creator.GetConstantBuffer<CBCamera>("CBCamera");
            var gbuffer = creator.GetGBuffer("GBuffer");

            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);
            context.PSSetShaderResource(0, gbuffer.SRVs[1]);
            context.PSSetShaderResource(1, Input);
            context.PSSetShaderResource(2, depth.SRV);
            context.PSSetShaderResource(3, gbuffer.SRVs[2]);
            context.PSSetConstantBuffer(1, camera);
            context.PSSetSampler(0, pointClampSampler);
            context.PSSetSampler(1, linearClampSampler);
            context.PSSetSampler(2, linearBorderSampler);

            context.SetGraphicsPipeline(pipelineSSR);
            context.DrawInstanced(4, 1, 0, 0);

            context.ClearState();
        }

        public override void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport)
        {
            Output = view;
            Viewport = viewport;
        }

        public override void SetInput(IShaderResourceView view, ITexture2D resource)
        {
            Input = view;
        }

        protected override void DisposeCore()
        {
            pipelineSSR.Dispose();
            pointClampSampler.Dispose();
            linearClampSampler.Dispose();
            linearBorderSampler.Dispose();
        }
    }
}