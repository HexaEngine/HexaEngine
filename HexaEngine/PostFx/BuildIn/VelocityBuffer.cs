namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.PostFx;
    using System.Numerics;

    [EditorDisplayName("Velocity Buffer")]
    public class VelocityBuffer : PostFxBase
    {
#nullable disable
        private IGraphicsPipelineState pipeline;
        private ConstantBuffer<VelocityBufferParams> paramsBuffer;
        private ResourceRef<Texture2D> Velocity;
        private PostFxGraphResourceBuilder creator;
#nullable restore

        private float scale = 64;

        public override string Name => "VelocityBuffer";

        public override PostFxFlags Flags => PostFxFlags.NoOutput | PostFxFlags.NoInput | PostFxFlags.Optional;

        /// <inheritdoc/>
        public override PostFxColorSpace ColorSpace { get; } = PostFxColorSpace.None;

        public float Scale
        {
            get => scale;
            set => NotifyPropertyChangedAndSet(ref scale, value);
        }

        #region Structs

        private struct VelocityBufferParams
        {
            public float Scale;
            public Vector3 Padding;

            public VelocityBufferParams(float scale)
            {
                Scale = scale;
                Padding = default;
            }
        }

        #endregion Structs

        /// <inheritdoc/>
        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
            builder.AddSource("VelocityBuffer");
        }

        public override void Initialize(IGraphicsDevice device, PostFxGraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            this.creator = creator;

            pipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "HexaEngine.PostFx:shaders/quad.hlsl",
                PixelShader = "HexaEngine.PostFx:shaders/effects/velocity/ps.hlsl",
                Macros = macros
            }, GraphicsPipelineStateDesc.DefaultFullscreen);

            paramsBuffer = new(CpuAccessFlags.Write);
            Velocity = creator.CreateTexture2D("VelocityBuffer", new(Format.R32G32Float, width, height, 1, 1, GpuAccessFlags.RW), ResourceCreationFlags.None);
        }

        public override void Update(IGraphicsContext context)
        {
            if (dirty)
            {
                paramsBuffer.Update(context, new(scale));
                dirty = false;
            }
        }

        public override void UpdateBindings()
        {
            creator.Device.SetGlobalSRV("velocityBufferTex", Velocity.Value);
            pipeline.Bindings.SetCBV("VelocityBufferParams", paramsBuffer);
        }

        public override void Draw(IGraphicsContext context)
        {
            context.SetRenderTarget(Velocity.Value!.RTV, null);
            context.SetViewport(Velocity.Value!.Viewport);
            context.SetGraphicsPipelineState(pipeline);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipelineState(null);
            context.SetRenderTarget(null, null);
        }

        public override void Resize(int width, int height)
        {
            creator.UpdateTexture2D("VelocityBuffer", new Texture2DDescription(Format.R32G32Float, width, height, 1, 1, GpuAccessFlags.RW));
            Viewport = new(width, height);
        }

        protected override void DisposeCore()
        {
            pipeline.Dispose();
            paramsBuffer.Dispose();
            creator.DisposeResource("VelocityBuffer");
        }
    }
}