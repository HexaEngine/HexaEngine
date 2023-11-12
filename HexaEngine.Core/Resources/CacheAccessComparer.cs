namespace HexaEngine.Core.Resources
{
    using System.Collections.Generic;

    /// <summary>
    /// Compares two <see cref="CacheHandle"/> objects based on their last access timestamps.
    /// </summary>
    public class CacheAccessComparer : IComparer<CacheHandle>
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="CacheAccessComparer"/>.
        /// </summary>
        public static readonly CacheAccessComparer Instance = new();

        /// <summary>
        /// Compares two <see cref="CacheHandle"/> objects based on their last access timestamps.
        /// </summary>
        /// <param name="x">The first <see cref="CacheHandle"/> to compare.</param>
        /// <param name="y">The second <see cref="CacheHandle"/> to compare.</param>
        /// <returns>
        /// A negative integer if <paramref name="x"/> is less recently accessed than <paramref name="y"/>.
        /// A positive integer if <paramref name="x"/> is more recently accessed than <paramref name="y"/>.
        /// Zero if <paramref name="x"/> and <paramref name="y"/> have the same last access timestamp.
        /// </returns>
        public int Compare(CacheHandle? x, CacheHandle? y)
        {
            if (x == null || y == null)
            {
                return 0;
            }

            if (x.LastAccess < y.LastAccess)
            {
                return -1;
            }
            else if (x.LastAccess > y.LastAccess)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}