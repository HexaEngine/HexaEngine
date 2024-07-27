using System.Buffers.Binary;

namespace HexaEngine.Network.Protocol
{
    public unsafe struct Container
    {
        public ProtocolVersion Version;
        public ushort NumRecords;

        public uint SizeOf()
        {
            return 6;
        }

        public readonly int Write(Span<byte> buffer)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(buffer, (ushort)Version);
            BinaryPrimitives.WriteUInt16LittleEndian(buffer[4..], NumRecords);
            return 6;
        }

        public bool TryRead(Span<byte> buffer, out int readBytes)
        {
            if (buffer.Length < 6)
            {
                readBytes = 0;
                return false;
            }

            Version = (ProtocolVersion)BinaryPrimitives.ReadUInt32LittleEndian(buffer);
            NumRecords = BinaryPrimitives.ReadUInt16LittleEndian(buffer[4..]);

            readBytes = 6;
            return true;
        }

        public bool TryRead(Memory<byte> buffer, int start, int length, out int readBytes)
        {
            if (length < 6)
            {
                readBytes = 0;
                return false;
            }

            Span<byte> span = buffer.Span.Slice(start, length);

            Version = (ProtocolVersion)BinaryPrimitives.ReadUInt32LittleEndian(span);
            NumRecords = BinaryPrimitives.ReadUInt16LittleEndian(span[4..]);

            readBytes = 6;
            return true;
        }

        public void Free()
        {
        }
    }
}