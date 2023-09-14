namespace HexaEngine.Meshes
{
    using System.Numerics;

    public struct CBWorld
    {
        public Matrix4x4 World;
        public Matrix4x4 WorldInv;

        public CBWorld(Matrix4x4 transform)
        {
            Matrix4x4.Invert(transform, out var inverse);
            World = Matrix4x4.Transpose(transform);
            WorldInv = Matrix4x4.Transpose(inverse);
        }
    }
}