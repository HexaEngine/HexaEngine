namespace HexaEngine.Core.Lights
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Mathematics;

    [EditorNode<DirectionalLight>("Directional Light")]
    public class DirectionalLight : Light
    {
        public new CameraTransform Transform = new();

        public DirectionalLight()
        {
            base.Transform = Transform;
            OverwriteTransform(Transform);
        }

        public override LightType LightType => LightType.Directional;

        public override bool IntersectFrustum(BoundingBox box)
        {
            return true;
        }
    }
}