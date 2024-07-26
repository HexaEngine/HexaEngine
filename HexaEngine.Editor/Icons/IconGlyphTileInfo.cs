namespace HexaEngine.Editor.Icons
{
    using Hexa.NET.Mathematics;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;

    public struct IconGlyphTileInfo : IEquatable<IconGlyphTileInfo>
    {
        public Guid Key;
        public Point2 Pos;
        public Point2 Size;

        public IconGlyphTileInfo(string name, Point2 pos, Point2 size)
        {
            Key = new(MD5.HashData(MemoryMarshal.AsBytes(name.AsSpan())));
            Pos = pos;
            Size = size;
        }

        public IconGlyphTileInfo(Guid key, Point2 pos, Point2 size)
        {
            Key = key;
            Pos = pos;
            Size = size;
        }

        public const int SizeInBytes = 32; // 16 Bytes + 2 * 4 Byte + 2 * 4 Bytes;

        public readonly void Write(Span<byte> bytes)
        {
            Key.TryWriteBytes(bytes[..16]);
            Pos.Write(bytes[16..], Endianness.LittleEndian);
            Size.Write(bytes[24..], Endianness.LittleEndian);
        }

        public static IconGlyphTileInfo Read(ReadOnlySpan<byte> bytes)
        {
            Guid key = new(bytes[..16]);
            Point2 pos = Point2.Read(bytes[16..], Endianness.LittleEndian);
            Point2 size = Point2.Read(bytes[24..], Endianness.LittleEndian);
            return new(key, pos, size);
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is IconGlyphTileInfo info && Equals(info);
        }

        public readonly bool Equals(IconGlyphTileInfo other)
        {
            return Key.Equals(other.Key);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Key);
        }

        public static bool operator ==(IconGlyphTileInfo left, IconGlyphTileInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(IconGlyphTileInfo left, IconGlyphTileInfo right)
        {
            return !(left == right);
        }
    }
}