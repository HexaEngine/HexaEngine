#nullable disable

namespace HexaEngine.Effects
{
    using HexaEngine.Core.Effects.Blur;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes.Managers;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public class HBAO : IEffect, IAmbientOcclusion
    {
        private IGraphicsDevice device;
        private Quad quad;
        private IGraphicsPipeline pipeline;
        private ConstantBuffer<HBAOParams> paramsBuffer;
        private Texture2D noiseTex;
        private Texture2D intermediateTex;
        private GaussianBlur blur;

        private Viewport viewport;

        private ISamplerState samplerLinear;

        public ResourceRef<Texture> Output;
        public ResourceRef<IBuffer> Camera;
        public ResourceRef<IShaderResourceView> Depth;
        public ResourceRef<IShaderResourceView> Normal;

        private bool disposedValue;
        private bool enabled = true;
        private float samplingRadius = 0.5f;
        private uint numSamplingDirections = 8;
        private float samplingStep = 0.004f;
        private uint numSamplingSteps = 4;
        private float power = 1;

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

        public string Name => "HBAO";

        public bool Enabled { get => enabled; set => enabled = value; }

        public float SamplingRadius { get => samplingRadius; set => samplingRadius = value; }

        public uint NumSamplingDirections { get => numSamplingDirections; set => numSamplingDirections = value; }

        public float SamplingStep { get => samplingStep; set => samplingStep = value; }

        public uint NumSamplingSteps { get => numSamplingSteps; set => numSamplingSteps = value; }

        public float Power { get => power; set => power = value; }

        #endregion Properties

        public async Task Initialize(IGraphicsDevice device, int width, int height)
        {
            this.device = device;
            Output = ResourceManager2.Shared.AddTexture("AOBuffer", TextureDescription.CreateTexture2DWithRTV(width, height, 1, Format.R32Float));

            quad = new Quad(device);

            pipeline = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "effects/hbao/vs.hlsl",
                PixelShader = "effects/hbao/ps.hlsl",
            });
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

            Depth = ResourceManager2.Shared.GetResource<IShaderResourceView>("GBuffer.Depth");
            Normal = ResourceManager2.Shared.GetResource<IShaderResourceView>("GBuffer.Normal");
            Camera = ResourceManager2.Shared.GetBuffer("CBCamera");

            samplerLinear = device.CreateSamplerState(SamplerDescription.LinearClamp);
            viewport = new(width, height);
        }

        public void BeginResize()
        {
        }

        public void Resize(int width, int height)
        {
            Output = ResourceManager2.Shared.UpdateTexture("AOBuffer", TextureDescription.CreateTexture2DWithRTV(width, height, 1, Format.R32Float));

            intermediateTex.Resize(device, Format.R32Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);

            viewport = new(width, height);
        }

        public unsafe void Draw(IGraphicsContext context)
        {
            if (Output is null)
            {
                return;
            }

            if (!enabled)
            {
                context.ClearRenderTargetView(Output.Value.RenderTargetView, Vector4.One);
                return;
            }
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

            nint* srvs = stackalloc nint[] { Depth.Value.NativePointer, Normal.Value.NativePointer, noiseTex.SRV.NativePointer };
            nint* cbs = stackalloc nint[] { paramsBuffer.NativePointer, Camera.Value.NativePointer };
            context.SetRenderTarget(intermediateTex.RTV, null);
            context.SetViewport(viewport);
            context.PSSetShaderResources(0, 3, (void**)srvs);
            context.PSSetConstantBuffers(0, 2, (void**)cbs);
            context.PSSetSampler(0, samplerLinear);
            quad.DrawAuto(context, pipeline);
            ZeroMemory(srvs, sizeof(nint) * 3);
            ZeroMemory(cbs, sizeof(nint) * 2);
            context.PSSetSampler(0, null);
            context.PSSetConstantBuffers(0, 2, (void**)cbs);
            context.PSSetShaderResources(0, 3, (void**)srvs);
            context.SetRenderTarget(null, null);

            blur.Blur(context, intermediateTex.SRV, Output.Value.RenderTargetView, (int)viewport.Width, (int)viewport.Height);
        }

        protected virtual unsafe void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                quad.Dispose();
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