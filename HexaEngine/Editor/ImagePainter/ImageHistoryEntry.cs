namespace HexaEngine.Editor.ImagePainter
{
    public unsafe struct ImageHistoryEntry
    {
        public int Index;
        public void* Data;
        public nint Size;
    }
}