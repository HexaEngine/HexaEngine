namespace HexaEngine.Plugins.Records
{
    using HexaEngine.Core;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Plugins;

    public unsafe struct SceneRecord : IRecord
    {
        public RecordHeader* Header;
        public char* Name;

        public int Decode(Span<byte> src, Endianness endianness)
        {
            fixed (char** @this = &Name)
            {
                return Utilities.ReadString(src, endianness, @this);
               
            }
        }

        public int Encode(Span<byte> dest, Endianness endianness)
        {
            
                return Utilities.WriteString(dest, endianness, Name);
            
        }

        public void Overwrite(void* pRecord)
        {
            throw new NotImplementedException();
        }

        public int Size()
        {
            return Utilities.StringSizeNullTerminated(Name);
        }
    }
}