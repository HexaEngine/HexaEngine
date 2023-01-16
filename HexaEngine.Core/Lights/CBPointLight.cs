namespace HexaEngine.Core.Lights
{
    using HexaEngine.Lights;
    using System.Numerics;

    public struct CBPointLight
    {
        public Vector4 Color;
        public Vector3 Position;
        public int padd;

        public CBPointLight(PointLight point) : this()
        {
            Color = point.Color * point.Strength;
            Position = point.Transform.GlobalPosition;
        }

        public void Update(PointLight point)
        {
            Color = point.Color * point.Strength;
            Position = point.Transform.GlobalPosition;
        }

        public override string ToString()
        {
            return Color.ToString();
        }
    }
}