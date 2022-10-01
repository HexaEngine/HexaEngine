namespace HexaEngine.Lights
{
    using HexaEngine.Editor.Attributes;

    [EditorNode("Point Light")]
    public class PointLight : Light
    {
        [EditorProperty("Strength")]
        public float Strength { get; set; } = 1000;

        public override LightType Type => LightType.Point;
    }
}