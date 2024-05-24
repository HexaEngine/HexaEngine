namespace HexaEngine.Network.Protocol
{
    using System;
    using System.Buffers.Binary;
    using System.Runtime.CompilerServices;

    public struct TestPayload : IRecord
    {
        [InlineArray(8192)]
        public struct Buffer
        {
            private ulong _element0;
        }

        public Buffer Payload;

        public RecordType Type => RecordType.TestPayload;

        public readonly int Write(Span<byte> span)
        {
            int idx = 0;
            for (var i = 0; i < 8192; i++)
            {
                BinaryPrimitives.WriteUInt64LittleEndian(span[idx..], Payload[i]);
                idx += 8;
            }
            return idx;
        }

        public int Read(ReadOnlySpan<byte> span)
        {
            int idx = 0;
            for (var i = 0; i < 8192; i++)
            {
                Payload[i] = BinaryPrimitives.ReadUInt64LittleEndian(span[idx..]);
                idx += 8;
            }
            return idx;
        }

        public readonly int SizeOf()
        {
            return 8 * 8192;
        }

        public void Free()
        {
        }
    }
}