namespace HexaEngine.Core.IO.Archives
{
    using HexaEngine.Core.IO;
    using K4os.Compression.LZ4;
    using K4os.Compression.LZ4.Streams;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Text;

    /// <summary>
    /// Represents an archive containing assets, providing methods to load, save, and extract assets.
    /// </summary>
    public class AssetArchive
    {
        private readonly Stream stream;
        private readonly AssetArchiveHeader header;
        private readonly List<BundleAsset> assets;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetArchive"/> class from the specified file path.
        /// </summary>
        /// <param name="path">The path to the asset archive file.</param>
        public AssetArchive(string path)
        {
            var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            header = new();
            header.Read(fs, Encoding.UTF8);
            stream = fs;

            var basePath = Path.GetDirectoryName(path);

            if (basePath == null)
            {
                throw new InvalidDataException("Bad archive, couldn't get the base path of the archive");
            }

            if (header.Entries == null)
            {
                throw new InvalidDataException("Bad archive, Header.Entires was null");
            }

            if (header.Parts == null)
            {
                throw new InvalidDataException("Bad archive, Header.Parts was null");
            }

            assets = new(header.Entries.Length);
            for (int i = 0; i < assets.Count; i++)
            {
                var entry = header.Entries[i];
                var archivePath = path;

                if ((header.Flags & AssetArchiveFlags.Partial) != 0)
                {
                    archivePath = Path.Combine(basePath, header.Parts[entry.PartIndex]);
                }

                assets[i] = new BundleAsset(archivePath, header.Compression, entry.PartIndex, entry.Type, entry.Start + header.ContentStart, entry.Length, entry.ActualLength, entry.Path);
            }
        }

        /// <summary>
        /// Gets an list of <see cref="BundleAsset"/> representing the assets in the archive.
        /// </summary>
        public List<BundleAsset> Assets => assets;

        /// <summary>
        /// Gets the stream associated with the archive.
        /// </summary>
        /// <returns>The stream associated with the archive.</returns>
        public Stream GetStream() => stream;

        /// <summary>
        /// Saves the asset archive to the specified path with optional compression.
        /// </summary>
        /// <param name="path">The path where the asset archive will be saved.</param>
        /// <param name="compression">The compression algorithm to use.</param>
        /// <param name="level">The compression level to apply.</param>
        public void Save(string path, Compression compression = Compression.None, CompressionLevel level = CompressionLevel.Optimal)
        {
            var fs = File.OpenWrite(path);
            var headerLength = header.Size(Encoding.UTF8);
            fs.Position = headerLength;

            long position = 0;
            for (int i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                var entry = header.Entries[i];
                entry.Start = position;

                switch (compression)
                {
                    case Compression.None:
                        asset.CopyTo(fs);
                        entry.Length = entry.ActualLength = asset.Length;
                        break;

                    case Compression.Deflate:
                        {
                            var buffer = DeflateCompress(asset.GetStream(), level);
                            entry.Length = buffer.Length;
                            entry.ActualLength = asset.Length;
                            fs.Write(buffer);
                        }
                        break;

                    case Compression.LZ4:
                        {
                            var buffer = LZ4Compress(asset.GetStream(), level);
                            entry.Length = buffer.Length;
                            entry.ActualLength = asset.Length;
                            fs.Write(buffer);
                        }
                        break;
                }

                position += entry.Length;
                header.Entries[i] = entry;
            }

            fs.Position = 0;
            header.Write(fs, Encoding.UTF8);

            fs.Flush();
            fs.Close();
        }

        /// <summary>
        /// Extracts the assets from the archive to the specified directory.
        /// </summary>
        /// <param name="path">The directory where the assets will be extracted.</param>
        public void Extract(string path)
        {
            var root = new DirectoryInfo(path);
            foreach (BundleAsset asset in assets)
            {
#pragma warning disable CS8604 // Possible null reference argument for parameter 'path' in 'DirectoryInfo Directory.CreateDirectory(string path)'.
                Directory.CreateDirectory(Path.GetDirectoryName(root.FullName + asset.Path));
#pragma warning restore CS8604 // Possible null reference argument for parameter 'path' in 'DirectoryInfo Directory.CreateDirectory(string path)'.
                var fs = File.Create(root.FullName + asset.Path);
                fs.Write(asset.GetData());
                fs.Flush();
                fs.Close();
                fs.Dispose();
            }
        }

