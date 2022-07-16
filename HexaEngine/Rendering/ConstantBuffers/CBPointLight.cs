namespace HexaEngine.Rendering.ConstantBuffers
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
            Position = point.Transform.Position;
        }
    }
}