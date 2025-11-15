namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics.Graph;

    /// <summary>
    /// Post-processing effect for adding grain to the scene.
    /// </summary>
    [EditorDisplayName("Grain")]
    public class Grain : PostFxBase
    {
#nullable disable
        private IGraphicsPipelineState pipeline;
        private ConstantBuffer<GrainParams> paramsBuffer;
        private float grainIntensity = 0.05f;
        private float grainSize = 1.6f;
        private bool grainColored = false;
        private float grainColorAmount = 0.6f;
        private float grainLumaAmount = 1;

        public float GrainIntensity { get => grainIntensity; set => NotifyPropertyChangedAndSet(ref grainIntensity, value); }

        public float GrainSize { get => grainSize; set => NotifyPropertyChangedAndSet(ref grainSize, value); }

        public bool GrainColored { get => grainColored; set => NotifyPropertyChangedAndSet(ref grainColored, value); }

        public float GrainColorAmount { get => grainColorAmount; set => NotifyPropertyChangedAndSet(ref grainColorAmount, value); }

        public float GrainLumaAmount { get => grainLumaAmount; set => NotifyPropertyChangedAndSet(ref grainLumaAmount, value); }

#nullable restore

        private struct GrainParams
        {
            public float Width;
            public float Height;
            public float Time;
            public float GrainIntensity;

            public float GrainSize;
            public int GrainColored;
            public float GrainColorAmount;
            public float GrainLumaAmount;

            public GrainParams(float width, float height, float time, float grainIntensity, float grainSize, bool grainColored, float grainColorAmount, float grainLumaAmount)
            {
                Width = width;
                Height = height;
                Time = time;
                GrainIntensity = grainIntensity;
                GrainSize = grainSize;
                GrainColored = grainColored ? 1 : 0;
                GrainColorAmount = grainColorAmount;
                GrainLumaAmount = grainLumaAmount;
            }
        }

        /// <inheritdoc/>
        public override string Name { get; } = "Grain";

        /// <inheritdoc/>
        public override PostFxFlags Flags { get; } = PostFxFlags.None;

        /// <inheritdoc/>
        public override PostFxColorSpace ColorSpace { get; } = PostFxColorSpace.SDR;

        /// <inheritdoc/>
        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
            builder
                .RunAfter<ColorGrading>()
                .RunAfter<UserLUT>()
                .RunBefore<FXAA>();
        }

        /// <inheritdoc/>
        public override void Initialize(IGraphicsDevice device, PostFxGraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            paramsBuffer = new(CpuAccessFlags.Write);
            pipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "HexaEngine.Core:shaders/quad.hlsl",
                PixelShader = "HexaEngine.Core:shaders/effects/grain/ps.hlsl",
                Macros = macros
            }, GraphicsPipelineStateDesc.DefaultFullscreen);
        }

        public override void Update(IGraphicsContext context)
        {
            paramsBuffer.Update(context, new(Viewport.Width, Viewport.Height, Time.CumulativeFrameTime, grainIntensity, grainSize, grainColored, grainColorAmount, grainLumaAmount));
        }

        public override void UpdateBindings()
        {
            pipeline.Bindings.SetSRV("hdrTexture", Input);
            pipeline.Bindings.SetCBV("Params", paramsBuffer);
        }

        /// <inheritdoc/>
        public override void Draw(IGraphicsContext context)
        {
            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);
            context.SetGraphicsPipelineState(pipeline);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipelineState(null);
            context.SetViewport(default);
            context.SetRenderTarget(null, null);
        }

        /// <inheritdoc/>
        protected override void DisposeCore()
        {
            paramsBuffer.Dispose();
        }
    }
}