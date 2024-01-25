#nullable disable

namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics.Effects.Blur;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.PostFx;
    using System.Numerics;

    public class SSAO : PostFxBase
    {
        private IGraphicsDevice device;
        private IGraphicsPipeline pipeline;
        private ConstantBuffer<SSAOParams> paramsBuffer;
        private Texture2D noiseTex;
        private Texture2D intermediateTex;
        private GaussianBlur blur;

        private ISamplerState samplerLinear;

        private ResourceRef<Texture2D> ao;
        private ResourceRef<DepthStencil> depth;
        private ResourceRef<GBuffer> gbuffer;
        private ResourceRef<ConstantBuffer<CBCamera>> camera;

        private Viewport viewport;

        private float tapSize = 0.0002f;
        private uint numTaps = 16;
        private float power = 1.0f;

        private const int NoiseSize = 4;
        private const int NoiseStride = 4;

        #region Structs

        private struct SSAOParams
        {
            public float TapSize;
            public uint NumTaps;
            public float Power;
            public Vector2 NoiseScale;
            public Vector3 padd;

            public SSAOParams()
            {
                TapSize = 0.0002f;
                NumTaps = 16;
                Power = 1.0f;
                padd = default;
            }
        }

        #endregion Structs

        #region Properties

        public override string Name => "SSAO";

        public override PostFxFlags Flags { get; } = PostFxFlags.NoOutput | PostFxFlags.NoInput;

        /// <inheritdoc/>
        public override PostFxColorSpace ColorSpace { get; } = PostFxColorSpace.None;

        public float TapSize
        {
            get => tapSize;
            set => tapSize = value;
        }

        public uint NumTaps
        {
            get => numTaps;
            set => numTaps = value;
        }

        public float Power
        {
            get => power;
            set => power = value;
        }

        #endregion Properties

        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
        }

        public override void Initialize(IGraphicsDevice device, PostFxGraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            this.device = device;

            ao = creator.GetTexture2D("#AOBuffer");
            depth = creator.GetDepthStencilBuffer("#DepthStencil");
            camera = creator.GetConstantBuffer<CBCamera>("CBCamera");
            gbuffer = creator.GetGBuffer("GBuffer");

            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/ssao/ps.hlsl",
                State = GraphicsPipelineState.DefaultFullscreen,
            });
            paramsBuffer = new(device, CpuAccessFlags.Write);
            intermediateTex = new(device, Format.R32Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            blur = new(creator, "SSAO", Format.R32Float);

            unsafe
            {
                Texture2DDescription description = new(Format.R32G32B32A32Float, NoiseSize, NoiseSize, 1, 1, BindFlags.ShaderResource, Usage.Immutable);

                float* pixelData = AllocT<float>(NoiseSize * NoiseSize * NoiseStride);

                SubresourceData initialData = default;
                initialData.DataPointer = (nint)pixelData;
                initialData.RowPitch = NoiseSize * NoiseStride;

                int idx = 0;
                for (int i = 0; i < NoiseSize * NoiseSize; i++)
                {
                    pixelData[idx++] = Random.Shared.NextSingle().Map01ToN1P1();
                    pixelData[idx++] = Random.Shared.NextSingle().Map01ToN1P1();
                    pixelData[idx++] = 0.0f.Map01ToN1P1();
                    pixelData[idx++] = 1.0f;
                }

                noiseTex = new(device, description, initialData);
            }

            samplerLinear = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
            viewport = new(width, height);
        }

        public override void Resize(int width, int height)
        {
            intermediateTex.Resize(device, Format.R32Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            viewport = new(width, height);
        }

        public override void Update(IGraphicsContext context)
        {
            if (dirty)
            {
                SSAOParams ssaoParams = default;
                ssaoParams.TapSize = tapSize;
                ssaoParams.NumTaps = numTaps;
                ssaoParams.Power = power;
                ssaoParams.NoiseScale = viewport.Size / NoiseSize;
                paramsBuffer.Update(context, ssaoParams);
                dirty = false;
            }
        }

        public override unsafe void Draw(IGraphicsContext context)
        {
            context.ClearRenderTargetView(ao.Value.RTV, Vector4.One);

            nint* srvs = stackalloc nint[] { depth.Value.SRV.NativePointer, gbuffer.Value.SRVs[1].NativePointer, noiseTex.SRV.NativePointer };
            nint* cbs = stackalloc nint[] { paramsBuffer.NativePointer, camera.Value.NativePointer };
            context.SetRenderTarget(intermediateTex.RTV, null);
            context.SetViewport(viewport);
            context.PSSetShaderResources(0, 3, (void**)srvs);
            context.PSSetConstantBuffers(0, 2, (void**)cbs);
            context.PSSetSampler(0, samplerLinear);
            context.SetGraphicsPipeline(pipeline);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipeline(null);
            ZeroMemory(srvs, sizeof(nint) * 3);
            ZeroMemory(cbs, sizeof(nint) * 2);
            context.PSSetSampler(0, null);
            context.PSSetConstantBuffers(0, 2, (void**)cbs);
            context.PSSetShaderResources(0, 3, (void**)srvs);
            context.SetRenderTarget(null, null);

            blur.Blur(context, intermediateTex.SRV, ao.Value.RTV, (int)viewport.Width, (int)viewport.Height);
        }

        protected override void DisposeCore()
        {
            pipeline.Dispose();
            paramsBuffer.Dispose();
            noiseTex.Dispose();
            intermediateTex.Dispose();
            blur.Dispose();
            samplerLinear.Dispose();
        }
    }
}