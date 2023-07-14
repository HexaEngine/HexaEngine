namespace HexaEngine.Effects.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Resources;
    using HexaEngine.Mathematics;
    using HexaEngine.PostFx;
    using System;
    using System.Threading.Tasks;

    public class SSR : IPostFx
    {
        private IGraphicsDevice device;
        private bool enabled = true;
        private Quad quad;
        private IGraphicsPipeline pipelineSSR;

        private ISamplerState pointClampSampler;
        private ISamplerState linearClampSampler;
        private ISamplerState linearBorderSampler;

        public IRenderTargetView Output;
        public Viewport Viewport;
        public IShaderResourceView Input;
        public ResourceRef<IShaderResourceView> Normal;
        public ResourceRef<IShaderResourceView> Misc;
        public ResourceRef<IShaderResourceView> Depth;
        public ResourceRef<IBuffer> Camera;
        private int priority = 200;

        public event Action<bool>? OnEnabledChanged;

        public event Action<int>? OnPriorityChanged;

        public string Name { get; } = "SSR";

        public PostFxFlags Flags { get; } = PostFxFlags.None;

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

        #region Structs

        private struct BlurParams
        {
            public int Size;
            public float Reserved0;
            public float Reserved1;
            public float Reserved2;

            public BlurParams()
            {
                Size = 3;
                Reserved0 = 0;
                Reserved1 = 0;
                Reserved2 = 0;
            }

            public BlurParams(int size)
            {
                Size = size;
                Reserved0 = 0;
                Reserved1 = 0;
                Reserved2 = 0;
            }
        }

        #endregion Structs

        public async Task Initialize(IGraphicsDevice device, int width, int height, ShaderMacro[] macros)
        {
            this.device = device;
            quad = new(device);

            pointClampSampler = device.CreateSamplerState(SamplerStateDescription.PointClamp);
            linearClampSampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
            linearBorderSampler = device.CreateSamplerState(SamplerStateDescription.LinearBorder);

            pipelineSSR = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "effects/ssr/vs.hlsl",
                PixelShader = "effects/ssr/ps.hlsl",
            }, macros);

            Camera = ResourceManager2.Shared.GetBuffer("CBCamera");
            Depth = ResourceManager2.Shared.GetResource<IShaderResourceView>("GBuffer.Depth");
            Normal = ResourceManager2.Shared.GetResource<IShaderResourceView>("GBuffer.Normal");
            Misc = ResourceManager2.Shared.GetResource<IShaderResourceView>("GBuffer.Misc");
        }

        public void Resize(int width, int height)
        {
        }

        public void Draw(IGraphicsContext context)
        {
            if (Output == null)
                return;

            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);
            context.PSSetShaderResource(0, Normal.Value);
            context.PSSetShaderResource(1, Input);
            context.PSSetShaderResource(2, Depth.Value);
            context.PSSetShaderResource(3, Misc.Value);
            context.PSSetConstantBuffer(1, Camera.Value);
            context.PSSetSampler(0, pointClampSampler);
            context.PSSetSampler(1, linearClampSampler);
            context.PSSetSampler(2, linearBorderSampler);
            quad.DrawAuto(context, pipelineSSR);

            context.ClearState();
        }

        public void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport)
        {
            Output = view;
            Viewport = viewport;
        }

        public void SetInput(IShaderResourceView view, ITexture2D resource)
        {
            Input = view;
        }

        public void Update(IGraphicsContext context)
        {
        }

        public void Dispose()
        {
            quad.Dispose();
            pipelineSSR.Dispose();
            pointClampSampler.Dispose();
            linearClampSampler.Dispose();
            linearBorderSampler.Dispose();
        }
    }
}