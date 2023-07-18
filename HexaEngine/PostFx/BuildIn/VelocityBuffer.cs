namespace HexaEngine.Effects.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Resources;
    using HexaEngine.Mathematics;
    using HexaEngine.PostFx;
    using HexaEngine.Rendering.Graph;
    using HexaEngine.Scenes;
    using System;
    using System.Numerics;
    using System.Threading.Tasks;

    public class VelocityBuffer : IPostFx
    {
        private IGraphicsPipeline pipeline;
        private ConstantBuffer<VelocityBufferParams> paramsBuffer;

        private ISamplerState sampler;

        private ResourceRef<Texture2D> Velocity;

        private Viewport Viewport;
        private float scale = 64;

        private bool enabled = true;
        private int priority = 1000;

        public event Action<bool>? OnEnabledChanged;

        public event Action<int>? OnPriorityChanged;

        public string Name => "VelocityBuffer";

        public PostFxFlags Flags => PostFxFlags.NoOutput | PostFxFlags.NoInput;

        public bool Enabled
        {
            get => enabled; set
            {
                enabled = value;
                OnEnabledChanged?.Invoke(value);
            }
        }

        public int Priority
        {
            get => priority; set
            {
                priority = value;
                OnPriorityChanged?.Invoke(value);
            }
        }

        public float Scale { get => scale; set => scale = value; }

        #region Structs

        public struct VelocityBufferParams
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

        public async Task Initialize(IGraphicsDevice device, PostFxDependencyBuilder builder, int width, int height, ShaderMacro[] macros)
        {
            builder.AddSource("VelocityBuffer");

            pipeline = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/velocity/ps.hlsl",
            }, GraphicsPipelineState.DefaultFullscreen, macros);

            paramsBuffer = new(device, CpuAccessFlags.Write);

            sampler = device.CreateSamplerState(SamplerStateDescription.LinearWrap);

            Velocity = ResourceManager2.Shared.AddTexture("VelocityBuffer", new Texture2DDescription(Format.R32G32Float, width, height, 1, 1, BindFlags.ShaderResource | BindFlags.RenderTarget));

            Viewport = new(width, height);
        }

        public void Update(IGraphicsContext context)
        {
            paramsBuffer.Update(context, new(scale));
        }

        public void Draw(IGraphicsContext context, GraphResourceBuilder creator)
        {
            var depth = creator.GetDepthStencilBuffer("#DepthStencil");
            var camera = creator.GetConstantBuffer<CBCamera>("CBCamera");

            context.SetRenderTarget(Velocity.Value?.RTV, null);
            context.SetViewport(Viewport);
            context.PSSetConstantBuffer(0, paramsBuffer);
            context.PSSetConstantBuffer(1, camera);
            context.PSSetSampler(0, sampler);
            context.PSSetShaderResource(0, depth.SRV);
            context.SetGraphicsPipeline(pipeline);
            context.DrawInstanced(4, 1, 0, 0);
            context.ClearState();
        }

        public void Resize(int width, int height)
        {
            Velocity = ResourceManager2.Shared.UpdateTexture("VelocityBuffer", new Texture2DDescription(Format.R32G32Float, width, height, 1, 1, BindFlags.ShaderResource | BindFlags.RenderTarget));
            Viewport = new(width, height);
        }

        public void SetInput(IShaderResourceView view, ITexture2D resource)
        {
        }

        public void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport)
        {
        }

        public void Dispose()
        {
            pipeline.Dispose();
            paramsBuffer.Dispose();
            sampler.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}