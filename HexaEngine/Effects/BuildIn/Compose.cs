namespace HexaEngine.Effects.BuildIn
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Resources;
    using HexaEngine.Mathematics;
    using HexaEngine.PostFx;
    using System.Numerics;

    public class Compose : IPostFx
    {
        private Quad quad;
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
        private bool dirty;

        private ResourceRef<IBuffer> Camera;
        private IRenderTargetView Output;
        private IShaderResourceView Input;
        private ResourceRef<Texture> Bloom;
        private ResourceRef<IShaderResourceView> Depth;
        private ResourceRef<IShaderResourceView> Luma;
        private Texture2D LUT;

        public Viewport Viewport;
        private int priority = 0;
        private bool enabled = true;

        public event Action<bool>? OnEnabledChanged;

        public event Action<int>? OnPriorityChanged;

        public string Name => "Compose";

        public PostFxFlags Flags => PostFxFlags.None;

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

        public unsafe float BloomStrength
        {
            get => bloomStrength;
            set
            {
                paramBuffer.Local->BloomStrength = value;
                bloomStrength = value;
                dirty = true;
            }
        }

        public unsafe bool FogEnabled
        {
            get => fogEnabled;
            set
            {
                paramBuffer.Local->FogEnabled = value ? 1 : 0;
                fogEnabled = value;
                dirty = true;
            }
        }

        public unsafe float FogStart
        {
            get => fogStart;
            set
            {
                fogStart = value;
                paramBuffer.Local->FogStart = value;
                dirty = true;
            }
        }

        public unsafe float FogEnd
        {
            get => fogEnd;
            set
            {
                fogEnd = value;
                paramBuffer.Local->FogEnd = value;
                dirty = true;
            }
        }

        public unsafe Vector3 FogColor
        {
            get => fogColor;
            set
            {
                fogColor = value;
                paramBuffer.Local->FogColor = value;
                dirty = true;
            }
        }

        public unsafe float LUTAmountChroma
        {
            get => lutAmountChroma;
            set
            {
                lutAmountChroma = value;
                paramBuffer.Local->LUTAmountChroma = value;
                dirty = true;
            }
        }

        public unsafe float LUTAmountLuma
        {
            get => lutAmountLuma;
            set
            {
                lutAmountLuma = value;
                paramBuffer.Local->LUTAmountLuma = value;
                dirty = true;
            }
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

        public async Task Initialize(IGraphicsDevice device, int width, int height, ShaderMacro[] macros)
        {
            quad = new(device);
            pipeline = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "effects/compose/vs.hlsl",
                PixelShader = "effects/compose/ps.hlsl",
            }, macros);
            paramBuffer = new(device, new ComposeParams(bloomStrength), CpuAccessFlags.Write);
            sampler = device.CreateSamplerState(SamplerDescription.LinearClamp);

            Bloom = ResourceManager2.Shared.GetResource<Texture>("Bloom");
            Depth = ResourceManager2.Shared.GetResource<IShaderResourceView>("GBuffer.Position");
            Luma = ResourceManager2.Shared.GetResource<IShaderResourceView>("Luma");
            Camera = ResourceManager2.Shared.GetResource<IBuffer>("CBCamera");
            LUT = new Texture2D(device, new TextureFileDescription(Paths.CurrentAssetsPath + "textures/lut.png", mipLevels: 1));
            InitUnsafe();
            Bloom.Resource.ValueChanged += OnUpdate;
            Depth.Resource.ValueChanged += OnUpdate;
            Luma.Resource.ValueChanged += OnUpdate;
            Camera.Resource.ValueChanged += OnUpdate;
        }

        private unsafe void OnUpdate(object? sender, IDisposable? e)
        {
            srvs[1] = (void*)(Bloom.Value?.ShaderResourceView?.NativePointer ?? 0);
            srvs[3] = (void*)(Depth.Value?.NativePointer ?? 0);
            srvs[4] = (void*)(Luma.Value?.NativePointer ?? 0);
            cbvs[0] = (void*)paramBuffer.Buffer.NativePointer;
            cbvs[1] = (void*)(Camera.Value?.NativePointer ?? 0);
        }

        private unsafe void InitUnsafe()
        {
            srvs = AllocArray(6);
            srvs[0] = (void*)(Input?.NativePointer ?? 0);
            srvs[1] = (void*)(Bloom.Value?.ShaderResourceView?.NativePointer ?? 0);
            srvs[2] = null;
            srvs[3] = (void*)(Depth.Value?.NativePointer ?? 0);
            srvs[4] = (void*)(Luma.Value?.NativePointer ?? 0);
            srvs[5] = (void*)(LUT.SRV?.NativePointer ?? 0);
            cbvs = AllocArray(2);
            cbvs[0] = (void*)paramBuffer.Buffer.NativePointer;
            cbvs[1] = (void*)(Camera.Value?.NativePointer ?? 0);
        }

        public void Resize(int width, int height)
        {
        }

        public void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport)
        {
            Output = view;
            Viewport = viewport;
        }

        public unsafe void SetInput(IShaderResourceView view, ITexture2D resource)
        {
            Input = view;
            srvs[0] = (void*)view.NativePointer;
        }

        public void Update(IGraphicsContext context)
        {
            if (dirty)
            {
                paramBuffer.Update(context);
                dirty = false;
            }
        }

        public unsafe void Draw(IGraphicsContext context)
        {
            if (Output is null)
            {
                return;
            }

            context.SetRenderTarget(Output, default);
            context.SetViewport(Viewport);
            context.PSSetConstantBuffers(0, 2, cbvs);
            context.PSSetSampler(0, sampler);
            context.PSSetShaderResources(0, 6, srvs);
            quad.DrawAuto(context, pipeline);
            context.ClearState();
        }

        public unsafe void Dispose()
        {
            quad.Dispose();
            pipeline.Dispose();
            sampler.Dispose();
            paramBuffer.Dispose();
            LUT.Dispose();
            Free(srvs);
            GC.SuppressFinalize(this);
        }
    }
}