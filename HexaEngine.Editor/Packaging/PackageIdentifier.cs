namespace HexaEngine.Editor.Packaging
{
    using HexaEngine.Core.IO;

    public struct PackageIdentifier : IEquatable<PackageIdentifier>
    {
        public string Id;
        public Version Version;

        public PackageIdentifier(string id, Version version)
        {
            Id = id;
            Version = version;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is PackageIdentifier identifier && Equals(identifier);
        }

        public readonly bool Equals(PackageIdentifier other)
        {
            return Id == other.Id &&
                   Version.Equals(other.Version);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Id, Version);
        }

        public static bool operator ==(PackageIdentifier left, PackageIdentifier right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PackageIdentifier left, PackageIdentifier right)
        {
            return !(left == right);
        }

        public override readonly string ToString()
        {
            return $"{Id}, {Version}";
        }
    }
}