namespace HexaEngine.Effects.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graph;
    using HexaEngine.Graphics.Effects.Blur;
    using HexaEngine.Meshes;
    using HexaEngine.PostFx;
    using HexaEngine.Rendering.Graph;

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

        private ResourceRef<DepthMipChain> depth;
        private ResourceRef<ConstantBuffer<CBCamera>> camera;
        private ResourceRef<GBuffer> gbuffer;
        private float intensity;
        private float distance;
        private float noiseAmount;

        public override string Name { get; } = "SSGI";

        public override PostFxFlags Flags { get; } = PostFxFlags.Inline;

        public float Intensity
        {
            get => intensity;
            set => NotifyPropertyChangedAndSet(ref intensity, value);
        }

        public float Distance
        {
            get => distance;
            set => NotifyPropertyChangedAndSet(ref distance, value);
        }

        public float NoiseAmount
        {
            get => noiseAmount;
            set => NotifyPropertyChangedAndSet(ref noiseAmount, value);
        }

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
                .RunBefore("ColorGrading")
                .RunAfter("HBAO")
                .RunBefore("SSR")
                .RunBefore("MotionBlur")
                .RunBefore("AutoExposure")
                .RunBefore("TAA")
                .RunBefore("DepthOfField")
                .RunBefore("ChromaticAberration")
                .RunBefore("Bloom");

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

        public override void Update(IGraphicsContext context)
        {
            if (dirty)
            {
                SSGIParams ssgiParams = new();
                ssgiParams.Intensity = intensity;
                ssgiParams.Distance = distance;
                ssgiParams.NoiseAmount = noiseAmount;
                this.ssgiParams.Update(context, ssgiParams);
                dirty = false;
            }
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

        protected override void DisposeCore()
        {
            pipelineSSGI.Dispose();
            copy.Dispose();
            linearWrapSampler.Dispose();
            ssgiParams.Dispose();
            inputChain.Dispose();
            outputBuffer.Dispose();
            blur.Dispose();
        }
    }
}