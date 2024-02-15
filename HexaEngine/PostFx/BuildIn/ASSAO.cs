#nullable disable

namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using System.Numerics;

    public class ASSAO
    {
        private IGraphicsDevice device;
        private IGraphicsPipelineState pipeline;
        private ConstantBuffer<ASSAOParams> paramsBuffer;

        private ISamplerState samplerLinear;

        private bool disposedValue;
        private int enabled;

        public float Radius;                             // [0.0,  ~ ] World (view) space size of the occlusion sphere.
        public float ShadowMultiplier;                   // [0.0, 5.0] Effect strength linear multiplier
        public float ShadowPower;                        // [0.5, 5.0] Effect strength pow modifier
        public float ShadowClamp;                        // [0.0, 1.0] Effect max limit (applied after multiplier but before blur)
        public float HorizonAngleThreshold;              // [0.0, 0.2] Limits self-shadowing (makes the sampling area less of a hemisphere, more of a spherical cone, to avoid self-shadowing and various artifacts due to low tessellation and depth buffer imprecision, etc.)
        public float FadeOutFrom;                        // [0.0,  ~ ] Distance to start start fading out the effect.
        public float FadeOutTo;                          // [0.0,  ~ ] Distance at which the effect is faded out.
        public int QualityLevel;                         // [ -1,  3 ] Effect quality; -1 - lowest (low, half res checkerboard), 0 - low, 1 - medium, 2 - high, 3 - very high / adaptive; each quality level is roughly 2x more costly than the previous, except the q3 which is variable but, in general, above q2.
        public float AdaptiveQualityLimit;               // [0.0, 1.0] (only for Quality Level 3)
        public int BlurPassCount;                        // [  0,   6] Number of edge-sensitive smart blur passes to apply. Quality 0 is an exception with only one 'dumb' blur pass used.
        public float Sharpness;                          // [0.0, 1.0] (How much to bleed over edges; 1: not at all, 0.5: half-half; 0.0: completely ignore edges)
        public float TemporalSupersamplingAngleOffset;   // [0.0,  PI] Used to rotate sampling kernel; If using temporal AA / supersampling, suggested to rotate by ( (frame%3)/3.0*PI ) or similar. Kernel is already symmetrical, which is why we use PI and not 2*PI.
        public float TemporalSupersamplingRadiusOffset;  // [0.0, 2.0] Used to scale sampling kernel; If using temporal AA / supersampling, suggested to scale by ( 1.0f + (((frame%3)-1.0)/3.0)*0.1 ) or similar.
        public float DetailShadowStrength;               // [0.0, 5.0] Used for high-res detail AO using neighboring depth pixels: adds a lot of detail but also reduces temporal stability (adds aliasing).

        public ASSAO()
        {
            Radius = 1.2f;
            ShadowMultiplier = 1.0f;
            ShadowPower = 1.50f;
            ShadowClamp = 0.98f;
            HorizonAngleThreshold = 0.06f;
            FadeOutFrom = 50.0f;
            FadeOutTo = 300.0f;
            AdaptiveQualityLimit = 0.45f;
            QualityLevel = 2;
            BlurPassCount = 2;
            Sharpness = 0.98f;
            TemporalSupersamplingAngleOffset = 0.0f;
            TemporalSupersamplingRadiusOffset = 1.0f;
            DetailShadowStrength = 0.5f;
        }

        #region Structs

        private unsafe struct ASSAOParams
        {
            public Vector2 ViewportPixelSize;              // .zw == 1.0 / ViewportSize.xy
            public Vector2 HalfViewportPixelSize;          // .zw == 1.0 / ViewportHalfSize.xy

            public Vector2 DepthUnpackConsts;
            public Vector2 CameraTanHalfFOV;

            public Vector2 NDCToViewMul;
            public Vector2 NDCToViewAdd;

            public Vector2 PerPassFullResCoordOffset;
            public Vector2 PerPassFullResUVOffset;

            public Vector2 Viewport2xPixelSize;
            public Vector2 Viewport2xPixelSize_x_025;          // Viewport2xPixelSize * 0.25 (for fusing add+mul into mad)

            public float EffectRadius;                           // world (viewspace) maximum size of the shadow
            public float EffectShadowStrength;                   // global strength of the effect (0 - 5)
            public float EffectShadowPow;
            public float EffectShadowClamp;

            public float EffectFadeOutMul;                       // effect fade out from distance (ex. 25)
            public float EffectFadeOutAdd;                       // effect fade out to distance   (ex. 100)
            public float EffectHorizonAngleThreshold;            // limit errors on slopes and caused by insufficient geometry tessellation (0.05 to 0.5)
            public float EffectSamplingRadiusNearLimitRec;       // if viewspace pixel closer than this, don't enlarge shadow sampling filterRadius anymore (makes no sense to grow beyond some distance, not enough samples to cover everything, so just limit the shadow growth; could be SSAOSettingsFadeOutFrom * 0.1 or less)

            public float DepthPrecisionOffsetMod;
            public float NegRecEffectRadius;                     // -1.0 / EffectRadius
            public float LoadCounterAvgDiv;                      // 1.0 / ( halfDepthMip[SSAO_DEPTH_MIP_LEVELS-1].SizeX * halfDepthMip[SSAO_DEPTH_MIP_LEVELS-1].sizeY )
            public float AdaptiveSampleCountLimit;

            public float InvSharpness;
            public int PassIndex;
            public Vector2 QuarterResPixelSize;                    // used for importance map only

            public Vector2 PatternRotScaleMatrices0;
            public Vector2 PatternRotScaleMatrices1;
            public Vector2 PatternRotScaleMatrices2;
            public Vector2 PatternRotScaleMatrices3;
            public Vector2 PatternRotScaleMatrices4;

            public float NormalsUnpackMul;
            public float NormalsUnpackAdd;
            public float DetailAOStrength;
            public float Dummy0;

            public Matrix4x4 NormalsWorldToViewspaceMatrix;

            public ASSAOParams()
            {
            }
        }

        #endregion Structs

        #region Properties

        public string Name => "ASSAO";

        public bool Enabled { get => enabled == 1; set => enabled = value ? 1 : 0; }

        #endregion Properties

        public void Initialize(IGraphicsDevice device, int width, int height)
        {
            this.device = device;

            pipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/assao/ps.hlsl",
            }, GraphicsPipelineStateDesc.DefaultFullscreen);
            paramsBuffer = new(device, CpuAccessFlags.Write);

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

        ~ASSAO()
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