        /// <summary>
        /// Creates an asset archive from the specified list of <see cref="AssetDesc"/> objects.
        /// </summary>
        /// <param name="assets">The list of asset descriptions.</param>
        /// <param name="output">The path where the asset archive will be saved.</param>
        /// <param name="compression">The compression algorithm to use.</param>
        /// <param name="level">The compression level to apply.</param>
        public static void CreateFrom(AssetDesc[] assets, string output, Compression compression = Compression.None, CompressionLevel level = CompressionLevel.Optimal)
        {
            var fs = File.Create(output);
            var filename = Path.GetFileName(output);

            AssetArchiveHeader header = default;
            header.Compression = compression;
            header.Flags = AssetArchiveFlags.Normal;
            header.Parts = Array.Empty<string>();
            header.Entries = new AssetArchiveHeaderEntry[assets.Length];

            for (int i = 0; i < assets.Length; i++)
            {
                var file = assets[i];
                header.Entries[i].Path = file.Path; // Path.GetRelativePath(dir.FullName, file.FullName);
            }

            var headerLength = header.Size(Encoding.UTF8);
            fs.Position = headerLength;

            long position = 0;
            for (int i = 0; i < assets.Length; i++)
            {
                var file = assets[i];
                var ts = file.Stream;
                var entry = header.Entries[i];
                var type = file.Type;

                entry.Type = type;
                entry.Start = position;

                Debug.WriteLine($"Packing {filename} <-- [{type}]");

                switch (compression)
                {
                    case Compression.None:
                        ts.CopyTo(fs);
                        entry.Length = entry.ActualLength = ts.Length;
                        break;

                    case Compression.Deflate:
                        {
                            var buffer = DeflateCompress(ts, level);
                            entry.ActualLength = ts.Length;
                            entry.Length = buffer.Length;
                            fs.Write(buffer);
                        }
                        break;

                    case Compression.LZ4:
                        {
                            var buffer = LZ4Compress(ts, level);
                            entry.ActualLength = ts.Length;
                            entry.Length = buffer.Length;
                            fs.Write(buffer);
                        }
                        break;
                }

                ts.Close();

                position += entry.Length;
                header.Entries[i] = entry;
            }

            fs.Position = 0;
            header.Write(fs, Encoding.UTF8);

            fs.Flush();
            fs.Close();
        }

        /// <summary>
        /// Creates an asset archive from the assets in the specified directory.
        /// </summary>
        /// <param name="path">The path to the directory containing the assets.</param>
        /// <param name="output">The path where the asset archive will be saved.</param>
        /// <param name="compression">The compression algorithm to use.</param>
        /// <param name="level">The compression level to apply.</param>
        public static void CreateFrom(string path, string output, Compression compression = Compression.None, CompressionLevel level = CompressionLevel.Optimal)
        {
            var dir = new DirectoryInfo(path);

            List<FileInfo> files = new();
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

                files.Add(file);
            }

            var fs = File.Create(output);
            var filename = Path.GetFileName(output);

            AssetArchiveHeader header = default;
            header.Compression = compression;
            header.Flags = AssetArchiveFlags.Normal;
            header.Parts = Array.Empty<string>();
            header.Entries = new AssetArchiveHeaderEntry[files.Count];

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                header.Entries[i].Path = Path.GetRelativePath(dir.FullName, file.FullName);
            }

            var headerLength = header.Size(Encoding.UTF8);
            fs.Position = headerLength;

            long position = 0;
            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                var ts = file.OpenRead();
                var entry = header.Entries[i];
                var type = GetAssetType(entry.Path, out string relTrim);

                entry.Type = type;
                entry.Start = position;

                Debug.WriteLine($"Packing {filename} <-- [{type}] {relTrim}");

                switch (compression)
                {
                    case Compression.None:
                        ts.CopyTo(fs);
                        entry.Length = entry.ActualLength = ts.Length;
                        break;

                    case Compression.Deflate:
                        {
                            var buffer = DeflateCompress(ts, level);
                            entry.ActualLength = ts.Length;
                            entry.Length = buffer.Length;
                            fs.Write(buffer);
                        }
                        break;

                    case Compression.LZ4:
                        {
                            var buffer = LZ4Compress(ts, level);
                            entry.ActualLength = ts.Length;
                            entry.Length = buffer.Length;
                            fs.Write(buffer);
                        }
                        break;
                }

                ts.Close();

