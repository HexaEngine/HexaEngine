namespace HexaEngine.Core.IO.Meshes
{
    using HexaEngine.Core.Meshes;

    public unsafe struct SkinnedBody
    {
        public SkinnedMeshVertex* Vertices;
        public int* Indices;
    }
}