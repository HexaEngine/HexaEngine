namespace HexaEngine.ShadingLang.Text
{
    using Hexa.NET.Utilities;
    using System;

    public unsafe struct TextSpan : IEquatable<TextSpan>
    {
        public char* Text;
        public int Start;
        public int Length;

        public TextSpan(char* text, int start, int length)
        {
            Text = text;
            Start = start;
            Length = length;
        }

        public readonly char this[int index] => Text[Start + index];

        public readonly int End => Start + Length;

        public readonly ReadOnlySpan<char> AsSpan()
        {
            return new(Text + Start, Length);
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is TextSpan span && Equals(span);
        }

        public readonly bool Equals(TextSpan other)
        {
            return Text == other.Text &&
                   Start == other.Start &&
                   Length == other.Length;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine((nint)Text, Start, Length);
        }

        public override readonly string ToString()
        {
            return AsSpan().ToString();
        }

        public readonly TextSpan Merge(TextSpan other)
        {
            if (Text != other.Text) throw new InvalidOperationException("Cannot merge TextSpan based of a different string pointer.");

            int newStart = Math.Min(Start, other.Start);
            int newEnd = Math.Max(End, other.End);
            int newLength = newEnd - newStart;

            return new TextSpan(Text, newStart, newLength);
        }

        public readonly StdWString ToStdWString()
        {
            StdWString str = new();
            str.Append(Text + Start, Length);
            return str;
        }

        public readonly bool SequenceEquals(TextSpan span)
        {
            if (span.Length != Length) return false;
            for (int i = 0; i < Length; i++)
            {
                if (span[i] != this[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool operator ==(TextSpan left, TextSpan right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TextSpan left, TextSpan right)
        {
            return !(left == right);
        }

        public static implicit operator ReadOnlySpan<char>(TextSpan span) => span.AsSpan();
    }
}