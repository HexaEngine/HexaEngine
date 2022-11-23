namespace HexaEngine.Lights
{
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Mathematics;

#if GenericAttributes
    [EditorNode<DirectionalLight>("Directional Light")]
#endif

    public class DirectionalLight : Light
    {
        public new CameraTransform Transform = new();

        public DirectionalLight()
        {
            base.Transform = Transform;
            CreatePropertyEditor<DirectionalLight>();
            Transform.Updated += (s, e) => { Updated = true; };
        }

        public override LightType Type => LightType.Directional;
    }
}