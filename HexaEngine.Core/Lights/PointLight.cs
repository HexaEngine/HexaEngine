namespace HexaEngine.Core.Lights
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using Newtonsoft.Json;

    [EditorNode<PointLight>("Point Light")]
    public class PointLight : Light
    {
        [JsonIgnore]
        public unsafe BoundingBox* ShadowBox;

        private float shadowRange = 100;
        private float strength = 1;
        private float falloff = 100;

        [EditorProperty("Shadow Range")]
        public float ShadowRange { get => shadowRange; set => SetAndNotifyWithEqualsTest(ref shadowRange, value); }

        [EditorProperty("Strength")]
        public float Strength { get => strength; set => SetAndNotifyWithEqualsTest(ref strength, value); }

        [EditorProperty("Falloff")]
        public float Falloff { get => falloff; set => SetAndNotifyWithEqualsTest(ref falloff, value); }

        [JsonIgnore]
        public override LightType LightType => LightType.Point;

        public override unsafe void Initialize(IGraphicsDevice device)
        {
            Updated = true;
            ShadowBox = Alloc<BoundingBox>();
            base.Initialize(device);
        }

        public override unsafe bool IntersectFrustum(BoundingBox box)
        {
            return ShadowBox->Intersects(box);
        }

        public override unsafe void Uninitialize()
        {
            base.Uninitialize();
            Free(ShadowBox);
        }
    }
}