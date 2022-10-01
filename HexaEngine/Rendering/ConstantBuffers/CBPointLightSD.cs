namespace HexaEngine.Rendering.ConstantBuffers
{
    using HexaEngine.Lights;
    using System.Numerics;

    public struct CBPointLightSD
    {
        public Vector4 Color;
        public Vector3 Position;
        public int padd;

        public CBPointLightSD(PointLight point) : this()
        {
            Color = point.Color * point.Strength;
            Position = point.Transform.GlobalPosition;
            padd = default;
        }
    }
}