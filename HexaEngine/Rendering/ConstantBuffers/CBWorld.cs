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
}