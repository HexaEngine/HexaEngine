namespace HexaEngine.Core.Meshes.IO
{
    using HexaEngine.Core.Meshes;

    public unsafe struct TerrainBody
    {
        public TerrainVertex* Vertices;
        public int* Indices;
    }
}