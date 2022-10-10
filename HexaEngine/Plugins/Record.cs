namespace HexaEngine.Plugins
{
    public unsafe struct Record
    {
        public RecordHeader Header;
        public void* Data;
    }
}