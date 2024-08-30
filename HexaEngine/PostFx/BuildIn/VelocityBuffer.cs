namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.PostFx;
    using Silk.NET.OpenAL;
    using System.Numerics;

    [EditorDisplayName("Velocity Buffer")]
    public class VelocityBuffer : PostFxBase
    {
#nullable disable
        private IGraphicsPipelineState pipeline;
        private ConstantBuffer<VelocityBufferParams> paramsBuffer;

        private ISamplerState sampler;

        private ResourceRef<Texture2D> Velocity;

        private float scale = 64;
        private ResourceRef<DepthStencil> depth;
        private ResourceRef<ConstantBuffer<CBCamera>> camera;
        private PostFxGraphResourceBuilder creator;
#nullable restore

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

            depth = creator.GetDepthStencilBuffer("#DepthStencil");
            camera = creator.GetConstantBuffer<CBCamera>("CBCamera");

            pipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/velocity/ps.hlsl",
                Macros = macros
            }, GraphicsPipelineStateDesc.DefaultFullscreen);

            paramsBuffer = new(CpuAccessFlags.Write);

            sampler = device.CreateSamplerState(SamplerStateDescription.LinearWrap);

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
            pipeline.Bindings.SetSRV("depthTex", depth.Value!.SRV);
            pipeline.Bindings.SetCBV("VelocityBufferParams", paramsBuffer);
            pipeline.Bindings.SetCBV("CameraBuffer", camera.Value);
            pipeline.Bindings.SetSampler("linearWrapSampler", sampler);
        }

        public override void Draw(IGraphicsContext context)
        {
            context.SetRenderTarget(Velocity.Value?.RTV, null);
            context.SetViewport(Viewport);
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
            sampler.Dispose();
            creator.DisposeResource("VelocityBuffer");
        }
    }
}