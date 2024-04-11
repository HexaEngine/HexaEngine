namespace HexaEngine.Core.Assets
{
    using System.Diagnostics.CodeAnalysis;

    public class CharComparerIgnoreCase : IEqualityComparer<char>
    {
        public static readonly CharComparerIgnoreCase Instance = new();

        public bool Equals(char x, char y)
        {
            return char.ToLowerInvariant(x) - char.ToLowerInvariant(y) == 0;
        }

        public int GetHashCode([DisallowNull] char obj)
        {
            return char.ToLowerInvariant(obj).GetHashCode();
        }
    }
}