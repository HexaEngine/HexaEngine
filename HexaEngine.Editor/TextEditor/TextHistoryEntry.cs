namespace HexaEngine.Editor.TextEditor
{
    using Hexa.NET.Utilities;

    public unsafe struct TextHistoryEntry
    {
        public StdWString* Data;

        public void Release()
        {
            if (Data != null)
            {
                Data->Release();
                Free(Data);
                Data = null;
            }
        }
    }
}