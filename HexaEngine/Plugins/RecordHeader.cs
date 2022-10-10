namespace HexaEngine.Plugins
{
    using HexaEngine.Core.Unsafes;
    using System.Buffers.Binary;

    public unsafe struct RecordHeader : IRecord
    {
        public ulong Id;
        public ulong ParentId;
        public RecordType Type;
        public int Length;

        public void Decode(Span<byte> src, Endianness endianness)
        {
            fixed (RecordHeader* @this = &this)
            {
                if (endianness == Endianness.LittleEndian)
                {
                    Id = BinaryPrimitives.ReadUInt64LittleEndian(src);
                    ParentId = BinaryPrimitives.ReadUInt64LittleEndian(src[8..]);
                    Type = (RecordType)BinaryPrimitives.ReadInt32LittleEndian(src[16..]);
                    Length = BinaryPrimitives.ReadInt32LittleEndian(src[20..]);
                }
                else if (endianness == Endianness.BigEndian)
                {
                    Id = BinaryPrimitives.ReadUInt64BigEndian(src);
                    ParentId = BinaryPrimitives.ReadUInt64BigEndian(src[8..]);
                    Type = (RecordType)BinaryPrimitives.ReadInt32BigEndian(src[16..]);
                    Length = BinaryPrimitives.ReadInt32BigEndian(src[20..]);
                }
            }
        }

        public void Encode(Span<byte> dst, Endianness endianness)
        {
            fixed (RecordHeader* @this = &this)
            {
                if (endianness == Endianness.LittleEndian)
                {
                    BinaryPrimitives.WriteUInt64LittleEndian(dst, Id);
                    BinaryPrimitives.WriteUInt64LittleEndian(dst[8..], ParentId);
                    BinaryPrimitives.WriteInt32LittleEndian(dst[16..], (int)Type);
                    BinaryPrimitives.WriteInt32LittleEndian(dst[20..], Length);
                }
                else if (endianness == Endianness.BigEndian)
                {
                    BinaryPrimitives.WriteUInt64BigEndian(dst, Id);
                    BinaryPrimitives.WriteUInt64BigEndian(dst[8..], ParentId);
                    BinaryPrimitives.WriteInt32BigEndian(dst[16..], (int)Type);
                    BinaryPrimitives.WriteInt32BigEndian(dst[20..], Length);
                }
            }
        }

        public int Size()
        {
            return sizeof(RecordHeader);
        }
    }
}