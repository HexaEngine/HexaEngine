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
            Transform.Updated += (s, e) => { Updated = true; };
        }

        public override LightType Type => LightType.Directional;
    }
}