namespace HexaEngine.Core.IO.Meshes
{
    using HexaEngine.Core.Meshes;

    public unsafe struct TerrainBody
    {
        public TerrainVertex* Vertices;
        public int* Indices;
    }
}