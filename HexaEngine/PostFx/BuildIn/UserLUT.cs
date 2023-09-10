namespace HexaEngine.Effects.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.PostFx;
    using HexaEngine.Rendering.Graph;
    using System.Numerics;

    public class UserLUT : PostFxBase
    {
        private IGraphicsPipeline pipeline;
        private ConstantBuffer<LUTParams> paramsBuffer;
        private ISamplerState samplerState;
        private ISamplerState lutSamplerState;

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

        public override void Initialize(IGraphicsDevice device, PostFxDependencyBuilder builder, GraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            builder
                .RunAfter("ColorGrading")
                .RunBefore("Grain");

            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/lut/ps.hlsl",
            }, GraphicsPipelineState.DefaultFullscreen, macros);
            paramsBuffer = new(device, new LUTParams(), CpuAccessFlags.Write);
            samplerState = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
            lutSamplerState = device.CreateSamplerState(SamplerStateDescription.LinearClamp);

            LUT = Texture2D.LoadFromAssets(device, lutTexPath, false);
        }

        public override unsafe void Draw(IGraphicsContext context, GraphResourceBuilder creator)
        {
            nint* passSRVs = stackalloc nint[] { Input.NativePointer, LUT?.SRV?.NativePointer ?? 0 };
            nint* passSMPs = stackalloc nint[] { samplerState.NativePointer, lutSamplerState.NativePointer };
            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);
            context.PSSetShaderResources(0, 2, (void**)passSRVs);
            context.PSSetConstantBuffer(0, paramsBuffer);
            context.PSSetSamplers(0, 2, (void**)passSMPs);
            context.SetGraphicsPipeline(pipeline);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipeline(null);
            nint* empty = stackalloc nint[] { 0, 0 };
            context.PSSetSamplers(0, 2, (void**)empty);
            context.PSSetConstantBuffer(0, null);
            context.PSSetShaderResources(0, 2, (void**)empty);
            context.SetViewport(default);
            context.SetRenderTarget(null, null);
        }

        protected override void DisposeCore()
        {
            pipeline.Dispose();
            samplerState.Dispose();
            paramsBuffer.Dispose();
            LUT?.Dispose();
        }
    }
}