namespace HexaEngine.Effects.BuildIn
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Resources;
    using HexaEngine.Mathematics;
    using HexaEngine.PostFx;
    using HexaEngine.Rendering.Graph;
    using HexaEngine.Scenes;
    using System.Numerics;

    public class Compose : PostFxBase
    {
        private IGraphicsPipeline pipeline;
        private ConstantBuffer<ComposeParams> paramBuffer;
        private ISamplerState sampler;
        private unsafe void** srvs;
        private unsafe void** cbvs;
        private float bloomStrength = 0.04f;
        private bool fogEnabled = false;
        private float fogStart = 10;
        private float fogEnd = 100;
        private Vector3 fogColor = Vector3.Zero;
        private float lutAmountChroma = 1;
        private float lutAmountLuma = 1;

        private IRenderTargetView Output;
        private IShaderResourceView Input;
        private ResourceRef<Texture2D> Bloom;
        private ResourceRef<IShaderResourceView> Luma;
        private Texture2D LUT;

        public Viewport Viewport;

        public override string Name => "Compose";

        public override PostFxFlags Flags => PostFxFlags.None;

        public unsafe float BloomStrength
        {
            get => bloomStrength;
            set => NotifyPropertyChangedAndSet(ref bloomStrength, value);
        }

        public unsafe bool FogEnabled
        {
            get => fogEnabled;
            set => NotifyPropertyChangedAndSet(ref fogEnabled, value);
        }

        public unsafe float FogStart
        {
            get => fogStart;
            set => NotifyPropertyChangedAndSet(ref fogStart, value);
        }

        public unsafe float FogEnd
        {
            get => fogEnd;
            set => NotifyPropertyChangedAndSet(ref fogEnd, value);
        }

        public unsafe Vector3 FogColor
        {
            get => fogColor;
            set => NotifyPropertyChangedAndSet(ref fogColor, value);
        }

        public unsafe float LUTAmountChroma
        {
            get => lutAmountChroma;
            set => NotifyPropertyChangedAndSet(ref lutAmountChroma, value);
        }

        public unsafe float LUTAmountLuma
        {
            get => lutAmountLuma;
            set => NotifyPropertyChangedAndSet(ref lutAmountLuma, value);
        }

        #region Structs

        private struct ComposeParams
        {
            public float BloomStrength;
            public float FogEnabled;
            public float FogStart;
            public float FogEnd;
            public Vector3 FogColor;
            public float LUTAmountChroma;
            public float LUTAmountLuma;
            public Vector3 padd;

            public ComposeParams(float bloomStrength)
            {
                BloomStrength = bloomStrength;
                padd = default;
            }
        }

        #endregion Structs

        public override async Task Initialize(IGraphicsDevice device, PostFxDependencyBuilder builder, int width, int height, ShaderMacro[] macros)
        {
            builder
                .AddBinding("Bloom")
                .AddBinding("AutoExposure")
                .RunAfter("!AllNotReferenced");

            pipeline = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/compose/ps.hlsl",
            }, GraphicsPipelineState.DefaultFullscreen, macros);
            paramBuffer = new(device, new ComposeParams(bloomStrength), CpuAccessFlags.Write);
            sampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);

            Bloom = ResourceManager2.Shared.GetTexture("Bloom");
            Luma = ResourceManager2.Shared.GetResource<IShaderResourceView>("Luma");
            LUT = new Texture2D(device, new TextureFileDescription(Paths.CurrentAssetsPath + "textures/lut.png", mipLevels: 1));
            InitUnsafe();
            Bloom.Resource.ValueChanged += OnUpdate;
            Luma.Resource.ValueChanged += OnUpdate;
        }

        private unsafe void OnUpdate(object? sender, IDisposable? e)
        {
            srvs[1] = (void*)(Bloom.Value?.SRV?.NativePointer ?? 0);
            srvs[4] = (void*)(Luma.Value?.NativePointer ?? 0);
            cbvs[0] = (void*)paramBuffer.Buffer.NativePointer;
        }

        private unsafe void InitUnsafe()
        {
            srvs = AllocArray(6);
            srvs[0] = (void*)(Input?.NativePointer ?? 0);
            srvs[1] = (void*)(Bloom.Value?.SRV?.NativePointer ?? 0);
            srvs[2] = null;
            srvs[4] = (void*)(Luma.Value?.NativePointer ?? 0);
            srvs[5] = (void*)(LUT.SRV?.NativePointer ?? 0);
            cbvs = AllocArray(2);
            cbvs[0] = (void*)paramBuffer.Buffer.NativePointer;
        }

        public override void Resize(int width, int height)
        {
        }

        public override void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport)
        {
            Output = view;
            Viewport = viewport;
        }

        public override unsafe void SetInput(IShaderResourceView view, ITexture2D resource)
        {
            Input = view;
            srvs[0] = (void*)view.NativePointer;
        }

        public override void Update(IGraphicsContext context)
        {
            if (dirty)
            {
                ComposeParams composeParams;
                composeParams.BloomStrength = bloomStrength;
                composeParams.FogEnabled = fogEnabled ? 1 : 0;
                composeParams.FogStart = fogStart;
                composeParams.FogEnd = fogEnd;
                composeParams.FogColor = fogColor;
                composeParams.LUTAmountChroma = lutAmountChroma;
                composeParams.LUTAmountLuma = lutAmountLuma;
                composeParams.padd = default;
                paramBuffer.Update(context, composeParams);
                dirty = false;
            }
        }

        public override unsafe void Draw(IGraphicsContext context, GraphResourceBuilder creator)
        {
            if (Output is null)
            {
                return;
            }

            var depth = creator.GetDepthStencilBuffer("#DepthStencil");
            var camera = creator.GetConstantBuffer<CBCamera>("CBCamera");

            srvs[3] = (void*)(depth.SRV?.NativePointer ?? 0);
            cbvs[1] = (void*)camera.NativePointer;

            context.SetRenderTarget(Output, default);
            context.SetViewport(Viewport);
            context.PSSetConstantBuffers(0, 2, cbvs);
            context.PSSetSampler(0, sampler);
            context.PSSetShaderResources(0, 6, srvs);
            context.SetGraphicsPipeline(pipeline);
            context.DrawInstanced(4, 1, 0, 0);
            context.ClearState();
        }

        protected override unsafe void DisposeCore()
        {
            pipeline.Dispose();
            sampler.Dispose();
            paramBuffer.Dispose();
            LUT.Dispose();
            Free(srvs);
            srvs = null;
        }
    }
}