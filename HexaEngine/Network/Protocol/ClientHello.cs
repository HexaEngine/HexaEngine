namespace HexaEngine.Network.Protocol
{
    using System;
    using System.Buffers.Binary;
    using System.Text;

    public struct ClientHello : IRecord
    {
        public ulong GameVersion;
        public TimeSpan LocalTimeOffset;

        public ClientHello(ulong gameVersion, TimeSpan localTimeOffset)
        {
            GameVersion = gameVersion;
            LocalTimeOffset = localTimeOffset;
        }

        public readonly RecordType Type => RecordType.ClientHello;

        public readonly int Write(Span<byte> span)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(span[0..], GameVersion);
            BinaryPrimitives.WriteInt64LittleEndian(span[8..], LocalTimeOffset.Ticks);
            return 16;
        }

        public int Read(ReadOnlySpan<byte> span)
        {
            GameVersion = BinaryPrimitives.ReadUInt64LittleEndian(span[0..]);
            LocalTimeOffset = new(BinaryPrimitives.ReadInt64LittleEndian(span[8..]));
            return 16;
        }

        public readonly int SizeOf()
        {
            return 16;
        }

        public void Free()
        {
        }
    }
}