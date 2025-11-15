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
    using HexaEngine.Scenes.Managers;
    using System.Numerics;

    [EditorDisplayName("HBAO")]
    public class HBAO : PostFxBase
    {
        private IGraphicsDevice device;
        private IGraphicsPipelineState pipeline;
        private ConstantBuffer<HBAOParams> paramsBuffer;
        private Texture2D noiseTex;
        private Texture2D intermediateTex;
        private GaussianBlur blur;

        private ResourceRef<Texture2D> ao;

        private Viewport viewport;

        private float samplingRadius = 0.02f;
        private uint numSamplingDirections = 8;
        private float samplingStep = 0.004f;
        private uint numSamplingSteps = 4;
        private float power = 1;

        private const int NoiseSize = 4;

        #region Structs

        private struct HBAOParams
        {
            public float SamplingRadius;
            public float SamplingRadiusToScreen;
            public uint NumSamplingDirections;
            public float SamplingStep;

            public uint NumSamplingSteps;
            public float Power;
            public Vector2 NoiseScale;

            public HBAOParams()
            {
                SamplingRadius = 0.02f;
                SamplingRadiusToScreen = 0;
                NumSamplingDirections = 8;
                SamplingStep = 0.004f;
                NumSamplingSteps = 4;
                Power = 1;
            }
        }

        #endregion Structs

        #region Properties

        public override string Name => "HBAO";

        public override PostFxFlags Flags { get; } = PostFxFlags.NoOutput | PostFxFlags.NoInput | PostFxFlags.PrePass;

        /// <inheritdoc/>
        public override PostFxColorSpace ColorSpace { get; } = PostFxColorSpace.None;

        public float SamplingRadius
        {
            get => samplingRadius;
            set => NotifyPropertyChangedAndSet(ref samplingRadius, value);
        }

        public uint NumSamplingDirections
        {
            get => numSamplingDirections;
            set => NotifyPropertyChangedAndSet(ref numSamplingDirections, value);
        }

        public float SamplingStep
        {
            get => samplingStep;
            set => NotifyPropertyChangedAndSet(ref samplingStep, value);
        }

        public uint NumSamplingSteps
        {
            get => numSamplingSteps;
            set => NotifyPropertyChangedAndSet(ref numSamplingSteps, value);
        }

        public float Power
        {
            get => power;
            set => NotifyPropertyChangedAndSet(ref power, value);
        }

        #endregion Properties

        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
            builder.Override<SSAO>();
        }

        public override void Initialize(IGraphicsDevice device, PostFxGraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            ao = creator.GetTexture2D("#AOBuffer");

            this.device = device;

            pipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "HexaEngine.Core:shaders/quad.hlsl",
                PixelShader = "HexaEngine.Core:shaders/effects/hbao/ps.hlsl",
            }, GraphicsPipelineStateDesc.DefaultFullscreen);

            var cameraObj = CameraManager.Current;
            HBAOParams hbaoParams = default;
            hbaoParams.SamplingRadius = samplingRadius;
            hbaoParams.SamplingRadiusToScreen = samplingRadius * 0.5f * viewport.Height / (MathF.Tan(cameraObj.Fov.ToRad() * 0.5f) * 2.0f); ;
            hbaoParams.SamplingStep = samplingStep;
            hbaoParams.NumSamplingSteps = numSamplingSteps;
            hbaoParams.NumSamplingDirections = numSamplingDirections;
            hbaoParams.Power = power;
            hbaoParams.NoiseScale = viewport.Size / NoiseSize;

            paramsBuffer = new(hbaoParams, CpuAccessFlags.Write);
            intermediateTex = new(Format.R32Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            blur = new(creator, "HBAO", Format.R32Float);

            unsafe
            {
                Texture2DDescription description = new(Format.R32G32B32A32Float, NoiseSize, NoiseSize, 1, 1, BindFlags.ShaderResource, Usage.Immutable);

                Vector4* pixelData = stackalloc Vector4[NoiseSize * NoiseSize];

                SubresourceData initialData = default;
                initialData.DataPointer = (nint)pixelData;
                initialData.RowPitch = NoiseSize * sizeof(Vector4);

                for (int i = 0; i < NoiseSize * NoiseSize; i++)
                {
                    float rand = Random.Shared.NextSingle() * float.Pi * 2.0f;
                    pixelData[i] = new Vector4(MathF.Sin(rand), MathF.Cos(rand), Random.Shared.NextSingle(), 1.0f);
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
                var camera = CameraManager.Current;
                HBAOParams hbaoParams = default;
                hbaoParams.SamplingRadius = samplingRadius;
                hbaoParams.SamplingRadiusToScreen = samplingRadius * 0.5f * viewport.Height / (MathF.Tan(camera.Fov.ToRad() * 0.5f) * 2.0f); ;
                hbaoParams.SamplingStep = samplingStep;
                hbaoParams.NumSamplingSteps = numSamplingSteps;
                hbaoParams.NumSamplingDirections = numSamplingDirections;
                hbaoParams.Power = power;
                hbaoParams.NoiseScale = viewport.Size / NoiseSize;

                paramsBuffer.Update(context, hbaoParams);
                dirty = false;
            }
        }

        public override void UpdateBindings()
        {
            pipeline.Bindings.SetSRV("noiseTex", noiseTex);
            pipeline.Bindings.SetCBV("ConfigBuffer", paramsBuffer);
        }

        public override void PrePassDraw(IGraphicsContext context, GraphResourceBuilder creator)
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

        public override unsafe void Draw(IGraphicsContext context)
        {
        }

        protected override unsafe void DisposeCore()
        {
            pipeline.Dispose();
            paramsBuffer.Dispose();
            noiseTex.Dispose();
            intermediateTex.Dispose();
            blur.Dispose();
        }
    }
}