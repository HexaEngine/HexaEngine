namespace HexaEngine.Effects.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Resources;
    using HexaEngine.Effects.Blur;
    using HexaEngine.Mathematics;
    using HexaEngine.PostFx;
    using HexaEngine.Rendering.Graph;
    using HexaEngine.Scenes;
    using System;
    using System.Numerics;
    using System.Threading.Tasks;

    public class SSGI : IPostFx
    {
        private IGraphicsDevice device;
        private bool enabled = false;
        private IGraphicsPipeline pipelineSSGI;
        private ConstantBuffer<SSGIParams> ssgiParams;
        private Texture2D inputChain;
        private Texture2D outputBuffer;
        private GaussianBlur blur;
        private ISamplerState linearWrapSampler;
        private IGraphicsPipeline copy;

        public IRenderTargetView Output;
        public Viewport Viewport;
        public IShaderResourceView Input;
        public ITexture2D InputTex;
        private bool isDirty = false;

        private int priority = 402;

        public event Action<bool>? OnEnabledChanged;

        public event Action<int>? OnPriorityChanged;

        public string Name { get; } = "SSGI";

        public PostFxFlags Flags { get; } = PostFxFlags.Inline;

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

        private struct SSGIParams
        {
            public float Intensity;
            public float Distance;
            public float NoiseAmount;
            public int Noise;

            public SSGIParams()
            {
                Intensity = 1f;
                Distance = 10;
                NoiseAmount = .4f;
                Noise = 1;
            }
        }

        #endregion Structs

        public async Task Initialize(IGraphicsDevice device, PostFxDependencyBuilder builder, int width, int height, ShaderMacro[] macros)
        {
            builder
                .RunBefore("Compose")
                .RunAfter("TAA")
                .RunAfter("HBAO")
                .RunAfter("MotionBlur")
                .RunAfter("DepthOfField")
                .RunAfter("GodRays")
                .RunAfter("VolumetricClouds")
                .RunAfter("SSR")
                .RunBefore("LensFlare")
                .RunBefore("Bloom")
                .RunBefore("AutoExposure");

            this.device = device;

            linearWrapSampler = device.CreateSamplerState(SamplerStateDescription.LinearWrap);

            pipelineSSGI = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/ssgi/ps.hlsl",
            },
            new GraphicsPipelineState()
            {
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleStrip
            },
            macros);

            copy = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "effects/copy/vs.hlsl",
                PixelShader = "effects/copy/ps.hlsl",
            });

            ssgiParams = new(device, new SSGIParams(), CpuAccessFlags.Write);
            inputChain = new(device, Format.R16G16B16A16Float, width, height, 1, 10, CpuAccessFlags.None, GpuAccessFlags.RW, ResourceMiscFlag.GenerateMips);
            outputBuffer = new(device, Format.R16G16B16A16Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            blur = new(device, Format.R16G16B16A16Float, width, height);
        }

        public void Update(IGraphicsContext context)
        {
        }

        public void Resize(int width, int height)
        {
        }

        public void Draw(IGraphicsContext context, GraphResourceBuilder creator)
        {
            if (Output == null)
                return;

            var depth = creator.GetDepthMipChain("HiZBuffer");
            var camera = creator.GetConstantBuffer<CBCamera>("CBCamera");
            var gbuffer = creator.GetGBuffer("GBuffer");

            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);
            context.PSSetShaderResource(0, Input);
            context.PSSetShaderResource(1, depth.SRV);
            context.PSSetShaderResource(2, gbuffer.SRVs[1]);
            context.PSSetConstantBuffer(0, ssgiParams);
            context.PSSetConstantBuffer(1, camera);
            context.PSSetSampler(0, linearWrapSampler);

            context.SetGraphicsPipeline(pipelineSSGI);

            context.DrawInstanced(4, 1, 0, 0);

            context.SetGraphicsPipeline(null);

            context.PSSetSampler(0, null);
            context.PSSetConstantBuffer(1, null);
            context.PSSetConstantBuffer(0, null);
            context.PSSetShaderResource(1, null);
            context.PSSetShaderResource(0, null);
            context.SetRenderTarget(null, null);
        }

        public void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport)
        {
            Output = view;
            Viewport = viewport;
        }

        public void SetInput(IShaderResourceView view, ITexture2D resource)
        {
            Input = view;
            InputTex = resource;
        }

        public void Dispose()
        {
            pipelineSSGI.Dispose();
            linearWrapSampler.Dispose();
            ssgiParams.Dispose();
        }
    }
}