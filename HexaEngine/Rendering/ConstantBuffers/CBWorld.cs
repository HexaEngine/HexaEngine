namespace HexaEngine.Rendering.ConstantBuffers
{
    using HexaEngine.Objects;
    using HexaEngine.Scenes;
    using System.Numerics;

    public struct CBWorld
    {
        public Matrix4x4 World;
        public Matrix4x4 WorldInv;

        public CBWorld(SceneNode mesh)
        {
            World = Matrix4x4.Transpose(mesh.Transform.Global);
            WorldInv = Matrix4x4.Transpose(mesh.Transform.GlobalInverse);
        }

        public CBWorld(Matrix4x4 transform)
        {
            Matrix4x4.Invert(transform, out var inverse);
            World = Matrix4x4.Transpose(transform);
            WorldInv = Matrix4x4.Transpose(inverse);
        }
    }

    public struct CBTessellation
    {
        public float MinFactor;
        public float MaxFactor;
        public float MinDistance;
        public float MaxDistance;

        public CBTessellation()
        {
            MinFactor = 1.0f;
            MaxFactor = 8.0f;
            MinDistance = 4.0f;
            MaxDistance = 50.0f;
        }

        public CBTessellation(float minFactor, float maxFactor, float minDistance, float maxDistance)
        {
            MinFactor = minFactor;
            MaxFactor = maxFactor;
            MinDistance = minDistance;
            MaxDistance = maxDistance;
        }
    }
}