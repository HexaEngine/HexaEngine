namespace HexaEngine.Core.Meshes.IO
{
    using HexaEngine.Core.Meshes;

    public unsafe struct SkinnedBody
    {
        public SkinnedMeshVertex* Vertices;
        public int* Indices;
    }
}