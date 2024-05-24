namespace HexaEngine.Network.Protocol
{
    using System.Buffers.Binary;

    public unsafe struct Record
    {
        public RecordType Type;
        public uint Length;
        public byte* Payload;

        public static int Size => 8;

        public readonly Span<byte> AsSpan()
        {
            return new Span<byte>(Payload, (int)Length);
        }

        public static int WriteHeader(IRecord record, Span<byte> span, int size)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(span, (uint)record.Type);
            BinaryPrimitives.WriteUInt32LittleEndian(span[4..], (uint)size);
            return 8;
        }

        public bool TryRead(Span<byte> span, out uint read)
        {
            if (span.Length < 8)
            {
                read = 0;
                return false;
            }
            Type = (RecordType)BinaryPrimitives.ReadUInt32LittleEndian(span);
            Length = BinaryPrimitives.ReadUInt32LittleEndian(span[4..]);

            read = 8;
            return true;
        }

        public readonly Record Clone()
        {
            Record record;
            record.Type = Type;
            record.Length = Length;
            record.Payload = AllocT<byte>(Length);
            MemcpyT(Payload, record.Payload, Length);
            return record;
        }

        public void Release()
        {
            if (Payload != null)
            {
                Free(Payload);
                Payload = null;
                Length = 0;
            }
        }
    }
}