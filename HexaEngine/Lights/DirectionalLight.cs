namespace HexaEngine.Lights
{
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Mathematics;

    [EditorNode<DirectionalLight>("Directional Light")]
    public class DirectionalLight : Light
    {
        public new CameraTransform Transform = new();

        public DirectionalLight()
        {
            base.Transform = Transform;
            CreatePropertyEditor<DirectionalLight>();
        }

        public override LightType Type => LightType.Directional;
    }
}