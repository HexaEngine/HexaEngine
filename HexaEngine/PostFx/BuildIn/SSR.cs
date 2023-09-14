namespace HexaEngine.Effects.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graph;
    using HexaEngine.Meshes;
    using HexaEngine.PostFx;
    using HexaEngine.Rendering.Graph;

    public class SSR : PostFxBase
    {
        private IGraphicsPipeline pipelineSSR;

        private ISamplerState pointClampSampler;
        private ISamplerState linearClampSampler;
        private ISamplerState linearBorderSampler;

        private ResourceRef<DepthStencil> depth;
        private ResourceRef<ConstantBuffer<CBCamera>> camera;
        private ResourceRef<GBuffer> gbuffer;

        public override string Name { get; } = "SSR";

        public override PostFxFlags Flags { get; }

        public override void Initialize(IGraphicsDevice device, PostFxDependencyBuilder builder, GraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            builder
                .RunBefore("ColorGrading")
                .RunAfter("HBAO")
                .RunAfter("SSGI")
                .RunBefore("MotionBlur")
                .RunBefore("AutoExposure")
                .RunBefore("TAA")
                .RunBefore("DepthOfField")
                .RunBefore("ChromaticAberration")
                .RunBefore("Bloom");

            depth = creator.GetDepthStencilBuffer("#DepthStencil");
            camera = creator.GetConstantBuffer<CBCamera>("CBCamera");
            gbuffer = creator.GetGBuffer("GBuffer");

            pointClampSampler = device.CreateSamplerState(SamplerStateDescription.PointClamp);
            linearClampSampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
            linearBorderSampler = device.CreateSamplerState(SamplerStateDescription.LinearBorder);

            pipelineSSR = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/ssr/ps.hlsl",
            }, GraphicsPipelineState.DefaultFullscreen, macros);
        }

        public override void Draw(IGraphicsContext context, GraphResourceBuilder creator)
        {
            if (Output == null)
                return;

            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);
            context.PSSetShaderResource(0, depth.Value.SRV);
            context.PSSetShaderResource(1, gbuffer.Value.SRVs[1]);
            context.PSSetShaderResource(2, Input);
            context.PSSetShaderResource(3, gbuffer.Value.SRVs[2]);
            context.PSSetConstantBuffer(1, camera.Value);
            context.PSSetSampler(0, pointClampSampler);
            context.PSSetSampler(1, linearClampSampler);
            context.PSSetSampler(2, linearBorderSampler);

            context.SetGraphicsPipeline(pipelineSSR);
            context.DrawInstanced(4, 1, 0, 0);

            context.ClearState();
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