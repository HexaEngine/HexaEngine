namespace HexaEngine.Objects
{
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;

    public struct MeshData
    {
        public string Name = string.Empty;
        public MeshVertex[]? Vertices;
        public int[]? Indices;
        public BoundingBox BoundingBox;
        public MeshBone[]? Bones;
        public Animature? Animature;

        public MeshData()
        {
        }
    }
}