namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.PostFx;
    using System;
    using System.Numerics;

    public class VelocityBuffer : PostFxBase
    {
        private IGraphicsPipeline pipeline;
        private ConstantBuffer<VelocityBufferParams> paramsBuffer;

        private ISamplerState sampler;

        private ResourceRef<Texture2D> Velocity;

        private float scale = 64;
        private ResourceRef<DepthStencil> depth;
        private ResourceRef<ConstantBuffer<CBCamera>> camera;
        private GraphResourceBuilder creator;

        public override string Name => "VelocityBuffer";

        public override PostFxFlags Flags => PostFxFlags.NoOutput | PostFxFlags.NoInput | PostFxFlags.Optional;

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

        public override void Initialize(IGraphicsDevice device, GraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            this.creator = creator;

            depth = creator.GetDepthStencilBuffer("#DepthStencil");
            camera = creator.GetConstantBuffer<CBCamera>("CBCamera");

            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/velocity/ps.hlsl",
                State = GraphicsPipelineState.DefaultFullscreen,
                Macros = macros
            });

            paramsBuffer = new(device, CpuAccessFlags.Write);

            sampler = device.CreateSamplerState(SamplerStateDescription.LinearWrap);

            Velocity = creator.CreateTexture2D("VelocityBuffer", new(Format.R32G32Float, width, height, 1, 1, GpuAccessFlags.RW), false);
        }

        public override void Update(IGraphicsContext context)
        {
            if (dirty)
            {
                paramsBuffer.Update(context, new(scale));
                dirty = false;
            }
        }

        public override void Draw(IGraphicsContext context)
        {
            context.SetRenderTarget(Velocity.Value?.RTV, null);
            context.SetViewport(Viewport);
            context.PSSetConstantBuffer(0, paramsBuffer);
            context.PSSetConstantBuffer(1, camera.Value);
            context.PSSetSampler(0, sampler);
            context.PSSetShaderResource(0, depth.Value.SRV);
            context.SetGraphicsPipeline(pipeline);
            context.DrawInstanced(4, 1, 0, 0);
            context.ClearState();
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