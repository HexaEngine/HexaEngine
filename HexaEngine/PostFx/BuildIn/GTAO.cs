#nullable disable

namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using System.Numerics;

    public class GTAO
    {
        private IGraphicsDevice device;
        private IGraphicsPipelineState pipeline;
        private ConstantBuffer<GTAOParams> paramsBuffer;
        private ISamplerState samplerLinear;

        private bool disposedValue;
        private int enabled;

        public int QualityLevel = 2;        // 0: low; 1: medium; 2: high; 3: ultra
        public int DenoisePasses = 1;        // 0: disabled; 1: sharp; 2: medium; 3: soft
        public float Radius = 0.5f;     // [0.0,  ~ ]   World (view) space size of the occlusion sphere.

        // auto-tune-d settings
        public float RadiusMultiplier = 1.457f;

        public float FalloffRange = 0.615f;
        public float SampleDistributionPower = 2.0f;
        public float ThinOccluderCompensation = 0.0f;
        public float FinalValuePower = 2.2f;
        public float DepthMIPSamplingOffset = 3.30f;

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

        public void Initialize(IGraphicsDevice device, int width, int height)
        {
            this.device = device;

            pipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "effects/gato/vs.hlsl",
                PixelShader = "effects/gato/ps.hlsl",
            }, GraphicsPipelineStateDesc.DefaultFullscreen);
            paramsBuffer = new(CpuAccessFlags.Write);

            samplerLinear = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
        }

        public unsafe void Draw(IGraphicsContext context)
        {
        }

        protected virtual unsafe void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                pipeline.Dispose();
                paramsBuffer.Dispose();
                samplerLinear.Dispose();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}