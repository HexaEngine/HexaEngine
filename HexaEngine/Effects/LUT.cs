namespace HexaEngine.Effects
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.PostFx;
    using HexaEngine.Mathematics;
    using System.Numerics;
    using System.Threading.Tasks;

    public class LUT : IPostFx
    {
        private Quad quad;
        private IGraphicsPipeline pipeline;
        private ISamplerState sampler;
        private ISamplerState samplerLut;
        private ConstantBuffer<LUTParams> paramBuffer;

        private Texture2D lut;

        public IRenderTargetView Output;
        public IShaderResourceView Input;
        public Viewport Viewport;
        private bool enabled = true;
        private float amountChroma;
        private float amountLuma;
        private int priority = -1;

        public event Action<bool>? OnEnabledChanged;

        public event Action<int>? OnPriorityChanged;

        public string Name => "LUT";

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

        [Slider(0f, 1f)]
        public float AmountChroma { get => amountChroma; set => amountChroma = value; }

        [Slider(0f, 1f)]
        public float AmountLuma { get => amountLuma; set => amountLuma = value; }

        #region Structs

        private struct LUTParams
        {
            public float AmountChroma;
            public float AmountLuma;
            public Vector2 Padding;

            public LUTParams(float amountChroma, float amountLuma)
            {
                AmountChroma = amountChroma;
                AmountLuma = amountLuma;
            }

            public LUTParams()
            {
                AmountChroma = 1;
                AmountLuma = 1;
            }
        }

        #endregion Structs

        public async Task Initialize(IGraphicsDevice device, int width, int height, ShaderMacro[] macros)
        {
            quad = new(device);
            pipeline = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "effects/lut/vs.hlsl",
                PixelShader = "effects/lut/ps.hlsl"
            }, macros);
            sampler = device.CreateSamplerState(SamplerDescription.LinearClamp);
            samplerLut = device.CreateSamplerState(SamplerDescription.LinearClamp);

            paramBuffer = new(device, new LUTParams(), CpuAccessFlags.Write);

            lut = new Texture2D(device, new TextureFileDescription(Paths.CurrentAssetsPath + "textures/lut.png", mipLevels: 1));

            Viewport = new(width, height);
        }

        public void Resize(int width, int height)
        {
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

        public unsafe void Draw(IGraphicsContext context)
        {
            if (Output == null)
            {
                return;
            }

            nint* srvs = stackalloc nint[2];
            srvs[0] = Input.NativePointer;
            srvs[1] = lut.SRV.NativePointer;

            context.SetRenderTarget(Output, default);
            context.SetViewport(Viewport);
            context.PSSetShaderResources(0, 2, (void**)srvs);
            context.PSSetSampler(0, sampler);
            context.PSSetSampler(1, samplerLut);
            context.PSSetConstantBuffer(0, paramBuffer);
            quad.DrawAuto(context, pipeline);
            context.ClearState();
        }

        public void Dispose()
        {
            quad.Dispose();
            pipeline.Dispose();
            sampler.Dispose();
        }
    }
}