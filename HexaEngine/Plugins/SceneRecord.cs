namespace HexaEngine.Plugins
{
    using HexaEngine.Core.Unsafes;

    public unsafe struct SceneRecord : IRecord
    {
        public UnsafeString Name;

        public void Decode(Span<byte> src, Endianness endianness)
        {
            fixed (SceneRecord* @this = &this)
            {
                UnsafeString.Read(&@this->Name, endianness, src);
            }
        }

        public void Encode(Span<byte> dest, Endianness endianness)
        {
            fixed (SceneRecord* @this = &this)
            {
                UnsafeString.Write(&@this->Name, endianness, dest);
            }
        }

        public int Size()
        {
            return Name.Sizeof();
        }
    }
}