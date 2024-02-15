namespace HexaEngine.Core.IO.Binary.Archives
{
    using System.IO;

    /// <summary>
    /// Represents a description of an asset, including its path, type, and associated stream.
    /// </summary>
    public readonly struct AssetDesc
    {
        /// <summary>
        /// Gets the path of the asset.
        /// </summary>
        public readonly string Path;

        /// <summary>
        /// Gets the type of the asset.
        /// </summary>
        public readonly AssetType Type;

        /// <summary>
        /// Gets the stream associated with the asset.
        /// </summary>
        public readonly Stream Stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetDesc"/> struct.
        /// </summary>
        /// <param name="path">The path of the asset.</param>
        /// <param name="type">The type of the asset.</param>
        /// <param name="stream">The stream associated with the asset.</param>
        public AssetDesc(string path, AssetType type, Stream stream)
        {
            Path = path;
            Type = type;
            Stream = stream;
        }

        /// <summary>
        /// Creates an array of <see cref="AssetDesc"/> instances from an array of file paths.
        /// </summary>
        /// <param name="root">The root directory.</param>
        /// <param name="paths">An array of file paths.</param>
        /// <returns>An array of <see cref="AssetDesc"/> instances.</returns>
        public static AssetDesc[] CreateFromPaths(string root, string[] paths)
        {
            AssetDesc[] descs = new AssetDesc[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                var file = paths[i];
                var path = System.IO.Path.GetRelativePath(root, file);
                var type = AssetArchive.GetAssetType(path, out string _);
                var fs = File.OpenRead(file);
                descs[i] = new(path, type, fs);
            }
            return descs;
        }

        /// <summary>
        /// Creates an array of <see cref="AssetDesc"/> instances from a directory.
        /// </summary>
        /// <param name="root">The root directory.</param>
        /// <returns>An array of <see cref="AssetDesc"/> instances.</returns>
        public static AssetDesc[] CreateFromDir(string root)
        {
            var dir = new DirectoryInfo(root);
            List<string> files = new();
            foreach (var file in dir.GetFiles("*.*", SearchOption.AllDirectories))
            {
                if (file.Extension == ".assets")
                {
                    continue;
                }

                if (file.Extension == ".dll")
                {
                    continue;
                }

                if (file.Extension == ".hexlvl")
                {
                    continue;
                }

                files.Add(file.FullName);
            }

            AssetDesc[] descs = new AssetDesc[files.Count];

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                var path = System.IO.Path.GetRelativePath(root, file);
                var type = AssetArchive.GetAssetType(path, out string _);
                var fs = File.OpenRead(file);
                descs[i] = new(path, type, fs);
            }

            return descs;
        }

        /// <summary>
        /// Implicitly converts a <see cref="BundleAsset"/> to an <see cref="AssetDesc"/>.
        /// </summary>
        /// <param name="asset">The <see cref="BundleAsset"/> to convert.</param>
        /// <returns>An <see cref="AssetDesc"/> representing the same asset.</returns>
        public static implicit operator AssetDesc(BundleAsset asset)
        {
            return new(asset.Path, asset.Type, asset.GetStream());
        }
    }
}