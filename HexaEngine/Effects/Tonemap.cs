namespace HexaEngine.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.PostFx;
    using HexaEngine.Core.Resources;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public class Tonemap : IPostFx
    {
        private Quad quad;
        private IGraphicsPipeline pipeline;
        private ConstantBuffer<Params> paramBuffer;
        private ISamplerState sampler;
        private unsafe void** srvs;
        private unsafe void** cbvs;
        private float bloomStrength = 0.04f;
        private bool fogEnabled = false;
        private float fogStart = 10;
        private float fogEnd = 100;
        private Vector3 fogColor = Vector3.Zero;
        private bool dirty;

        private ResourceRef<IBuffer> Camera;
        private IRenderTargetView Output;
        private IShaderResourceView Input;
        private ResourceRef<Texture> Bloom;
        private ResourceRef<IShaderResourceView> Position;
        public Viewport Viewport;

        private struct Params
        {
            public float BloomStrength;
            public float FogEnabled;
            public float FogStart;
            public float FogEnd;
            public Vector3 FogColor;
            public float Padding;

            public Params(float bloomStrength)
            {
                BloomStrength = bloomStrength;
                Padding = default;
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

        public string Name => "Tonemap";

        public PostFxFlags Flags => PostFxFlags.None;

        public bool Enabled { get; set; } = true;

        public int Priority { get; set; } = 0;

        public async Task Initialize(IGraphicsDevice device, int width, int height, ShaderMacro[] macros)
        {
            quad = new(device);
            pipeline = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "effects/tonemap/vs.hlsl",
                PixelShader = "effects/tonemap/ps.hlsl",
            }, macros);
            paramBuffer = new(device, new Params(bloomStrength), CpuAccessFlags.Write);
            sampler = device.CreateSamplerState(SamplerDescription.LinearClamp);

            Bloom = ResourceManager2.Shared.GetResource<Texture>("Bloom");
            Position = ResourceManager2.Shared.GetResource<IShaderResourceView>("GBuffer.Position");
            Camera = ResourceManager2.Shared.GetResource<IBuffer>("CBCamera");
            InitUnsafe();
            Bloom.Resource.ValueChanged += OnUpdate;
            Position.Resource.ValueChanged += OnUpdate;
            Camera.Resource.ValueChanged += OnUpdate;
        }

        private unsafe void OnUpdate(object? sender, IDisposable? e)
        {
            srvs[1] = (void*)(Bloom.Value?.ShaderResourceView?.NativePointer ?? 0);
            srvs[2] = (void*)(Position.Value?.NativePointer ?? 0);
            cbvs[0] = (void*)paramBuffer.Buffer.NativePointer;
            cbvs[1] = (void*)(Camera.Value?.NativePointer ?? 0);
        }

        private unsafe void InitUnsafe()
        {
            srvs = AllocArray(3);
            srvs[0] = (void*)(Input?.NativePointer ?? 0);
            srvs[1] = (void*)(Bloom.Value?.ShaderResourceView?.NativePointer ?? 0);
            srvs[2] = (void*)(Position.Value?.NativePointer ?? 0);
            cbvs = AllocArray(2);
            cbvs[0] = (void*)paramBuffer.Buffer.NativePointer;
            cbvs[1] = (void*)(Camera.Value?.NativePointer ?? 0);
        }

        public void Resize(int width, int height)
        {
        }

        public void SetOutput(IRenderTargetView view, Viewport viewport)
        {
            Output = view;
            Viewport = viewport;
        }

        public unsafe void SetInput(IShaderResourceView view)
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
            if (Output is null) return;

            context.SetRenderTarget(Output, default);
            context.SetViewport(Viewport);
            context.PSSetConstantBuffers(cbvs, 2, 0);
            context.PSSetSampler(sampler, 0);
            context.PSSetShaderResources(srvs, 3, 0);
            quad.DrawAuto(context, pipeline);
            context.ClearState();
        }

        public unsafe void Dispose()
        {
            quad.Dispose();
            pipeline.Dispose();
            sampler.Dispose();
            paramBuffer.Dispose();
            Free(srvs);
            GC.SuppressFinalize(this);
        }
    }
}