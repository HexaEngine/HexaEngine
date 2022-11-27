namespace HexaEngine.Lights
{
    using HexaEngine.Editor.Attributes;

    [EditorNode<PointLight>("Point Light")]
    public class PointLight : Light
    {
        public PointLight()
        {
            CreatePropertyEditor<PointLight>();
            Transform.Updated += (s, e) => { Updated = true; };
        }

        [EditorProperty("Shadow Range")]
        public float ShadowRange { get; set; } = 100;

        [EditorProperty("Strength")]
        public float Strength { get; set; } = 1000;

        public override LightType Type => LightType.Point;
    }
}