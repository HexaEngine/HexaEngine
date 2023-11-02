namespace HexaEngine.Editor.TextEditor
{
    using HexaEngine.Core.Unsafes;

    public unsafe struct TextHistoryEntry
    {
        public StdString* Data;

        public void Release()
        {
            Data->Release();
            Free(Data);
            Data = null;
        }
    }
}