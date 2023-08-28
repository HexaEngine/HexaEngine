namespace HexaEngine.Core.IO
{
    public struct Version : IEquatable<Version>
    {
        public ushort Major;
        public ushort Minor;
        public ushort Patch;
        public ushort Build;

        public Version(ushort major, ushort minor, ushort patch, ushort build)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Build = build;
        }

        public Version(ulong combinedVersion)
        {
            Build = (ushort)(combinedVersion >> 48);
            Patch = (ushort)((combinedVersion >> 32) & 0xFFFF);
            Minor = (ushort)((combinedVersion >> 16) & 0xFFFF);
            Major = (ushort)(combinedVersion & 0xFFFF);
        }

        public readonly ulong Value => ((ulong)Build << 48) | ((ulong)Patch << 32) | ((ulong)Minor << 16) | Major;

        public static implicit operator ulong(Version version)
        {
            return version.Value;
        }

        public static implicit operator Version(ulong version)
        {
            return new(version);
        }

        public static bool operator ==(Version left, Version right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Version left, Version right)
        {
            return !(left == right);
        }

        public override readonly string ToString()
        {
            return $"{Major}.{Minor}.{Patch}.{Build}";
        }

        public override bool Equals(object? obj)
        {
            return obj is Version version && Equals(version);
        }

        public readonly bool Equals(Version other)
        {
            return Major == other.Major &&
                   Minor == other.Minor &&
                   Patch == other.Patch &&
                   Build == other.Build;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Major, Minor, Patch, Build);
        }
    }
}