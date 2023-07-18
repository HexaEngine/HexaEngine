namespace HexaEngine.Meshes
{
    using HexaEngine.Core.Graphics;
    using System.Numerics;

    public class Decal
    {
        public Texture2D? AlbedoDecalTexture = null;
        public Texture2D? NormalDecalTexture = null;
        public Matrix4x4 Transform;
        public DecalType Type = DecalType.ProjectXY;
        public bool ModifyGBufferNormals = false;
    }
}