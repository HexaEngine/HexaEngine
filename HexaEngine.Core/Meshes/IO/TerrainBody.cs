namespace HexaEngine.IO.Meshes
{
    using HexaEngine.Meshes;

    public unsafe struct TerrainBody
    {
        public TerrainVertex* Vertices;
        public int* Indices;
    }
}