namespace HexaEngine.Network
{
    public unsafe struct PayloadBufferSegment : IEquatable<PayloadBufferSegment>
    {
        public byte* Buffer;
        public int Start;
        public int Length;

        public static PayloadBufferSegment Invalid => new(null, -1, 0);

        public PayloadBufferSegment(byte* buffer, int start, int length)
        {
            Buffer = buffer;
            Start = start;
            Length = length;
        }

        public readonly Span<byte> AsSpan()
        {
            return new Span<byte>(Buffer + Start, Length);
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is PayloadBufferSegment segment && Equals(segment);
        }

        public readonly bool Equals(PayloadBufferSegment other)
        {
            return Buffer == other.Buffer &&
                   Start == other.Start &&
                   Length == other.Length;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine((nint)Buffer, Start, Length);
        }

        public static implicit operator Span<byte>(PayloadBufferSegment segment)
        {
            return segment.AsSpan();
        }

        public static bool operator ==(PayloadBufferSegment left, PayloadBufferSegment right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PayloadBufferSegment left, PayloadBufferSegment right)
        {
            return !(left == right);
        }
    }
}