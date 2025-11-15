namespace HexaEngine.Core.IO
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public class PathComparer : IEqualityComparer<ReadOnlySpan<char>>, IEqualityComparer<string>, IAlternateEqualityComparer<ReadOnlySpan<char>, string>
    {
        public static readonly PathComparer Instance = new();

        public string Create(ReadOnlySpan<char> alternate)
        {
            return alternate.ToString();
        }

        public bool Equals(ReadOnlySpan<char> x, ReadOnlySpan<char> y)
        {
            if (x.Length != y.Length) return false;
            for (int i = 0; i < x.Length; ++i)
            {
                var a = x[i];
                var b = y[i];
                if (a == '\\') a = '/';
                if (b == '\\') b = '/';
                if (a != b) return false;
            }

            return true;
        }

        public bool Equals(ReadOnlySpan<char> alternate, string other)
        {
            return Equals(alternate, other.AsSpan());
        }

        public bool Equals(string? x, string? y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return Equals(x.AsSpan(), y.AsSpan());
        }

        public int GetHashCode([DisallowNull] string obj)
        {
            return GetHashCode(obj.AsSpan());
        }

        public int GetHashCode(ReadOnlySpan<char> obj)
        {
            const ulong prime = 1099511628211;
            const ulong offsetBasis = 14695981039346656037;
            ulong hash = offsetBasis;
            for (int i = 0; i < obj.Length; ++i)
            {
                var c = obj[i];
                if (c == '\\') c = '/';
                hash ^= c;
                hash *= prime;
            }
            return (int)hash;
        }
    }
}