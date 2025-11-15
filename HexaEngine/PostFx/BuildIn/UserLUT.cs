namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.PostFx;
    using System.Numerics;

    [EditorDisplayName("User LUT")]
    public class UserLUT : PostFxBase
    {
#nullable disable
        private IGraphicsPipelineState pipeline;
        private ConstantBuffer<LUTParams> paramsBuffer;
#nullable enable

        private Texture2D? LUT;

        private float amountChroma = 1;
        private float amountLuma = 1;

        private string lutTexPath = string.Empty;

        private struct LUTParams
        {
            public float LUTAmountChroma;
            public float LUTAmountLuma;
            public Vector2 Padd;
        }

        public override string Name { get; } = "UserLUT";

        public override PostFxFlags Flags { get; }

        /// <inheritdoc/>
        public override PostFxColorSpace ColorSpace { get; } = PostFxColorSpace.SDR;

        public unsafe float AmountChroma
        {
            get => amountChroma;
            set => NotifyPropertyChangedAndSet(ref amountChroma, value);
        }

        public unsafe float AmountLuma
        {
            get => amountLuma;
            set => NotifyPropertyChangedAndSet(ref amountLuma, value);
        }

        public string LUTTexPath
        {
            get => lutTexPath;
            set => NotifyPropertyChangedAndSetAndReload(ref lutTexPath, value ?? string.Empty);
        }

        /// <inheritdoc/>
        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
            builder
                .RunAfter<ColorGrading>()
                .RunBefore<Grain>();
        }

        public override void Initialize(IGraphicsDevice device, PostFxGraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            pipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "HexaEngine.Core:shaders/quad.hlsl",
                PixelShader = "HexaEngine.Core:shaders/effects/lut/ps.hlsl",
                Macros = macros
            }, GraphicsPipelineStateDesc.DefaultFullscreen);
            paramsBuffer = new(new LUTParams(), CpuAccessFlags.Write);
            LUT = Texture2D.LoadFromAssets(lutTexPath);
        }

        public override void UpdateBindings()
        {
            pipeline.Bindings.SetSRV("inputTex", Input);
            pipeline.Bindings.SetSRV("lutTex", LUT?.SRV);
            pipeline.Bindings.SetCBV("LUTParams", paramsBuffer);
        }

        public override unsafe void Draw(IGraphicsContext context)
        {
            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);

            context.SetGraphicsPipelineState(pipeline);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipelineState(null);

            context.SetViewport(default);
            context.SetRenderTarget(null, null);
        }

        protected override void DisposeCore()
        {
            pipeline.Dispose();
            paramsBuffer.Dispose();
            LUT?.Dispose();
        }
    }
}