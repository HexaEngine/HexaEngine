namespace HexaEngine.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.PostFx;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes.Managers;
    using HexaEngine.ImGuiNET;
    using HexaEngine.Mathematics;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Text;
    using System.Threading.Tasks;

    public class VelocityBuffer : IPostFx
    {
        private Quad quad;
        private IGraphicsPipeline pipeline;
        private ConstantBuffer<VelocityBufferParams> paramsBuffer;

        private ISamplerState sampler;

        private ResourceRef<IBuffer> Camera;
        private ResourceRef<IShaderResourceView> Depth;
        private ResourceRef<Texture> Velocity;

        private Viewport Viewport;
        private Matrix4x4 prevViewProj;
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

        public async Task Initialize(IGraphicsDevice device, int width, int height, ShaderMacro[] macros)
        {
            quad = new(device);
            pipeline = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "effects/velocity/vs.hlsl",
                PixelShader = "effects/velocity/ps.hlsl",
            }, macros);

            paramsBuffer = new(device, CpuAccessFlags.Write);

            sampler = device.CreateSamplerState(SamplerDescription.LinearWrap);

            Camera = ResourceManager2.Shared.GetBuffer("CBCamera");
            Depth = ResourceManager2.Shared.GetResource<IShaderResourceView>("GBuffer.Depth");
            Velocity = ResourceManager2.Shared.AddTexture("VelocityBuffer", TextureDescription.CreateTexture2DWithRTV(width, height, 1, Format.R32G32Float));

            Viewport = new(width, height);
        }

        public void Update(IGraphicsContext context)
        {
            paramsBuffer.Update(context, new(scale));
        }

        public void Draw(IGraphicsContext context)
        {
            context.SetRenderTarget(Velocity.Value?.RenderTargetView, null);
            context.SetViewport(Viewport);
            context.PSSetConstantBuffer(0, paramsBuffer);
            context.PSSetConstantBuffer(1, Camera.Value);
            context.PSSetSampler(0, sampler);
            context.PSSetShaderResource(0, Depth.Value);
            quad.DrawAuto(context, pipeline);
            context.ClearState();
        }

        public void Resize(int width, int height)
        {
            Velocity = ResourceManager2.Shared.UpdateTexture("VelocityBuffer", TextureDescription.CreateTexture2DWithRTV(width, height, 1, Format.R32G32Float));
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
            quad.Dispose();
            pipeline.Dispose();
            sampler.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}