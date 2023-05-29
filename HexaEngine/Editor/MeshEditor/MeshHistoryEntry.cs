namespace HexaEngine.Editor.MeshEditor
{
    public unsafe struct MeshHistoryEntry
    {
        public uint* VertexIds;
        public byte* Vertices;
        public uint Stride;
        public uint Count;
    }
}