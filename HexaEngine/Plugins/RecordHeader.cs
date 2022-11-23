namespace HexaEngine.Plugins
{
    public unsafe struct RecordHeader
    {
        public ulong Id;
        public ulong IdParent;
        public Record* Parent;
        public RecordType Type;
        public uint Length;
    }
}