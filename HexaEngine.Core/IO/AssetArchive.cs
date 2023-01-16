namespace HexaEngine.IO
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
            var headerLength = fs.ReadInt();
            var headerBuffer = new byte[headerLength];
            fs.Read(headerBuffer);
            AssetArchiveHeader header = default;
            header.Read(headerBuffer, Encoding.UTF8);

            int contentOffset = 4 + headerLength;

            stream = fs;
            this.header = header;

            Asset[] assets = new Asset[header.Entries.Length];
            for (int i = 0; i < assets.Length; i++)
            {
                var entry = header.Entries[i];
                assets[i] = new Asset(path, header.Compression, entry.Type, entry.Start + contentOffset, entry.Length, entry.ActualLength, entry.Path);
            }
            Assets = assets;
        }

        public Asset[] Assets { get; }

        public Stream GetStream() => stream;

        public void Save(string path, Compression compression = Compression.None, CompressionLevel level = CompressionLevel.Optimal)
        {
            var fs = File.OpenWrite(path);
            var headerLength = header.Size(Encoding.UTF8);
            var headerBuffer = new byte[headerLength];

            fs.WriteInt(headerLength);
            fs.Position += headerLength;

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

            header.Write(headerBuffer, Encoding.UTF8);
            fs.Position = 4;
            fs.Write(headerBuffer);

            fs.Flush();
            fs.Close();
        }

        public void Extract(string path)
        {
            var root = new DirectoryInfo(path);
            foreach (Asset asset in Assets)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(root.FullName + asset.Path));
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
            var headerBuffer = new byte[headerLength];

            fs.WriteInt(headerLength);
            fs.Position += headerLength;

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

            header.Write(headerBuffer, Encoding.UTF8);
            fs.Position = 4;
            fs.Write(headerBuffer);

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
                    continue;
                if (file.Extension == ".dll")
                    continue;
                if (file.Extension == ".hexlvl")
                    continue;
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
            var headerBuffer = new byte[headerLength];

            fs.WriteInt(headerLength);
            fs.Position += headerLength;

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

            header.Write(headerBuffer, Encoding.UTF8);
            fs.Position = 4;
            fs.Write(headerBuffer);

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
                        continue;
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
                var headerBuffer = new byte[headerLength];

                fs.WriteInt(headerLength);
                fs.Position += headerLength;

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

                header.Write(headerBuffer, Encoding.UTF8);
                fs.Position = 4;
                fs.Write(headerBuffer);

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
                return AssetType.Texture;
            }

            if (path.Contains("meshes"))
            {
                trimmed = path.Replace("meshes/", string.Empty).Replace("meshes\\", string.Empty);
                return AssetType.Mesh;
            }

            if (path.Contains("materials"))
            {
                trimmed = path.Replace("materials/", string.Empty).Replace("materials\\", string.Empty);
                return AssetType.Material;
            }

            if (path.Contains("shaders") && !Path.HasExtension(".sb"))
            {
                trimmed = path.Replace("shaders/", string.Empty).Replace("shaders\\", string.Empty);
                return AssetType.ShaderSource;
            }

            if (path.Contains("shaders") && Path.HasExtension(".sb"))
            {
                trimmed = path.Replace("shaders/", string.Empty).Replace("shaders\\", string.Empty);
                return AssetType.ShaderBytecode;
            }

            if (path.Contains("fonts"))
            {
                trimmed = path.Replace("fonts/", string.Empty).Replace("fonts\\", string.Empty);
                return AssetType.Font;
            }

            if (path.Contains("sounds"))
            {
                trimmed = path.Replace("sounds/", string.Empty).Replace("sounds\\", string.Empty);
                return AssetType.Sound;
            }

            if (path.Contains("scripts"))
            {
                trimmed = path.Replace("scripts/", string.Empty).Replace("scripts\\", string.Empty);
                return AssetType.Script;
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