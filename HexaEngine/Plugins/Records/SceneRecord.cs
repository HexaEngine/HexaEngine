namespace HexaEngine.Plugins.Records
{
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Plugins;

    public unsafe struct SceneRecord : IRecord
    {
        public RecordHeader* Header;
        public UnsafeString* Name;

        public int Decode(Span<byte> src, Endianness endianness)
        {
            fixed (SceneRecord* @this = &this)
            {
                return UnsafeString.Read(&@this->Name, endianness, src);
            }
        }

        public int Encode(Span<byte> dest, Endianness endianness)
        {
            fixed (SceneRecord* @this = &this)
            {
                return UnsafeString.Write(@this->Name, endianness, dest);
            }
        }

        public void Overwrite(void* pRecord)
        {
            throw new NotImplementedException();
        }

        public int Size()
        {
            return Name->Sizeof();
        }
    }
}