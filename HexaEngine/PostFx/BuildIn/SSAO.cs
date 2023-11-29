#nullable disable

using HexaEngine;

namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics.Effects.Blur;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.PostFx;
    using System.Numerics;

    public class SSAO
    {
        private IGraphicsDevice device;
        private IGraphicsPipeline pipeline;
        private ConstantBuffer<SSAOParams> paramsBuffer;
        private Texture2D noiseTex;
        private Texture2D intermediateBuffer;
        private GaussianBlur blur;

        private Viewport viewport;

        private ISamplerState samplerLinear;

        private ResourceRef<Texture2D> ao;
        private ResourceRef<ConstantBuffer<CBCamera>> camera;
        private ResourceRef<DepthStencil> depth;
        private ResourceRef<GBuffer> gbuffer;

        private bool disposedValue;
        private bool enabled;
        private float tapSize;
        private uint numTaps;
        private float power;

        private const int NoiseSize = 4;
        private const int NoiseStride = 4;

        #region Structs

        private struct SSAOParams
        {
            public float TapSize;
            public uint NumTaps;
            public float Power;
            public Vector2 NoiseScale;
            public Vector3 padd;

            public SSAOParams()
            {
                TapSize = 0.0002f;
                NumTaps = 16;
                Power = 1.0f;
                padd = default;
            }
        }

        #endregion Structs

        #region Properties

        public string Name => "SSAO";

        public bool Enabled
        {
            get => enabled;
            set => enabled = value;
        }

        public float TapSize
        {
            get => tapSize;
            set => tapSize = value;
        }

        public uint NumTaps
        {
            get => numTaps;
            set => numTaps = value;
        }

        public float Power
        {
            get => power;
            set => power = value;
        }

        #endregion Properties

        public async Task Initialize(IGraphicsDevice device, PostFxDependencyBuilder builder, GraphResourceBuilder creator, int width, int height)
        {
            this.device = device;

            ao = creator.GetTexture2D("#AOBuffer");
            depth = creator.GetDepthStencilBuffer("#DepthStencil");
            camera = creator.GetConstantBuffer<CBCamera>("CBCamera");
            gbuffer = creator.GetGBuffer("GBuffer");

            pipeline = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/ssao/ps.hlsl",
                State = GraphicsPipelineState.DefaultFullscreen,
            });
            paramsBuffer = new(device, CpuAccessFlags.Write);
            intermediateBuffer = new(device, Format.R32Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            blur = new(device, Format.R32Float, width, height);

            unsafe
            {
                Texture2DDescription description = new(Format.R32G32B32A32Float, NoiseSize, NoiseSize, 1, 1, BindFlags.ShaderResource, Usage.Immutable);

                float* pixelData = AllocT<float>(NoiseSize * NoiseSize * NoiseStride);

                SubresourceData initialData = default;
                initialData.DataPointer = (nint)pixelData;
                initialData.RowPitch = NoiseSize * NoiseStride;

                int idx = 0;
                for (int i = 0; i < NoiseSize * NoiseSize; i++)
                {
                    pixelData[idx++] = Random.Shared.NextSingle().Map01ToN1P1();
                    pixelData[idx++] = Random.Shared.NextSingle().Map01ToN1P1();
                    pixelData[idx++] = 0.0f.Map01ToN1P1();
                    pixelData[idx++] = 1.0f;
                }

                noiseTex = new(device, description, initialData);
            }

            samplerLinear = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
            viewport = new(width, height);
        }

        public void BeginResize()
        {
        }

        public void Resize(int width, int height)
        {
            intermediateBuffer.Resize(device, Format.R32Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);

            viewport = new(width, height);
        }

        public unsafe void Draw(IGraphicsContext context)
        {
            if (ao is null)
            {
                return;
            }

            if (!enabled)
            {
                context.ClearRenderTargetView(ao.Value.RTV, Vector4.One);
                return;
            }

            SSAOParams ssaoParams = default;
            ssaoParams.TapSize = tapSize;
            ssaoParams.NumTaps = numTaps;
            ssaoParams.Power = power;
            ssaoParams.NoiseScale = viewport.Size / NoiseSize;
            paramsBuffer.Update(context, ssaoParams);

            nint* srvs = stackalloc nint[] { depth.Value.SRV.NativePointer, gbuffer.Value.SRVs[1].NativePointer, noiseTex.SRV.NativePointer };
            nint* cbs = stackalloc nint[] { paramsBuffer.NativePointer, camera.Value.NativePointer };
            context.SetRenderTarget(intermediateBuffer.RTV, null);
            context.SetViewport(viewport);

            context.PSSetShaderResources(0, 3, (void**)srvs);
            context.PSSetConstantBuffers(0, 2, (void**)cbs);
            context.PSSetSampler(0, samplerLinear);

            context.SetGraphicsPipeline(pipeline);

            context.DrawInstanced(4, 1, 0, 0);

            context.SetGraphicsPipeline(null);

            ZeroMemory(srvs, sizeof(nint) * 3);
            ZeroMemory(cbs, sizeof(nint) * 2);
            context.PSSetSampler(0, null);
            context.PSSetConstantBuffers(0, 2, (void**)cbs);
            context.PSSetShaderResources(0, 3, (void**)srvs);

            context.SetRenderTarget(null, null);

            blur.Blur(context, intermediateBuffer.SRV, ao.Value.RTV, (int)viewport.Width, (int)viewport.Height);
        }

        protected virtual unsafe void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                pipeline.Dispose();
                paramsBuffer.Dispose();
                noiseTex.Dispose();
                intermediateBuffer.Dispose();
                blur.Dispose();
                samplerLinear.Dispose();
                disposedValue = true;
            }
        }

        ~SSAO()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}