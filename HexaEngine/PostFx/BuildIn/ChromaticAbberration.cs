namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Rendering.Graph;
    using System.Numerics;

    public class ChromaticAberration : PostFxBase
    {
        private IGraphicsPipeline pipeline;
        private ConstantBuffer<ChromaticAberrationParams> paramsBuffer;
        private ISamplerState samplerState;

        private float intensity = 1;

        private struct ChromaticAberrationParams
        {
            public float ChromaticAberrationIntensity;
            public Vector3 Padd;

            public ChromaticAberrationParams(float chromaticAberrationIntensity)
            {
                ChromaticAberrationIntensity = chromaticAberrationIntensity;
                Padd = default;
            }
        }

        public override string Name { get; } = "ChromaticAberration";

        public override PostFxFlags Flags { get; } = PostFxFlags.None;

        public float Intensity
        {
            get => intensity;
            set => NotifyPropertyChangedAndSet(ref intensity, value);
        }

        public override void Initialize(IGraphicsDevice device, PostFxDependencyBuilder builder, GraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            builder
                .RunBefore("ColorGrading")
                .RunAfter("HBAO")
                .RunAfter("SSGI")
                .RunAfter("SSR")
                .RunAfter("MotionBlur")
                .RunAfter("AutoExposure")
                .RunAfter("TAA")
                .RunAfter("DepthOfField")
                .RunBefore("Bloom");

            paramsBuffer = new(device, CpuAccessFlags.Write);
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/chromaticaberration/ps.hlsl",
            }, GraphicsPipelineState.DefaultFullscreen);

            samplerState = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
        }

        public override void Update(IGraphicsContext context)
        {
            if (dirty)
            {
                paramsBuffer.Update(context, new(intensity));
                dirty = false;
            }
        }

        public override void Draw(IGraphicsContext context, GraphResourceBuilder creator)
        {
            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);
            context.PSSetShaderResource(0, Input);
            context.PSSetConstantBuffer(0, paramsBuffer);
            context.PSSetSampler(0, samplerState);
            context.SetGraphicsPipeline(pipeline);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipeline(null);
            context.PSSetSampler(0, null);
            context.PSSetConstantBuffer(0, null);
            context.PSSetShaderResource(0, null);
            context.SetViewport(default);
            context.SetRenderTarget(null, null);
        }

        protected override void DisposeCore()
        {
            paramsBuffer.Dispose();
            pipeline.Dispose();
            samplerState.Dispose();
        }
    }
}