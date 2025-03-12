namespace HexaEngine.Editor.Widgets.AssetBrowser
{
    using System.Collections.Generic;

    public struct AssetDirectory(string name, string path) : IEquatable<AssetDirectory>
    {
        public string Path = path;
        public string Name = name;
        public string UIName = $"{name}##{path}";
        public List<AssetDirectory> SubDirs = [];

        public override readonly bool Equals(object? obj)
        {
            return obj is AssetDirectory directory && Equals(directory);
        }

        public readonly bool Equals(AssetDirectory other)
        {
            return Path == other.Path;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Path);
        }

        public static bool operator ==(AssetDirectory left, AssetDirectory right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AssetDirectory left, AssetDirectory right)
        {
            return !(left == right);
        }
    }
}