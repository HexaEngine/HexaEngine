namespace HexaEngine.Editor.Widgets.AssetBrowser
{
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Graphics;
    using System.Collections.Generic;

    public struct AssetItem : IEquatable<AssetItem>
    {
        public AssetItem(string name, string path, SourceAssetMetadata? metadata, Ref<Texture2D>? thumbnail)
        {
            Path = path;
            Name = name;
            NameNoExtension = System.IO.Path.GetFileNameWithoutExtension(path);
            Metadata = metadata;
            Thumbnail = thumbnail;
        }

        public AssetItem(string path)
        {
            Path = path;
        }

        public string Path;
        public string Name;
        public List<AssetItem> GroupItems = [];
        public string NameNoExtension;
        public SourceAssetMetadata? Metadata;
        public readonly Ref<Texture2D>? Thumbnail;

        public override readonly bool Equals(object? obj) => obj is AssetItem item && Equals(item);

        public readonly bool Equals(AssetItem other) => Path == other.Path;

        public override readonly int GetHashCode() => HashCode.Combine(Path);

        public static bool operator ==(AssetItem left, AssetItem right) => left.Equals(right);

        public static bool operator !=(AssetItem left, AssetItem right) => !(left == right);

        public static implicit operator AssetFileInfo(AssetItem item) => new(item.Path, item.Metadata);
    }
}