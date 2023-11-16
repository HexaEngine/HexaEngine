namespace HexaEngine.Editor.ImagePainter
{
    public unsafe struct ImageHistoryEntry
    {
        public int Index;
        public void* Data;
        public nint Size;

        public void Release()
        {
            Index = 0;

            if (Data != null)
            {
                Free(Data);
            }

            Data = null;
            Size = 0;
        }
    }
}