#nullable disable

using HexaEngine;

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

        public IBuffer Camera;
        private IRenderTargetView Output;
        private IShaderResourceView Input;
        private IShaderResourceView Bloom;
        private IShaderResourceView Position;
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
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "effects/tonemap/vs.hlsl",
                PixelShader = "effects/tonemap/ps.hlsl",
            }, macros);
            paramBuffer = new(device, CpuAccessFlags.Write);
            sampler = device.CreateSamplerState(SamplerDescription.LinearClamp);

            Bloom = await ResourceManager.GetTextureSRVAsync("Bloom", true);
            Position = await ResourceManager.GetSRVAsync("GBuffer.Position");
            Camera = await ResourceManager.GetConstantBufferAsync("CBCamera");
            InitUnsafe();
        }

        private unsafe void InitUnsafe()
        {
            srvs = AllocArray(3);
            srvs[0] = (void*)Input?.NativePointer;
            srvs[1] = (void*)Bloom?.NativePointer;
            srvs[2] = (void*)Position?.NativePointer;
            cbvs = AllocArray(2);
            cbvs[0] = (void*)paramBuffer.Buffer.NativePointer;
            cbvs[1] = (void*)Camera?.NativePointer;
        }

        private unsafe void EndResizeUnsafe()
        {
            srvs[0] = (void*)Input?.NativePointer;
            srvs[1] = (void*)Bloom?.NativePointer;
            srvs[2] = (void*)Position?.NativePointer;
            cbvs[0] = (void*)paramBuffer.Buffer.NativePointer;
            cbvs[1] = (void*)Camera?.NativePointer;
        }

        public async void Resize(int width, int height)
        {
            Bloom = await ResourceManager.GetTextureSRVAsync("Bloom", true);
            Position = await ResourceManager.GetSRVAsync("GBuffer.Position");
            Camera = await ResourceManager.GetConstantBufferAsync("CBCamera");
            EndResizeUnsafe();
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
            context.PSSetConstantBuffers(cbvs, 2, 0);
            context.PSSetSampler(sampler, 0);
            context.PSSetShaderResources(srvs, 3, 0);
            quad.DrawAuto(context, pipeline, Viewport);
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