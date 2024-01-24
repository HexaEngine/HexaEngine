namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using System.Numerics;

    /// <summary>
    /// Post-processing effect for generating temporal noise.
    /// </summary>
    public class TemporalNoise : PostFxBase
    {
#nullable disable
        private IGraphicsPipeline pipeline;
        private ConstantBuffer<TemporalNoiseParams> paramsBuffer;

        private GraphResourceBuilder creator;
        private ResourceRef<Texture2D> Noise;
        private ResourceRef<ConstantBuffer<CBCamera>> camera;

#nullable restore

        private float scale = 0.1f;

        /// <inheritdoc/>
        public override string Name => "TemporalNoise";

        /// <inheritdoc/>
        public override PostFxFlags Flags => PostFxFlags.NoOutput | PostFxFlags.NoInput | PostFxFlags.Optional;

        /// <summary>
        /// Gets or sets the scale of the temporal noise.
        /// </summary>
        public float Scale
        {
            get => scale;
            set => NotifyPropertyChangedAndSet(ref scale, value);
        }

        #region Structs

        private struct TemporalNoiseParams
        {
            public float Scale;
            public Vector3 Padding;

            public TemporalNoiseParams(float scale)
            {
                Scale = scale;
                Padding = default;
            }
        }

        #endregion Structs

        /// <inheritdoc/>
        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
            builder.AddSource("TemporalNoise");
        }

        /// <inheritdoc/>
        public override void Initialize(IGraphicsDevice device, GraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            this.creator = creator;

            camera = creator.GetConstantBuffer<CBCamera>("CBCamera");

            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/noise/temporal.hlsl",
                State = GraphicsPipelineState.DefaultFullscreen,
                Macros = macros
            });
            paramsBuffer = new(device, CpuAccessFlags.Write);

            Noise = creator.CreateTexture2D("TemporalNoise", new(Format.R32Float, width, height, 1, 1, BindFlags.ShaderResource | BindFlags.RenderTarget));
        }

        /// <inheritdoc/>
        public override void Update(IGraphicsContext context)
        {
            if (dirty)
            {
                paramsBuffer.Update(context, new(scale));
                dirty = false;
            }
        }

        /// <inheritdoc/>
        public override void Draw(IGraphicsContext context)
        {
            context.SetRenderTarget(Noise.Value?.RTV, null);
            context.SetViewport(Viewport);
            context.PSSetConstantBuffer(0, paramsBuffer);
            context.PSSetConstantBuffer(1, camera.Value);
            context.SetGraphicsPipeline(pipeline);
            context.DrawInstanced(4, 1, 0, 0);
            context.ClearState();
        }

        /// <inheritdoc/>
        protected override void DisposeCore()
        {
            pipeline.Dispose();
            paramsBuffer.Dispose();
            creator.DisposeResource("TemporalNoise");
        }
    }
}