namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics.Graph;
    using System.Numerics;

    /// <summary>
    /// Post-processing effect for adding grain to the scene.
    /// </summary>
    public class Grain : PostFxBase
    {
#nullable disable
        private IGraphicsPipeline pipeline;
        private ConstantBuffer<GrainParams> paramsBuffer;
        private ISamplerState samplerState;

        private ResourceRef<Texture2D> noise;
#nullable restore

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

        /// <inheritdoc/>
        public override string Name { get; } = "Grain";

        /// <inheritdoc/>
        public override PostFxFlags Flags { get; } = PostFxFlags.None;

        /// <summary>
        /// Gets or sets the color of the grain.
        /// </summary>
        public Vector3 Color
        {
            get => color;
            set => NotifyPropertyChangedAndSet(ref color, value);
        }

        /// <summary>
        /// Gets or sets the intensity of the grain effect.
        /// </summary>
        public float Intensity
        {
            get => intensity;
            set => NotifyPropertyChangedAndSet(ref intensity, value);
        }

        /// <inheritdoc/>
        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
            builder
                .AddBinding("TemporalNoise")
                .RunAfter("ColorGrading")
                .RunAfter("UserLUT")
                .RunBefore("FXAA");
        }

        /// <inheritdoc/>
        public override void Initialize(IGraphicsDevice device, GraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            paramsBuffer = new(device, CpuAccessFlags.Write);
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/grain/ps.hlsl",
                State = GraphicsPipelineState.DefaultFullscreen,
                Macros = macros
            });

            noise = creator.GetTexture2D("TemporalNoise");

            samplerState = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override void Draw(IGraphicsContext context, GraphResourceBuilder creator)
        {
            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);
            context.PSSetShaderResource(0, Input);
            context.PSSetShaderResource(1, noise.Value);
            context.PSSetConstantBuffer(0, paramsBuffer);
            context.PSSetSampler(0, samplerState);
            context.SetGraphicsPipeline(pipeline);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipeline(null);
            context.PSSetSampler(0, null);
            context.PSSetConstantBuffer(0, null);
            context.PSSetShaderResource(0, null);
            context.PSSetShaderResource(1, null);
            context.SetViewport(default);
            context.SetRenderTarget(null, null);
        }

        /// <inheritdoc/>
        protected override void DisposeCore()
        {
            pipeline.Dispose();
            paramsBuffer.Dispose();
            samplerState.Dispose();
        }
    }
}