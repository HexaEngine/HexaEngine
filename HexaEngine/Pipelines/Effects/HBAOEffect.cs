namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Nodes;
    using HexaEngine.Graphics;
    using HexaEngine.Objects.Primitives;
    using ImGuiNET;
    using Silk.NET.Core.Native;
    using System.Collections.Generic;

    public class HBAO : Effect
    {
        private readonly IBuffer cb;
        private bool isDirty;
        private float samplingRadius = 0.5f;
        private uint numSamplingDirections = 8;
        private float samplingStep = 0.004f;
        private uint numSamplingSteps = 4;

        public float SamplingRadius
        { get => samplingRadius; set { samplingRadius = value; isDirty = true; } }

        public uint NumSamplingDirections
        { get => numSamplingDirections; set { numSamplingDirections = value; isDirty = true; } }

        public float SamplingStep
        { get => samplingStep; set { samplingStep = value; isDirty = true; } }

        public uint NumSamplingSteps
        { get => numSamplingSteps; set { numSamplingSteps = value; isDirty = true; } }

        private struct HBAOCB
        {
            public float SAMPLING_RADIUS;
            public uint NUM_SAMPLING_DIRECTIONS;
            public float SAMPLING_STEP;
            public uint NUM_SAMPLING_STEPS;

            public HBAOCB(float samplingRadius, uint numSamplingDirections, float samplingStep, uint numSamplingSteps)
            {
                SAMPLING_RADIUS = samplingRadius;
                NUM_SAMPLING_DIRECTIONS = numSamplingDirections;
                SAMPLING_STEP = samplingStep;
                NUM_SAMPLING_STEPS = numSamplingSteps;
            }
        }

        public unsafe HBAO(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "effects/hbao/vs.hlsl",
            PixelShader = "effects/hbao/ps.hlsl",
        })
        {
            Mesh = new Quad(device);
            cb = CreateConstantBuffer<HBAOCB>(ShaderStage.Pixel, 0);
            device.Context.Write(cb, new HBAOCB(samplingRadius, numSamplingDirections, samplingStep, numSamplingSteps));

            TargetType = PinType.Texture2D;
            ResourceSlots.Add((1, "Position", PinType.Texture2D));
            ResourceSlots.Add((2, "Normal", PinType.Texture2D));
        }

        public override void Draw(IGraphicsContext context)
        {
            if (isDirty)
            {
                context.Write(cb, new HBAOCB(samplingRadius, numSamplingDirections, samplingStep, numSamplingSteps));
                isDirty = false;
            }
#nullable disable
            DrawAuto(context, Target.Viewport);
#nullable enable
        }

        public override void DrawSettings()
        {
            if (ImGui.CollapsingHeader("HBAO settings"))
            {
                {
                    var value = SamplingRadius;
                    if (ImGui.InputFloat("SamplingRadius", ref value))
                    {
                        SamplingRadius = value;
                    }
                }
                {
                    var value = (int)NumSamplingDirections;
                    if (ImGui.InputInt("NumSamplingDirections", ref value))
                    {
                        NumSamplingDirections = (uint)value;
                    }
                }
                {
                    var value = SamplingStep;
                    if (ImGui.InputFloat("SamplingStep", ref value))
                    {
                        SamplingStep = value;
                    }
                }
                {
                    var value = (int)NumSamplingSteps;
                    if (ImGui.InputInt("NumSamplingSteps", ref value))
                    {
                        NumSamplingSteps = (uint)value;
                    }
                }
            }
        }
    }
}