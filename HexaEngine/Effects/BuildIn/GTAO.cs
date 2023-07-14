#nullable disable

namespace HexaEngine.Effects.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Resources;
    using HexaEngine.Effects.Blur;
    using HexaEngine.Mathematics;
    using HexaEngine.PostFx;
    using System.Numerics;

    public class GTAO : IEffect, IAmbientOcclusion
    {
        private IGraphicsDevice device;
        private Quad quad;
        private IGraphicsPipeline pipeline;
        private ConstantBuffer<GTAOParams> paramsBuffer;
        private Texture2D noiseTex;
        private Texture2D intermediateBuffer;
        private GaussianBlur blur;

        private Viewport viewport;

        private ISamplerState samplerLinear;

        public ResourceRef<Texture2D> Output;
        public ResourceRef<IBuffer> Camera;
        public ResourceRef<IShaderResourceView> Depth;
        public ResourceRef<IShaderResourceView> Normal;

        private bool disposedValue;
        private int enabled;

        private int QualityLevel = 2;        // 0: low; 1: medium; 2: high; 3: ultra
        private int DenoisePasses = 1;        // 0: disabled; 1: sharp; 2: medium; 3: soft
        private float Radius = 0.5f;     // [0.0,  ~ ]   World (view) space size of the occlusion sphere.

        // auto-tune-d settings
        private float RadiusMultiplier = 1.457f;

        private float FalloffRange = 0.615f;
        private float SampleDistributionPower = 2.0f;
        private float ThinOccluderCompensation = 0.0f;
        private float FinalValuePower = 2.2f;
        private float DepthMIPSamplingOffset = 3.30f;

        private const int NoiseSize = 4;
        private const int NoiseStride = 4;

        public GTAO()
        {
        }

        #region Structs

        private unsafe struct GTAOParams
        {
            public Vector2 ViewportSize;
            public Vector2 ViewportPixelSize;                  // .zw == 1.0 / ViewportSize.xy

            public Vector2 DepthUnpackConsts;
            public Vector2 CameraTanHalfFOV;

            public Vector2 NDCToViewMul;
            public Vector2 NDCToViewAdd;

            public Vector2 NDCToViewMul_x_PixelSize;
            public float EffectRadius;                       // world (viewspace) maximum size of the shadow
            public float EffectFalloffRange;

            public float RadiusMultiplier;
            public float Padding0;
            public float FinalValuePower;
            public float DenoiseBlurBeta;

            public float SampleDistributionPower;
            public float ThinOccluderCompensation;
            public float DepthMIPSamplingOffset;
            public int NoiseIndex;                         // frameIndex % 64 if using TAA or 0 otherwise

            public GTAOParams()
            {
            }
        }

        #endregion Structs

        #region Properties

        public string Name => "GTAO";

        public bool Enabled { get => enabled == 1; set => enabled = value ? 1 : 0; }

        #endregion Properties

        public async Task Initialize(IGraphicsDevice device, int width, int height)
        {
            this.device = device;
            Output = ResourceManager2.Shared.AddTexture("AOBuffer", new Texture2DDescription(Format.R32Float, width / 2, height / 2, 1, 1, BindFlags.ShaderResource | BindFlags.RenderTarget));

            quad = new Quad(device);

            pipeline = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "effects/gato/vs.hlsl",
                PixelShader = "effects/gato/ps.hlsl",
            });
            paramsBuffer = new(device, CpuAccessFlags.Write);
            intermediateBuffer = new(device, Format.R32Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
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
                    pixelData[idx++] = Random.Shared.NextSingle();
                    pixelData[idx++] = Random.Shared.NextSingle();
                    pixelData[idx++] = 0.0f;
                    pixelData[idx++] = 1.0f;
                }

                noiseTex = new(device, description, initialData);
            }

            Depth = ResourceManager2.Shared.GetResource<IShaderResourceView>("GBuffer.Depth");
            Normal = ResourceManager2.Shared.GetResource<IShaderResourceView>("GBuffer.Normal");
            Camera = ResourceManager2.Shared.GetBuffer("CBCamera");

            samplerLinear = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
            viewport = new(width, height);
        }

        public void BeginResize()
        {
        }

        public void Resize(int width, int height)
        {
            Output = ResourceManager2.Shared.UpdateTexture("AOBuffer", new Texture2DDescription(Format.R32Float, width / 2, height / 2, 1, 1, BindFlags.ShaderResource | BindFlags.RenderTarget));

            intermediateBuffer.Resize(device, Format.R32Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);

            viewport = new(width, height);
        }

        public unsafe void Draw(IGraphicsContext context)
        {
            if (Output is null)
            {
                return;
            }
        }

        protected virtual unsafe void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                quad.Dispose();
                pipeline.Dispose();
                paramsBuffer.Dispose();
                noiseTex.Dispose();
                intermediateBuffer.Dispose();
                blur.Dispose();
                samplerLinear.Dispose();
                disposedValue = true;
            }
        }

        ~GTAO()
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