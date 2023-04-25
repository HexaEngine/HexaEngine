namespace HexaEngine.Editor.Meshes
{
    public unsafe struct MeshHistoryEntry
    {
        public uint* VertexIds;
        public byte* Vertices;
        public uint Stride;
        public uint Count;
    }
}