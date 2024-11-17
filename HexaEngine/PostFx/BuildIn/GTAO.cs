/*
namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Editor.Attributes;
    using System;
    using System.Diagnostics;
    using System.Numerics;

    [EditorDisplayName("GTAO")]
    public class GTAO : PostFxBase
    {
#nullable disable
        private IGraphicsPipelineState pipeline;
        private IComputePipelineState prefilterDepth;
        private IComputePipelineState gato;
        private IComputePipelineState denoise;
        private IComputePipelineState denoiseLastPass;
        private ConstantBuffer<GTAOParams> paramsBuffer;

        private Texture2D hilbertLUT;
#nullable restore

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
        }

        public override string Name => "GTAO";

        public override PostFxFlags Flags { get; }

        public override PostFxColorSpace ColorSpace { get; }

        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
            builder
                .Override<SSAO>()
                .Override<HBAO>();
        }

        const uint XE_HILBERT_LEVEL = 6U;

        const uint XE_HILBERT_WIDTH = (1U << (int)XE_HILBERT_LEVEL);

        const uint XE_HILBERT_AREA = (XE_HILBERT_WIDTH * XE_HILBERT_WIDTH);

        private static uint HilbertIndex(uint posX, uint posY)
        {
            uint index = 0U;
            for (uint curLevel = XE_HILBERT_WIDTH / 2U; curLevel > 0U; curLevel /= 2U)
            {
                uint regionX = ((posX & curLevel) > 0U) ? 1u : 0u;
                uint regionY = ((posY & curLevel) > 0U) ? 1u : 0u;
                index += curLevel * curLevel * ((3U * regionX) ^ regionY);
                if (regionY == 0U)
                {
                    if (regionX == 1U)
                    {
                        posX = XE_HILBERT_WIDTH - 1U - posX;
                        posY = XE_HILBERT_WIDTH - 1U - posY;
                    }

                    (posY, posX) = (posX, posY);
                }
            }
            return index;
        }

        public override unsafe void Initialize(IGraphicsDevice device, PostFxGraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            prefilterDepth = device.CreateComputePipelineState(new ComputePipelineDesc()
            {
                Entry = "CSPrefilterDepths16x16",
                Path = "effects/gtao/vaGTAO.hlsl"
            });

            gato = device.CreateComputePipelineState(new ComputePipelineDesc()
            {
                Entry = "CSGTAOHigh",
                Path = "effects/gtao/vaGTAO.hlsl"
            });

            denoise = device.CreateComputePipelineState(new ComputePipelineDesc()
            {
                Entry = "CSDenoisePass",
                Path = "effects/gtao/vaGTAO.hlsl"
            });

            denoiseLastPass = device.CreateComputePipelineState(new ComputePipelineDesc()
            {
                Entry = "CSDenoiseLastPass",
                Path = "effects/gtao/vaGTAO.hlsl"
            });

            paramsBuffer = new(CpuAccessFlags.Write);

            ushort* data = stackalloc ushort[64 * 64];
            for (uint x = 0; x < 64; x++)
                for (uint y = 0; y < 64; y++)
                {
                    uint r2index = HilbertIndex(x, y);
                    Debug.Assert(r2index < 65536);
                    data[x + 64 * y] = (ushort)r2index;
                }

            SubresourceData subresourceData = new(data, 64 * 2);
            hilbertLUT = new(new Texture2DDescription(Format.R16UInt, 64, 64, 1, 1, GpuAccessFlags.Read));
        }

        public override void Draw(IGraphicsContext context)
        {
        }

        protected override void DisposeCore()
        {
            pipeline.Dispose();
            paramsBuffer.Dispose();
        }
    }
}
*/