namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Objects.Primitives;
    using ImGuiNET;
    using System.Numerics;

    public class DDASSREffect : Effect
    {
        private IBuffer cb;
        private Vector2 targetSize = new(500, 500);
        private int maxRayStep = 70;
        private float depthbias = 0.01f;
        private float rayStepScale = 1.05f;
        private float maxThickness = 1.8f;
        private float maxRayLength = 200.0f;
        private bool isDirty = true;

        public DDASSREffect(IGraphicsDevice device) : base(device, new()
        {
            VertexShader = "effects/ddassr/vs.hlsl",
            PixelShader = "effects/ddassr/ps.hlsl",
        })
        {
            cb = CreateConstantBuffer<DDASSRCB>(ShaderStage.Pixel, 0);
            Mesh = new Quad(device);
        }

        public Vector2 TargetSize
        { get => targetSize; set { targetSize = value; isDirty = true; } }

        public int MaxRayStep
        { get => maxRayStep; set { maxRayStep = value; isDirty = true; } }

        public float Depthbias
        { get => depthbias; set { depthbias = value; isDirty = true; } }

        public float RayStepScale
        { get => rayStepScale; set { rayStepScale = value; isDirty = true; } }

        public float MaxThickness
        { get => maxThickness; set { maxThickness = value; isDirty = true; } }

        public float MaxRayLength
        { get => maxRayLength; set { maxRayLength = value; isDirty = true; } }

        public struct DDASSRCB
        {
            public Vector2 TargetSize;
            public Vector2 InvTargetSize;
            public int MaxRayStep;
            public Vector3 padd;
            public float Depthbias;
            public float RayStepScale;
            public float MaxThickness;
            public float MaxRayLength;

            public DDASSRCB(Vector2 targetSize, int maxRayStep, float depthbias, float rayStepScale, float maxThickness, float maxRayLength)
            {
                TargetSize = targetSize;
                InvTargetSize = new Vector2(1) / targetSize;
                MaxRayStep = maxRayStep;
                padd = default;
                Depthbias = depthbias;
                RayStepScale = rayStepScale;
                MaxThickness = maxThickness;
                MaxRayLength = maxRayLength;
            }
        }

        public override void Draw(IGraphicsContext context)
        {
            if (isDirty)
            {
                context.Write(cb, new DDASSRCB(targetSize, maxRayStep, depthbias, rayStepScale, maxThickness, maxRayLength));
                isDirty = false;
            }
#nullable disable
            DrawAuto(context, Target.Viewport);
#nullable enable
        }

        public override void DrawSettings()
        {
            if (ImGui.CollapsingHeader("DDA SSR settings"))
            {
                {
                    var value = TargetSize;
                    if (ImGui.InputFloat2("Target size", ref value))
                    {
                        TargetSize = value;
                    }
                }
                {
                    var value = MaxRayStep;
                    if (ImGui.SliderInt("Max ray steps", ref value, 1, 256))
                    {
                        MaxRayStep = value;
                    }
                }
                {
                    var value = Depthbias;
                    if (ImGui.InputFloat("Depthbias", ref value))
                    {
                        Depthbias = value;
                    }
                }
                {
                    var value = RayStepScale;
                    if (ImGui.InputFloat("Ray step scale", ref value))
                    {
                        RayStepScale = value;
                    }
                }
                {
                    var value = MaxThickness;
                    if (ImGui.InputFloat("Max thickness", ref value))
                    {
                        MaxThickness = value;
                    }
                }
                {
                    var value = MaxRayLength;
                    if (ImGui.InputFloat("Max ray length", ref value))
                    {
                        MaxRayLength = value;
                    }
                }
            }
        }
    }
}