namespace HexaEngine.Core.Lights
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Mathematics;
    using Newtonsoft.Json;

    [EditorNode<PointLight>("Point Light")]
    public class PointLight : Light
    {
        [JsonIgnore]
        public unsafe BoundingBox* ShadowBox;

        public unsafe PointLight()
        {
            Transform.Updated += (s, e) => { Updated = true; };
        }

        [EditorProperty("Shadow Range")]
        public float ShadowRange { get; set; } = 100;

        [EditorProperty("Strength")]
        public float Strength { get; set; } = 1;

        [EditorProperty("Falloff")]
        public float Falloff { get; set; }

        [JsonIgnore]
        public override LightType LightType => LightType.Point;

        public override unsafe void Initialize(IGraphicsDevice device)
        {
            ShadowBox = Alloc<BoundingBox>();
            base.Initialize(device);
        }

        public override unsafe void Uninitialize()
        {
            base.Uninitialize();
            Free(ShadowBox);
        }

        public unsafe void InsertLightData(StructuredUavBuffer<PointLightData> l, StructuredUavBuffer<ShadowPointLightData> sl)
        {
            if (CastShadows)
            {
                l.Add(new(this));
            }
            else
            {
                sl.Add(new(this));
            }
        }
    }
}