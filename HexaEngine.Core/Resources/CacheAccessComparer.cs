namespace HexaEngine.Core.Resources
{
    using System.Collections.Generic;

    public class CacheAccessComparer : IComparer<CacheHandle>
    {
        public static readonly CacheAccessComparer Instance = new();

        public int Compare(CacheHandle? x, CacheHandle? y)
        {
            // Sortiere in aufsteigender Reihenfolge nach LastAccess.
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