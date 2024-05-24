namespace HexaEngine.Network.Protocol
{
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Input;
    using System;
    using System.Buffers.Binary;
    using System.Text;

    public struct ClientInput : IRecord
    {
        public StdWString Axis;
        public float Value;
        public VirtualAxisStateFlags Flags;

        public readonly RecordType Type => RecordType.ClientInput;

        public readonly int Write(Span<byte> span)
        {
            int AxisLen = Encoding.UTF8.GetByteCount(Axis.AsSpan());
            BinaryPrimitives.WriteInt32LittleEndian(span[0..], AxisLen);
            int idx = 4;
            Encoding.UTF8.GetBytes(Axis.AsSpan(), span[idx..]);
            idx += AxisLen;
            BinaryPrimitives.WriteSingleLittleEndian(span[idx..], Value);
            idx += 4;
            span[idx] = (byte)Flags;
            idx += 1;
            return idx;
        }

        public int Read(ReadOnlySpan<byte> span)
        {
            int AxisLen = BinaryPrimitives.ReadInt32LittleEndian(span[0..]);
            int idx = 4;
            Axis = new StdWString(AxisLen);
            Axis.Resize(AxisLen);
            Encoding.UTF8.TryGetChars(span.Slice(idx, AxisLen), Axis.AsSpan(), out _);
            idx += AxisLen;
            Value = BinaryPrimitives.ReadSingleLittleEndian(span[idx..]);
            idx += 4;
            Flags = (VirtualAxisStateFlags)span[idx];
            idx += 1;
            return idx;
        }

        public readonly int SizeOf()
        {
            int idx = 4;
            idx += Encoding.UTF8.GetByteCount(Axis.AsSpan());
            idx += 4;
            idx += 1;
            return idx;
        }

        public void Free()
        {
            Axis.Release();
            Axis = default;
        }
    }
}