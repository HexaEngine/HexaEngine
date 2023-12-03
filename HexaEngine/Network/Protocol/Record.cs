namespace HexaEngine.Network.Protocol
{
    public enum ProtocolVersion
    {
        Version1
    }

    public unsafe struct Container
    {
        public ProtocolVersion Version;
        public ushort NumRecords;
        public Record* Records;
    }

    public enum RecordType : uint
    {
        Protocol,
        Scene,
        Physics,
        Input,
        User,
    }

    public unsafe struct Record
    {
        public RecordType Type;
        public uint Length;
        public byte* Payload;
    }
}