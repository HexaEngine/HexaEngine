namespace HexaEngine.Editor.Widgets.AssetBrowser
{
    using System.Collections.Generic;

    public readonly struct AssetItemGroupComparer(HashSet<Guid> groups) : IComparer<AssetItem>
    {
        private readonly HashSet<Guid> groups = groups;

        public int Compare(AssetItem x, AssetItem y)
        {
            var parentA = x.Metadata?.GroupGuid ?? default;
            var parentB = y.Metadata?.GroupGuid ?? default;

            if (parentA == default && parentB == default)
            {
                var a = groups.Contains(x.Metadata?.Guid ?? default);
                var b = groups.Contains(y.Metadata?.Guid ?? default);
                if (a && b)
                {
                    return x.Name.CompareTo(y.Name);
                }

                if (a)
                {
                    return -1;
                }

                if (b)
                {
                    return 1;
                }

                return x.Name.CompareTo(y.Name);
            }

            if (parentA != default && parentB != default)
            {
                return x.Name.CompareTo(y.Name);
            }

            if (parentA == default)
            {
                return -1;
            }

            return 1;
        }
    }
}