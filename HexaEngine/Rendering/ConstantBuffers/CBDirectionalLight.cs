namespace HexaEngine.Rendering.ConstantBuffers
{
    using HexaEngine.Lights;
    using System.Numerics;

    public struct CBDirectionalLight
    {
        public Vector4 Color;
        public Vector3 Direction;
        public int padd;

        public CBDirectionalLight(DirectionalLight light)
        {
            Color = light.Color;
            Direction = light.Transform.Forward;
            padd = default;
        }
    }
}