namespace HexaEngine.Core.IO
{
    using System.Numerics;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a version number with major, minor, patch, and build components.
    /// </summary>
    public struct Version : IEquatable<Version>
    {
        /// <summary>
        /// Gets or sets the major version component.
        /// </summary>
        public ushort Major;

        /// <summary>
        /// Gets or sets the minor version component.
        /// </summary>
        public ushort Minor;

        /// <summary>
        /// Gets or sets the patch version component.
        /// </summary>
        public ushort Patch;

        /// <summary>
        /// Gets or sets the build version component.
        /// </summary>
        public ushort Build;

        /// <summary>
        /// Initializes a new instance of the <see cref="Version"/> struct with the specified version components.
        /// </summary>
        /// <param name="major">The major version component.</param>
        /// <param name="minor">The minor version component.</param>
        /// <param name="patch">The patch version component.</param>
        /// <param name="build">The build version component.</param>
        public Version(ushort major, ushort minor, ushort patch, ushort build)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Build = build;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Version"/> struct from a combined version value.
        /// </summary>
        /// <param name="combinedVersion">The combined version value to initialize from.</param>
        public Version(ulong combinedVersion)
        {
            Build = (ushort)(combinedVersion >> 48);
            Patch = (ushort)((combinedVersion >> 32) & 0xFFFF);
            Minor = (ushort)((combinedVersion >> 16) & 0xFFFF);
            Major = (ushort)(combinedVersion & 0xFFFF);
        }

        /// <summary>
        /// Gets the combined version value of this <see cref="Version"/>.
        /// </summary>
        public readonly ulong Value => ((ulong)Build << 48) | ((ulong)Patch << 32) | ((ulong)Minor << 16) | Major;

        /// <summary>
        /// If all components are equals 0 then version is considered as any.
        /// </summary>
        public readonly bool IsAny => Value == 0;

        /// <summary>
        /// Converts a <see cref="Version"/> to an unsigned long integer.
        /// </summary>
        public static implicit operator ulong(Version version)
        {
            return version.Value;
        }

        /// <summary>
        /// Converts an unsigned long integer to a <see cref="Version"/>.
        /// </summary>
        public static implicit operator Version(ulong version)
        {
            return new(version);
        }

        /// <summary>
        /// Determines whether two <see cref="Version"/> instances are equal.
        /// </summary>
        public static bool operator ==(Version left, Version right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="Version"/> instances are not equal.
        /// </summary>
        public static bool operator !=(Version left, Version right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Returns a string representation of the <see cref="Version"/> in the format "Major.Minor.Patch.Build".
        /// </summary>
        public override readonly string ToString()
        {
            return $"{Major}.{Minor}.{Patch}.{Build}";
        }

        /// <summary>
        /// Determines whether this <see cref="Version"/> is equal to another object.
        /// </summary>
        public override readonly bool Equals(object? obj)
        {
            return obj is Version version && Equals(version);
        }

        /// <summary>
        /// Determines whether this <see cref="Version"/> is equal to another <see cref="Version"/>.
        /// </summary>
        public readonly bool Equals(Version other)
        {
            return Major == other.Major &&
                   Minor == other.Minor &&
                   Patch == other.Patch &&
                   Build == other.Build;
        }

        /// <summary>
        /// Computes a hash code for this <see cref="Version"/>.
        /// </summary>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Major, Minor, Patch, Build);
        }

        public static Version Parse(string s)
        {
            Unsafe.SkipInit(out Version version);
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
                    var value = ushort.Parse(chars);
                    unsafe
                    {
                        ((ushort*)&version)[index++] = value;
                    }
                    start = i + 1;
                }
            }

            return version;
        }

        public static bool TryParse(string s, out Version version)
        {
            Unsafe.SkipInit(out Version version1);
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
                    if (!ushort.TryParse(chars, out ushort value))
                    {
                        version = default;
                        return false;
                    }
                    unsafe
                    {
                        ((ushort*)&version1)[index++] = value;
                    }
                    start = i + 1;
                }
            }

            version = version1;

            return true;
        }
    }
}