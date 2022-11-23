namespace HexaEngine.Plugins2
{
    using HexaEngine.Core.Unsafes;
    using System.Buffers.Binary;

    public unsafe struct PluginVersion
    {
        public int Major;
        public int Minor;
        public int Patch;
        public int Build;

        public static readonly PluginVersion LatestFormatVersion = new(1, 0, 0, 0);

        public PluginVersion(int major, int minor, int patch, int build)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Build = build;
        }

        public int Encode(Span<byte> dest, Endianness endianness)
        {
            if (endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteInt32LittleEndian(dest[0..], Major);
                BinaryPrimitives.WriteInt32LittleEndian(dest[4..], Minor);
                BinaryPrimitives.WriteInt32LittleEndian(dest[8..], Patch);
                BinaryPrimitives.WriteInt32LittleEndian(dest[12..], Build);
                return 16;
            }
            else if (endianness == Endianness.BigEndian)
            {
                BinaryPrimitives.WriteInt32BigEndian(dest[0..], Major);
                BinaryPrimitives.WriteInt32BigEndian(dest[4..], Minor);
                BinaryPrimitives.WriteInt32BigEndian(dest[8..], Patch);
                BinaryPrimitives.WriteInt32BigEndian(dest[12..], Build);
                return 16;
            }
            else
            {
                return 0;
            }
        }

        public int Decode(Span<byte> src, Endianness endianness)
        {
            if (endianness == Endianness.LittleEndian)
            {
                Major = BinaryPrimitives.ReadInt32LittleEndian(src[0..]);
                Minor = BinaryPrimitives.ReadInt32LittleEndian(src[4..]);
                Patch = BinaryPrimitives.ReadInt32LittleEndian(src[8..]);
                Build = BinaryPrimitives.ReadInt32LittleEndian(src[12..]);
                return 16;
            }
            else if (endianness == Endianness.BigEndian)
            {
                Major = BinaryPrimitives.ReadInt32BigEndian(src[0..]);
                Minor = BinaryPrimitives.ReadInt32BigEndian(src[4..]);
                Patch = BinaryPrimitives.ReadInt32BigEndian(src[8..]);
                Build = BinaryPrimitives.ReadInt32BigEndian(src[12..]);
                return 16;
            }
            else
            {
                return 0;
            }
        }

        public int Size() => 16;

        public override string ToString()
        {
            return $"{Major}.{Minor}.{Patch}.{Build}";
        }
    }
}