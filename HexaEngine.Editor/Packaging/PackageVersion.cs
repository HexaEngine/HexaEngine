namespace HexaEngine.Editor.Packaging
{
    using System.Runtime.CompilerServices;

    public struct PackageVersion : IEquatable<PackageVersion>
    {
        public int Major;
        public int Minor;
        public int Patch;
        public int Revision;

        public PackageVersion(int major, int minor, int patch, int revision)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Revision = revision;
        }

        public static PackageVersion Parse(string s)
        {
            Unsafe.SkipInit(out PackageVersion version);
            int start = 0;
            int index = 0;

            if (s == "*")
            {
                return version;
            }

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '.')
                {
                    if (index >= 4)
                    {
                        throw new FormatException();
                    }

                    ReadOnlySpan<char> chars = s.AsSpan(start, i - start);
                    var value = int.Parse(chars);
                    unsafe
                    {
                        ((int*)&version)[index++] = value;
                    }
                    start = i + 1;
                }
            }

            return version;
        }

        public static bool TryParse(string s, out PackageVersion version)
        {
            Unsafe.SkipInit(out PackageVersion version1);
            int start = 0;
            int index = 0;
            if (s == "*")
            {
                version = default;
                return true;
            }

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '.')
                {
                    if (index >= 4)
                    {
                        version = default;
                        return false;
                    }

                    ReadOnlySpan<char> chars = s.AsSpan(start, i - start);
                    if (!int.TryParse(chars, out int value))
                    {
                        version = default;
                        return false;
                    }
                    unsafe
                    {
                        ((int*)&version1)[index++] = value;
                    }
                    start = i + 1;
                }
            }

            version = version1;

            return true;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is PackageVersion version && Equals(version);
        }

        public readonly bool Equals(PackageVersion other)
        {
            return Major == other.Major &&
                   Minor == other.Minor &&
                   Patch == other.Patch &&
                   Revision == other.Revision;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Major, Minor, Patch, Revision);
        }

        public static bool operator ==(PackageVersion left, PackageVersion right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PackageVersion left, PackageVersion right)
        {
            return !(left == right);
        }

        public override readonly string ToString()
        {
            return $"{Major}.{Minor}.{Patch}.{Revision}";
        }
    }
}