namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Graphics;
    using HexaEngine.Objects.Primitives;
    using ImGuiNET;
    using System.Numerics;

    public class HBAO : Effect
    {
        private readonly IBuffer cb;
        private bool isDirty = true;
        private HBAOParams hbaoParams;

        public bool Enabled
        { get => hbaoParams.Enabled == 1; set { hbaoParams.Enabled = value ? 1 : 0; isDirty = true; } }

        public float SamplingRadius
        { get => hbaoParams.SAMPLING_RADIUS; set { hbaoParams.SAMPLING_RADIUS = value; isDirty = true; } }

        public uint NumSamplingDirections
        { get => hbaoParams.NUM_SAMPLING_DIRECTIONS; set { hbaoParams.NUM_SAMPLING_DIRECTIONS = value; isDirty = true; } }

        public float SamplingStep
        { get => hbaoParams.SAMPLING_STEP; set { hbaoParams.SAMPLING_STEP = value; isDirty = true; } }

        public uint NumSamplingSteps
        { get => hbaoParams.NUM_SAMPLING_STEPS; set { hbaoParams.NUM_SAMPLING_STEPS = value; isDirty = true; } }

        private struct HBAOParams
        {
            public int Enabled;
            public float SAMPLING_RADIUS;
            public uint NUM_SAMPLING_DIRECTIONS;
            public float SAMPLING_STEP;
            public uint NUM_SAMPLING_STEPS;
            public Vector3 padd;

            public HBAOParams()
            {
                Enabled = 0;
                SAMPLING_RADIUS = 0.5f;
                NUM_SAMPLING_DIRECTIONS = 8;
                SAMPLING_STEP = 0.004f;
                NUM_SAMPLING_STEPS = 4;
                padd = default;
            }

            public HBAOParams(bool enabled, float samplingRadius, uint numSamplingDirections, float samplingStep, uint numSamplingSteps)
            {
                Enabled = enabled ? 1 : 0;
                SAMPLING_RADIUS = samplingRadius;
                NUM_SAMPLING_DIRECTIONS = numSamplingDirections;
                SAMPLING_STEP = samplingStep;
                NUM_SAMPLING_STEPS = numSamplingSteps;
                padd = default;
            }
        }

        public unsafe HBAO(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "effects/hbao/vs.hlsl",
            PixelShader = "effects/hbao/ps.hlsl",
        })
        {
            Mesh = new Quad(device);
            hbaoParams = new();
            cb = CreateConstantBuffer<HBAOParams>(ShaderStage.Pixel, 0);

            TargetType = PinType.Texture2D;
            ResourceSlots.Add((1, "Position", PinType.Texture2D));
            ResourceSlots.Add((2, "Normal", PinType.Texture2D));
        }

        public override void Draw(IGraphicsContext context)
        {
            if (isDirty)
            {
                context.Write(cb, hbaoParams);
                isDirty = false;
            }
#nullable disable
            DrawAuto(context, Target.Viewport);
#nullable enable
        }

        public override void DrawSettings()
        {
        }
    }
}