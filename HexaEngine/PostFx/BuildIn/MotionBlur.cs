namespace HexaEngine.Effects.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Resources;
    using HexaEngine.Mathematics;
    using HexaEngine.PostFx;
    using HexaEngine.Rendering.Graph;
    using System.Threading.Tasks;

    public class MotionBlur : PostFxBase
    {
        private IGraphicsPipeline pipeline;
        private ISamplerState sampler;

        private ResourceRef<Texture2D> Velocity;

        public IRenderTargetView Output;
        public IShaderResourceView Input;
        public Viewport Viewport;

        public override string Name => "MotionBlur";

        public override PostFxFlags Flags { get; }

        public override async Task Initialize(IGraphicsDevice device, PostFxDependencyBuilder builder, int width, int height, ShaderMacro[] macros)
        {
            builder
                .AddBinding("VelocityBuffer")
                .RunBefore("Compose")
                .RunAfter("TAA")
                .RunAfter("HBAO")
                .RunBefore("DepthOfField")
                .RunBefore("GodRays")
                .RunBefore("VolumetricClouds")
                .RunBefore("SSR")
                .RunBefore("SSGI")
                .RunBefore("LensFlare")
                .RunBefore("Bloom")
                .RunBefore("AutoExposure");

            pipeline = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/motionblur/ps.hlsl"
            }, GraphicsPipelineState.DefaultFullscreen, macros);
            sampler = device.CreateSamplerState(SamplerStateDescription.LinearWrap);

            Velocity = ResourceManager2.Shared.GetTexture("VelocityBuffer");

            Viewport = new(width, height);
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

        public override unsafe void Draw(IGraphicsContext context, GraphResourceBuilder creator)
        {
            if (Output == null)
            {
                return;
            }

            nint* srvs = stackalloc nint[2];
            srvs[0] = Input.NativePointer;
            srvs[1] = Velocity.Value.SRV.NativePointer;
            context.ClearRenderTargetView(Output, default);
            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);

            context.PSSetShaderResources(0, 2, (void**)srvs);
            context.PSSetSampler(0, sampler);

            context.SetGraphicsPipeline(pipeline);

            context.DrawInstanced(4, 1, 0, 0);

            context.SetGraphicsPipeline(null);

            context.PSSetSampler(0, null);
            ZeroMemory(srvs, sizeof(nint) * 2);
            context.PSSetShaderResources(0, 2, (void**)srvs);

            context.SetRenderTarget(null, null);
        }

        protected override void DisposeCore()
        {
            pipeline.Dispose();
            sampler.Dispose();
        }
    }
}