namespace HexaEngine.Core.IO
{
    using System;

    public struct AssetPath : IEquatable<AssetPath>
    {
        public string Raw;
        public int NamespaceSeparatorIndex;

        public AssetPath(string path)
        {
            Raw = path;
            NamespaceSeparatorIndex = path.IndexOf(':');
            if (Namespace.Length == 1)
            {
                NamespaceSeparatorIndex = -1;
            }
        }

        public readonly ReadOnlySpan<char> Path => NamespaceSeparatorIndex >= 0 ? Raw.AsSpan(NamespaceSeparatorIndex + 1) : Raw.AsSpan();

        public readonly ReadOnlySpan<char> Namespace => NamespaceSeparatorIndex >= 0 ? Raw.AsSpan(0, NamespaceSeparatorIndex) : [];

        public readonly bool HasNamespace => NamespaceSeparatorIndex >= 0;

        public readonly override bool Equals(object? obj)
        {
            return obj is AssetPath path && Equals(path);
        }

        public readonly bool Equals(AssetPath other)
        {
            return Raw == other.Raw;
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(Raw);
        }

        public static bool operator ==(AssetPath left, AssetPath right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AssetPath left, AssetPath right)
        {
            return !(left == right);
        }

        public static implicit operator AssetPath(string path) => new(path);
    }
}