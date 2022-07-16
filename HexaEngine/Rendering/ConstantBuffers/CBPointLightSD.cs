namespace HexaEngine.Rendering.ConstantBuffers
{
    using HexaEngine.Lights;
    using System.Numerics;

    public struct CBPointLightSD
    {
        public Matrix4x4 Y;
        public Matrix4x4 Yneg;
        public Matrix4x4 X;
        public Matrix4x4 Xneg;
        public Matrix4x4 Z;
        public Matrix4x4 Zneg;
        public Matrix4x4 Proj;
        public Vector4 Color;
        public Vector3 Position;
        public int padd;

        public CBPointLightSD(PointLight point) : this()
        {
            Y = default;
            Yneg = default;
            X = default;
            Xneg = default;
            Z = default;
            Zneg = default;
            Proj = default;
            Color = point.Color * point.Strength;
            Position = point.Transform.Position;
            padd = default;
        }
    }
}