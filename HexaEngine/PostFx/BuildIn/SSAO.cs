#nullable disable

namespace HexaEngine.PostFx.BuildIn
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Effects.Blur;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.PostFx;
    using System.Numerics;

    [EditorDisplayName("SSAO")]
    public class SSAO : PostFxBase
    {
        private IGraphicsDevice device;
        private IGraphicsPipelineState pipeline;
        private ConstantBuffer<SSAOParams> paramsBuffer;
        private Texture2D noiseTex;
        private Texture2D intermediateTex;
        private GaussianBlur blur;

        private ResourceRef<Texture2D> ao;

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

            pipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "HexaEngine.PostFx:shaders/quad.hlsl",
                PixelShader = "HexaEngine.PostFx:shaders/effects/ssao/ps.hlsl",
            }, GraphicsPipelineStateDesc.DefaultFullscreen);
            paramsBuffer = new(CpuAccessFlags.Write);
            intermediateTex = new(Format.R32Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
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

                noiseTex = new(description, initialData);
            }

            viewport = new(width, height);
        }

        public override void Resize(int width, int height)
        {
            intermediateTex.Resize(Format.R32Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
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

        public override void UpdateBindings()
        {
            pipeline.Bindings.SetSRV("noiseTex", noiseTex);
            pipeline.Bindings.SetCBV("SSAOParams", paramsBuffer);
        }

        public override unsafe void Draw(IGraphicsContext context)
        {
            context.ClearRenderTargetView(ao.Value.RTV, Vector4.One);

            context.SetRenderTarget(intermediateTex.RTV, null);
            context.SetViewport(viewport);
            context.SetGraphicsPipelineState(pipeline);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipelineState(null);
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
        }
    }
}