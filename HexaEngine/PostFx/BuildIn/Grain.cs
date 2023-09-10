namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Rendering.Graph;
    using System.Numerics;

    public class Grain : PostFxBase
    {
        private IGraphicsPipeline pipeline;
        private ConstantBuffer<GrainParams> paramsBuffer;

        private ISamplerState samplerState;

        private Vector3 color = Vector3.One;
        private float intensity = 0.05f;

        private struct GrainParams
        {
            public Vector3 Color;
            public float Time;
            public float Intensity;
            public Vector3 Padd;

            public GrainParams(Vector3 color, float time, float intensity)
            {
                Color = color;
                Time = time;
                Intensity = intensity;
                Padd = default;
            }
        }

        public override string Name { get; } = "Grain";

        public override PostFxFlags Flags { get; } = PostFxFlags.None;

        public Vector3 Color
        {
            get => color;
            set => NotifyPropertyChangedAndSet(ref color, value);
        }

        public float Intensity
        {
            get => intensity;
            set => NotifyPropertyChangedAndSet(ref intensity, value);
        }

        public override void Initialize(IGraphicsDevice device, PostFxDependencyBuilder builder, GraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            builder
                .RunAfter("ColorGrading")
                .RunAfter("UserLUT")
                .RunBefore("FXAA");

            paramsBuffer = new(device, CpuAccessFlags.Write);
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/grain/ps.hlsl"
            }, GraphicsPipelineState.DefaultFullscreen, macros);

            samplerState = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
        }

        public override unsafe void Update(IGraphicsContext context)
        {
            if (dirty)
            {
                paramsBuffer.Update(context, new(color, Time.CumulativeFrameTime, intensity));
                dirty = false;
            }
            else
            {
                paramsBuffer.Local->Time = Time.CumulativeFrameTime;
                paramsBuffer.Update(context);
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
            pipeline.Dispose();
            paramsBuffer.Dispose();
            samplerState.Dispose();
        }
    }
}