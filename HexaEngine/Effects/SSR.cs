namespace HexaEngine.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.PostFx;
    using HexaEngine.Core.Resources;
    using HexaEngine.Mathematics;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Text;
    using System.Threading.Tasks;

    public class SSR : IPostFx
    {
        private IGraphicsDevice device;
        private bool enabled = true;
        private Quad quad;
        private IGraphicsPipeline pipelineSSR;
        private ITexture2D ssrBuffer;
        private unsafe void** ssrSRVs;
        private IShaderResourceView ssrSRV;
        private IRenderTargetView ssrRTV;

        private BlurParams blurParams;
        private IBuffer cbBlur;
        private IGraphicsPipeline pipelineBlend;

        private ISamplerState sampler;

        public IRenderTargetView Output;
        public Viewport Viewport;
        public IShaderResourceView Input;
        public ResourceRef<IShaderResourceView> Position;
        public ResourceRef<IShaderResourceView> Normal;
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

            sampler = device.CreateSamplerState(SamplerDescription.LinearClamp);

            pipelineSSR = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "effects/ssr/vs.hlsl",
                PixelShader = "effects/ssr/ps.hlsl",
            }, macros);

            pipelineBlend = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "effects/ssr/vs.hlsl",
                PixelShader = "effects/ssr/blend.hlsl"
            }, macros);

            blurParams = new();
            cbBlur = device.CreateBuffer(blurParams, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);

            ssrBuffer = device.CreateTexture2D(Format.R16G16B16A16Float, width, height, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget);
            ssrSRV = device.CreateShaderResourceView(ssrBuffer);
            ssrRTV = device.CreateRenderTargetView(ssrBuffer, new(width, height));

            Camera = ResourceManager2.Shared.GetBuffer("CBCamera");
            Position = ResourceManager2.Shared.GetResource<IShaderResourceView>("GBuffer.Position");
            Normal = ResourceManager2.Shared.GetResource<IShaderResourceView>("GBuffer.Normal");
        }

        public void Resize(int width, int height)
        {
            ssrBuffer.Dispose();
            ssrRTV.Dispose();
            ssrSRV.Dispose();
            ssrBuffer = device.CreateTexture2D(Format.R16G16B16A16Float, width, height, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget);
            ssrSRV = device.CreateShaderResourceView(ssrBuffer);
            ssrRTV = device.CreateRenderTargetView(ssrBuffer, new(width, height));
        }

        public void Draw(IGraphicsContext context)
        {
            if (Output == null)
                return;

            context.ClearRenderTargetView(ssrRTV, default);
            context.SetRenderTarget(ssrRTV, null);
            context.SetViewport(ssrRTV.Viewport);
            context.PSSetShaderResource(Input, 0);
            context.PSSetShaderResource(Position.Value, 1);
            context.PSSetShaderResource(Normal.Value, 2);
            context.PSSetConstantBuffer(Camera.Value, 1);
            context.PSSetSampler(sampler, 0);
            quad.DrawAuto(context, pipelineSSR);

            context.SetRenderTarget(Output, null);
            context.SetViewport(Output.Viewport);
            context.PSSetShaderResource(ssrSRV, 0);
            context.PSSetShaderResource(Input, 1);
            context.PSSetConstantBuffer(cbBlur, 0);
            context.PSSetSampler(sampler, 0);
            quad.DrawAuto(context, pipelineBlend);
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
            ssrBuffer.Dispose();
            ssrRTV.Dispose();
            ssrSRV.Dispose();
        }
    }
}