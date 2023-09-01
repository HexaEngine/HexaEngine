namespace HexaEngine.Effects.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Effects.Blur;
    using HexaEngine.Graph;
    using HexaEngine.Mathematics;
    using HexaEngine.PostFx;
    using HexaEngine.Rendering.Graph;
    using HexaEngine.Scenes;

    public class SSGI : PostFxBase
    {
        private IGraphicsDevice device;
        private IGraphicsPipeline pipelineSSGI;
        private ConstantBuffer<SSGIParams> ssgiParams;
        private Texture2D inputChain;
        private Texture2D outputBuffer;
        private GaussianBlur blur;
        private ISamplerState linearWrapSampler;
        private IGraphicsPipeline copy;

        public IRenderTargetView Output;
        public Viewport Viewport;
        public IShaderResourceView Input;
        public ITexture2D InputTex;
        private ResourceRef<DepthMipChain> depth;
        private ResourceRef<ConstantBuffer<CBCamera>> camera;
        private ResourceRef<GBuffer> gbuffer;

        public override string Name { get; } = "SSGI";

        public override PostFxFlags Flags { get; } = PostFxFlags.Inline;

        #region Structs

        private struct SSGIParams
        {
            public float Intensity;
            public float Distance;
            public float NoiseAmount;
            public int Noise;

            public SSGIParams()
            {
                Intensity = 1f;
                Distance = 10;
                NoiseAmount = .4f;
                Noise = 1;
            }
        }

        #endregion Structs

        public override void Initialize(IGraphicsDevice device, PostFxDependencyBuilder builder, GraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            builder
                .RunBefore("Compose")
                .RunAfter("TAA")
                .RunAfter("HBAO")
                .RunAfter("MotionBlur")
                .RunAfter("DepthOfField")
                .RunAfter("GodRays")
                .RunAfter("VolumetricClouds")
                .RunAfter("SSR")
                .RunBefore("LensFlare")
                .RunBefore("Bloom")
                .RunBefore("AutoExposure");

            depth = creator.GetDepthMipChain("HiZBuffer");
            camera = creator.GetConstantBuffer<CBCamera>("CBCamera");
            gbuffer = creator.GetGBuffer("GBuffer");

            this.device = device;

            linearWrapSampler = device.CreateSamplerState(SamplerStateDescription.LinearWrap);

            pipelineSSGI = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/ssgi/ps.hlsl",
            },
            new GraphicsPipelineState()
            {
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleStrip
            },
            macros);

            copy = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/copy/ps.hlsl",
            });

            ssgiParams = new(device, new SSGIParams(), CpuAccessFlags.Write);
            inputChain = new(device, Format.R16G16B16A16Float, width, height, 1, 10, CpuAccessFlags.None, GpuAccessFlags.RW, ResourceMiscFlag.GenerateMips);
            outputBuffer = new(device, Format.R16G16B16A16Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            blur = new(device, Format.R16G16B16A16Float, width, height);
        }

        public override void Draw(IGraphicsContext context, GraphResourceBuilder creator)
        {
            if (Output == null)
                return;

            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);
            context.PSSetShaderResource(0, Input);
            context.PSSetShaderResource(1, depth.Value.SRV);
            context.PSSetShaderResource(2, gbuffer.Value.SRVs[1]);
            context.PSSetConstantBuffer(0, ssgiParams);
            context.PSSetConstantBuffer(1, camera.Value);
            context.PSSetSampler(0, linearWrapSampler);

            context.SetGraphicsPipeline(pipelineSSGI);

            context.DrawInstanced(4, 1, 0, 0);

            context.SetGraphicsPipeline(null);

            context.PSSetSampler(0, null);
            context.PSSetConstantBuffer(1, null);
            context.PSSetConstantBuffer(0, null);
            context.PSSetShaderResource(1, null);
            context.PSSetShaderResource(0, null);
            context.SetRenderTarget(null, null);
        }

        public override void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport)
        {
            Output = view;
            Viewport = viewport;
        }

        public override void SetInput(IShaderResourceView view, ITexture2D resource)
        {
            Input = view;
            InputTex = resource;
        }

        protected override void DisposeCore()
        {
            pipelineSSGI.Dispose();
            linearWrapSampler.Dispose();
            ssgiParams.Dispose();
        }
    }
}