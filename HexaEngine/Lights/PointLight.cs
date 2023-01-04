namespace HexaEngine.Lights
{
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Mathematics;

    [EditorNode<PointLight>("Point Light")]
    public class PointLight : Light
    {
        public readonly BoundingFrustum[] Frusta = new BoundingFrustum[6];

        public PointLight()
        {
            CreatePropertyEditor<PointLight>();
            Transform.Updated += (s, e) => { Updated = true; };
            for (int i = 0; i < Frusta.Length; i++)
            {
                Frusta[i] = new();
            }
        }

        [EditorProperty("Shadow Range")]
        public float ShadowRange { get; set; } = 100;

        [EditorProperty("Strength")]
        public float Strength { get; set; } = 1000;

        public override LightType Type => LightType.Point;
    }
}