namespace HexaEngine.Editor.Widgets.AssetBrowser
{
    using HexaEngine.Core.Assets;

    public struct AssetFileInfo : IEquatable<AssetFileInfo>
    {
        public string Path;
        public string Name;
        public SourceAssetMetadata? Metadata;

        public AssetFileInfo(string file, SourceAssetMetadata? metadata)
        {
            Path = file;
            Name = System.IO.Path.GetFileName(file);
            Metadata = metadata;
        }

        public AssetFileInfo(string file, string name, SourceAssetMetadata? metadata)
        {
            Path = file;
            Name = name;
            Metadata = metadata;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is AssetFileInfo info && Equals(info);
        }

        public readonly bool Equals(AssetFileInfo other)
        {
            return Path == other.Path;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Path);
        }

        public static bool operator ==(AssetFileInfo left, AssetFileInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AssetFileInfo left, AssetFileInfo right)
        {
            return !(left == right);
        }
    }
}