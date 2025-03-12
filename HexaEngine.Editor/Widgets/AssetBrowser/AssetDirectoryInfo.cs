namespace HexaEngine.Editor.Widgets.AssetBrowser
{
    using HexaEngine.Core.Assets;

    public struct AssetDirectoryInfo : IEquatable<AssetDirectoryInfo>
    {
        public string Path;
        public SourceAssetMetadata? Metadata;

        public AssetDirectoryInfo(string path, SourceAssetMetadata? metadata)
        {
            Path = path;
            Metadata = metadata;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is AssetDirectoryInfo info && Equals(info);
        }

        public readonly bool Equals(AssetDirectoryInfo other)
        {
            return Path == other.Path;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Path);
        }

        public static bool operator ==(AssetDirectoryInfo left, AssetDirectoryInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AssetDirectoryInfo left, AssetDirectoryInfo right)
        {
            return !(left == right);
        }
    }
}