                position += entry.Length;
                header.Entries[i] = entry;
            }

            fs.Position = 0;
            header.Write(fs, Encoding.UTF8);

            fs.Flush();
            fs.Close();
        }

        /// <summary>
        /// Generates an asset archive for each subdirectory in the specified root directory.
        /// </summary>
        /// <param name="path">The path to the root directory containing subdirectories with assets.</param>
        /// <param name="compression">The compression algorithm to use.</param>
        /// <param name="level">The compression level to apply.</param>
        public static void GenerateFrom(string path, Compression compression = Compression.None, CompressionLevel level = CompressionLevel.Optimal)
        {
            var root = new DirectoryInfo(path);

            foreach (var dir in root.GetDirectories())
            {
                List<FileInfo> files = new();
                foreach (var file in dir.GetFiles("*.*", SearchOption.AllDirectories))
                {
                    if (file.Extension == ".assets")
                    {
                        continue;
                    }

                    files.Add(file);
                }

                var filename = dir.Name + ".assets";
                var fs = File.Create(root.FullName + dir.Name + ".assets");

                AssetArchiveHeader header = default;
                header.Compression = compression;
                header.Flags = AssetArchiveFlags.Normal;
                header.Parts = Array.Empty<string>();
                header.Entries = new AssetArchiveHeaderEntry[files.Count];

                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    header.Entries[i].Path = Path.GetRelativePath(dir.FullName, file.FullName);
                }

                var headerLength = header.Size(Encoding.UTF8);
                fs.Position = headerLength;

                long position = 0;
                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    var ts = file.OpenRead();
                    var entry = header.Entries[i];
                    var type = GetAssetType(entry.Path, out string relTrim);

                    entry.Type = type;
                    entry.Start = position;

                    Console.WriteLine($"Packing {filename} <-- [{type}] {relTrim}");

                    switch (compression)
                    {
                        case Compression.None:
                            ts.CopyTo(fs);
                            entry.Length = entry.ActualLength = ts.Length;
                            break;

                        case Compression.Deflate:
                            {
                                var buffer = DeflateCompress(ts, level);
                                entry.ActualLength = ts.Length;
                                entry.Length = buffer.Length;
                                fs.Write(buffer);
                            }
                            break;

                        case Compression.LZ4:
                            {
                                var buffer = LZ4Compress(ts, level);
                                entry.ActualLength = ts.Length;
                                entry.Length = buffer.Length;
                                fs.Write(buffer);
                            }
                            break;
                    }

                    ts.Close();

                    position += entry.Length;
                    header.Entries[i] = entry;
                }

                fs.Position = 0;
                header.Write(fs, Encoding.UTF8);

                fs.Flush();
                fs.Close();
                dir.Delete(true);
            }
        }

        /// <summary>
        /// Determines the <see cref="AssetType"/> based on the specified path and returns the trimmed path.
        /// </summary>
        /// <param name="path">The path of the asset.</param>
        /// <param name="trimmed">The trimmed path.</param>
        /// <returns>The <see cref="AssetType"/> of the asset.</returns>
        public static AssetType GetAssetType(string path, out string trimmed)
        {
            if (path.Contains("textures"))
            {
                trimmed = path.Replace("textures/", string.Empty).Replace("textures\\", string.Empty);
                return AssetType.TextureFile;
            }

            if (path.Contains("meshes") && path.EndsWith(".model"))
            {
                trimmed = path.Replace("meshes/", string.Empty).Replace("meshes\\", string.Empty);
                return AssetType.ModelFile;
            }

            if (path.Contains("materials") && path.EndsWith(".matlib"))
            {
                trimmed = path.Replace("materials/", string.Empty).Replace("materials\\", string.Empty);
                return AssetType.MaterialLibrary;
            }

            if (path.Contains("shaders") && !path.EndsWith(".sb"))
            {
                trimmed = path.Replace("shaders/", string.Empty).Replace("shaders\\", string.Empty);
                return AssetType.ShaderSource;
            }

            if (path.Contains("shaders") && path.EndsWith(".sb"))
            {
                trimmed = path.Replace("shaders/", string.Empty).Replace("shaders\\", string.Empty);
                return AssetType.ShaderBytecode;
            }

            if (path.Contains("fonts"))
            {
                trimmed = path.Replace("fonts/", string.Empty).Replace("fonts\\", string.Empty);
                return AssetType.FontFile;
            }

            if (path.Contains("sounds"))
            {
                trimmed = path.Replace("sounds/", string.Empty).Replace("sounds\\", string.Empty);
                return AssetType.AudioFile;
            }

            trimmed = path;
            return AssetType.Binary;
        }

        private static byte[] DeflateCompress(Stream input, CompressionLevel level)
        {
            using var compressStream = new MemoryStream();
            using var compressor = new DeflateStream(compressStream, level);
            input.CopyTo(compressor);
            compressor.Close();
            return compressStream.ToArray();
        }

        private static byte[] LZ4Compress(Stream input, CompressionLevel level)
        {
            LZ4Level lzlevel = level switch
            {
                CompressionLevel.Optimal => LZ4Level.L11_OPT,
                CompressionLevel.Fastest => LZ4Level.L00_FAST,
                CompressionLevel.NoCompression => throw new NotImplementedException(),
                CompressionLevel.SmallestSize => LZ4Level.L12_MAX,
                _ => default,
            };
            using var compressStream = new MemoryStream();
            using var compressor = LZ4Stream.Encode(compressStream, lzlevel, 0);
            input.CopyTo(compressor);
            compressor.Close();
            return compressStream.ToArray();
        }
    }
}