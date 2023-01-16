namespace HexaEngine.IO.Meshes
{
    using HexaEngine.Meshes;

    public unsafe struct SkinnedBody
    {
        public SkinnedMeshVertex* Vertices;
        public int* Indices;
    }
}