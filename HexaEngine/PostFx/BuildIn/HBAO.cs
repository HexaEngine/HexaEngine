#nullable disable

namespace HexaEngine.Effects.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Effects.Blur;
    using HexaEngine.Mathematics;
    using HexaEngine.PostFx;
    using HexaEngine.Rendering.Graph;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using System.Numerics;

    public class HBAO : IPostFx
    {
        private IGraphicsDevice device;
        private IGraphicsPipeline pipeline;
        private ConstantBuffer<HBAOParams> paramsBuffer;
        private Texture2D noiseTex;
        private Texture2D intermediateTex;
        private GaussianBlur blur;

        private Viewport viewport;

        private ISamplerState samplerLinear;

        private bool disposedValue;
        private bool enabled = true;
        private float samplingRadius = 0.5f;
        private uint numSamplingDirections = 8;
        private float samplingStep = 0.004f;
        private uint numSamplingSteps = 4;
        private float power = 1;
        private int priority;
        private const int NoiseSize = 4;
        private const int NoiseStride = 4;

        #region Structs

        private struct HBAOParams
        {
            public float SamplingRadius;
            public float SamplingRadiusToScreen;
            public uint NumSamplingDirections;
            public float SamplingStep;

            public uint NumSamplingSteps;
            public float Power;
            public Vector2 NoiseScale;

            public HBAOParams()
            {
                SamplingRadius = 0.5f;
                SamplingRadiusToScreen = 0;
                NumSamplingDirections = 8;
                SamplingStep = 0.004f;
                NumSamplingSteps = 4;
                Power = 1;
            }
        }

        #endregion Structs

        #region Properties

        public event Action<bool>? OnEnabledChanged;

        public event Action<int>? OnPriorityChanged;

        public string Name => "HBAO";

        public PostFxFlags Flags { get; } = PostFxFlags.NoOutput | PostFxFlags.NoInput;

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

        public float SamplingRadius { get => samplingRadius; set => samplingRadius = value; }

        public uint NumSamplingDirections { get => numSamplingDirections; set => numSamplingDirections = value; }

        public float SamplingStep { get => samplingStep; set => samplingStep = value; }

        public uint NumSamplingSteps { get => numSamplingSteps; set => numSamplingSteps = value; }

        public float Power { get => power; set => power = value; }

        #endregion Properties

        public async Task Initialize(IGraphicsDevice device, PostFxDependencyBuilder builder, int width, int height, ShaderMacro[] macros)
        {
            this.device = device;

            pipeline = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/hbao/ps.hlsl",
            }, GraphicsPipelineState.DefaultFullscreen);
            paramsBuffer = new(device, CpuAccessFlags.Write);
            intermediateTex = new(device, Format.R32Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            blur = new(device, Format.R32Float, width, height);

            unsafe
            {
                Texture2DDescription description = new(Format.R32G32B32A32Float, NoiseSize, NoiseSize, 1, 1, BindFlags.ShaderResource, Usage.Immutable);

                float* pixelData = Alloc<float>(NoiseSize * NoiseSize * NoiseStride);

                SubresourceData initialData = default;
                initialData.DataPointer = (nint)pixelData;
                initialData.RowPitch = NoiseSize * NoiseStride;

                int idx = 0;
                for (int i = 0; i < NoiseSize * NoiseSize; i++)
                {
                    float rand = Random.Shared.NextSingle() * float.Pi * 2.0f;
                    pixelData[idx++] = MathF.Sin(rand);
                    pixelData[idx++] = MathF.Cos(rand);
                    pixelData[idx++] = Random.Shared.NextSingle();
                    pixelData[idx++] = 1.0f;
                }

                noiseTex = new(device, description, initialData);
            }

            samplerLinear = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
            viewport = new(width, height);
        }

        public void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport)
        {
        }

        public void SetInput(IShaderResourceView view, ITexture2D resource)
        {
        }

        public void Resize(int width, int height)
        {
            intermediateTex.Resize(device, Format.R32Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);

            viewport = new(width, height);
        }

        public void Update(IGraphicsContext context)
        {
            var camera = CameraManager.Current;
            HBAOParams hbaoParams = default;
            hbaoParams.SamplingRadius = samplingRadius;
            hbaoParams.SamplingRadiusToScreen = samplingRadius * 0.5f * viewport.Height / (MathF.Tan(camera.Fov.ToRad() * 0.5f) * 2.0f); ;
            hbaoParams.SamplingStep = samplingStep;
            hbaoParams.NumSamplingSteps = numSamplingSteps;
            hbaoParams.NumSamplingDirections = numSamplingDirections;
            hbaoParams.Power = power;
            hbaoParams.NoiseScale = viewport.Size / NoiseSize;

            paramsBuffer.Update(context, hbaoParams);
        }

        public unsafe void Draw(IGraphicsContext context, GraphResourceBuilder creator)
        {
            var ao = creator.GetTexture2D("#AOBuffer");
            var depth = creator.GetDepthStencilBuffer("#DepthStencil");
            var camera = creator.GetConstantBuffer<CBCamera>("CBCamera");
            var gbuffer = creator.GetGBuffer("GBuffer");

            context.ClearRenderTargetView(ao.RTV, Vector4.One);

            nint* srvs = stackalloc nint[] { depth.SRV.NativePointer, gbuffer.SRVs[1].NativePointer, noiseTex.SRV.NativePointer };
            nint* cbs = stackalloc nint[] { paramsBuffer.NativePointer, camera.NativePointer };
            context.SetRenderTarget(intermediateTex.RTV, null);
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

            blur.Blur(context, intermediateTex.SRV, ao.RTV, (int)viewport.Width, (int)viewport.Height);
        }

        protected virtual unsafe void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                pipeline.Dispose();
                paramsBuffer.Dispose();
                noiseTex.Dispose();
                intermediateTex.Dispose();
                blur.Dispose();
                samplerLinear.Dispose();
                disposedValue = true;
            }
        }

        ~HBAO()
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