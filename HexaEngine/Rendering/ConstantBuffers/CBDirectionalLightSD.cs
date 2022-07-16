namespace HexaEngine.Rendering.ConstantBuffers
{
    using HexaEngine.Lights;
    using System.Numerics;

    public struct CBDirectionalLightSD
    {
        public Matrix4x4 View;
        public Matrix4x4 Proj;
        public Vector4 Color;
        public Vector3 Direction;
        public int padd;

        public CBDirectionalLightSD(DirectionalLight light)
        {
            View = Matrix4x4.Transpose(light.Transform.View);
            Proj = Matrix4x4.Transpose(light.Transform.Projection);
            Color = light.Color;
            Direction = light.Transform.Forward;
            padd = default;
        }
    }
}