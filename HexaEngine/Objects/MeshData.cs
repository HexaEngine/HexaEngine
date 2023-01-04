namespace HexaEngine.Objects
{
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;

    public unsafe struct MeshData
    {
        public string Name = string.Empty;
        public MeshVertex* Vertices;
        public uint VerticesCount;
        public int* Indices;
        public uint IndicesCount;
        public BoundingBox BoundingBox;
        public BoundingSphere BoundingSphere;
        public MeshBone[]? Bones;
        public Animature? Animature;

        public MeshData()
        {
        }
    }
}