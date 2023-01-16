namespace HexaEngine.Core.Lights
{
    using HexaEngine.Lights;
    using System.Numerics;

    public struct CBPointLightSD
    {
        public Vector4 Color;
        public Vector3 Position;
        public float Far;

        public CBPointLightSD(PointLight point) : this()
        {
            Color = point.Color * point.Strength;
            Position = point.Transform.GlobalPosition;
            Far = point.ShadowRange;
        }

        public void Update(PointLight point)
        {
            Color = point.Color * point.Strength;
            Position = point.Transform.GlobalPosition;
            Far = point.ShadowRange;
        }

        public override string ToString()
        {
            return Color.ToString();
        }
    }
}