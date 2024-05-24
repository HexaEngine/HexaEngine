namespace HexaEngine.Network.Protocol
{
    using System;
    using System.Buffers.Binary;

    public struct ProtocolError : IRecord
    {
        public ErrorCode ErrorCode;
        public ErrorSeverity Severity;

        public ProtocolError(ErrorCode errorCode, ErrorSeverity severity)
        {
            ErrorCode = errorCode;
            Severity = severity;
        }

        public readonly RecordType Type => RecordType.ProtocolError;

        public readonly int Write(Span<byte> span)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(span[0..], (uint)ErrorCode);
            BinaryPrimitives.WriteUInt16LittleEndian(span[4..], (ushort)Severity);
            return 6;
        }

        public int Read(ReadOnlySpan<byte> span)
        {
            ErrorCode = (ErrorCode)BinaryPrimitives.ReadUInt32LittleEndian(span[0..]);
            Severity = (ErrorSeverity)BinaryPrimitives.ReadUInt16LittleEndian(span[4..]);
            return 6;
        }

        public readonly int SizeOf()
        {
            return 6;
        }

        public readonly void Free()
        {
        }

        public override readonly string ToString()
        {
            return $"{Severity}: {ErrorCode}";
        }
    }
}