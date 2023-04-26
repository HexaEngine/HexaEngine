namespace HexaEngine.Core.IO.Assets
{
    using K4os.Compression.LZ4;
    using K4os.Compression.LZ4.Streams;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Text;

    public class AssetArchive
    {
        private readonly Stream stream;
        private readonly AssetArchiveHeader header;

        public AssetArchive(string path)
        {
            var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            header = new();
            header.Read(fs, Encoding.UTF8);
            stream = fs;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            Asset[] assets = new Asset[header.Entries.Length];
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            for (int i = 0; i < assets.Length; i++)
            {
                var entry = header.Entries[i];
                assets[i] = new Asset(path, header.Compression, entry.Type, entry.Start + header.ContentStart, entry.Length, entry.ActualLength, entry.Path);
            }
            Assets = assets;
        }

        public Asset[] Assets { get; }

        public Stream GetStream() => stream;

        public void Save(string path, Compression compression = Compression.None, CompressionLevel level = CompressionLevel.Optimal)
        {
            var fs = File.OpenWrite(path);
            var headerLength = header.Size(Encoding.UTF8);
            fs.Position = headerLength;

            long position = 0;
            for (int i = 0; i < Assets.Length; i++)
            {
                var asset = Assets[i];
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

        public void Extract(string path)
        {
            var root = new DirectoryInfo(path);
            foreach (Asset asset in Assets)
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

        public static void CreateFrom(AssetDesc[] assets, string output, Compression compression = Compression.None, CompressionLevel level = CompressionLevel.Optimal)
        {
            var fs = File.Create(output);
            var filename = Path.GetFileName(output);

            AssetArchiveHeader header = default;
            header.Compression = compression;
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