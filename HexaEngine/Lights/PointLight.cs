namespace HexaEngine.Lights
{
    using HexaEngine.Editor.Attributes;

#if GenericAttributes
    [EditorNode<PointLight>("Point Light")]
#endif

    public class PointLight : Light
    {
        public PointLight()
        {
            CreatePropertyEditor<PointLight>();
            Transform.Updated += (s, e) => { Updated = true; };
        }

        [EditorProperty("Strength")]
        public float Strength { get; set; } = 1000;

        public override LightType Type => LightType.Point;
    }
}