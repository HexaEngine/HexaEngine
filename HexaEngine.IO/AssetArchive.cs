namespace HexaEngine.IO
{
    using System;
    using System.Buffers.Binary;
    using System.IO;
    using System.IO.Compression;
    using System.Text;

    public enum Compression
    {
        None = 0,
        Deflate,
    }

    public struct AssetArchiveHeaderEntry
    {
        /// <summary>
        /// The type of the asset.
        /// </summary>
        public AssetType Type;

        /// <summary>
        /// The path.
        /// </summary>
        public string Path;

        /// <summary>
        /// Position in file.
        /// </summary>
        public long Start;

        /// <summary>
        /// Length in file.
        /// </summary>
        public long Length;

        /// <summary>
        /// Decompressed size, is the same as lenght if the compression is set to none.
        /// </summary>
        public long ActualLength;

        public void Read(Stream stream, Encoding encoding)
        {
            Type = (AssetType)stream.ReadUInt64();
            Start = stream.ReadInt64();
            Length = stream.ReadInt64();
            ActualLength = stream.ReadInt64();
            Path = stream.ReadString(encoding);
        }

        public void Write(Stream stream, Encoding encoding)
        {
            stream.WriteUInt64((ulong)Type);
            stream.WriteInt64(Start);
            stream.WriteInt64(Length);
            stream.WriteInt64(ActualLength);
            stream.WriteString(Path, encoding);
        }

        public int Read(ReadOnlySpan<byte> src, Encoding encoding)
        {
            Type = (AssetType)BinaryPrimitives.ReadUInt64LittleEndian(src);
            Start = BinaryPrimitives.ReadInt64LittleEndian(src[8..]);
            Length = BinaryPrimitives.ReadInt64LittleEndian(src[16..]);
            ActualLength = BinaryPrimitives.ReadInt64LittleEndian(src[24..]);
            Path = src[32..].ReadString(encoding, out int read);
            return 32 + read;
        }

        public int Write(Span<byte> dst, Encoding encoding)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(dst, (ulong)Type);
            BinaryPrimitives.WriteInt64LittleEndian(dst[8..], Start);
            BinaryPrimitives.WriteInt64LittleEndian(dst[16..], Length);
            BinaryPrimitives.WriteInt64LittleEndian(dst[24..], ActualLength);
            return 32 + dst[32..].WriteString(Path, encoding);
        }

        public int Size(Encoding encoding)
        {
            return 8 + 8 + 8 + 8 + encoding.GetByteCount(Path) + 4;
        }
    }

    public struct AssetArchiveHeader
    {
        public static readonly byte[] MagicNumber = { 0x54, 0x72, 0x61, 0x6e, 0x73, 0x55, 0x77, 0x55, 0x00 };
        public const ulong Version = 1;

        public Compression Compression;
        public AssetArchiveHeaderEntry[] Entries;

        public void Read(Stream stream, Encoding encoding)
        {
            if (stream.Compare(MagicNumber))
                throw new InvalidDataException();
            if (stream.Compare(Version))
                throw new InvalidDataException();

            Compression = (Compression)stream.ReadInt();

            int count = stream.ReadInt();
            Entries = new AssetArchiveHeaderEntry[count];

            for (int i = 0; i < count; i++)
            {
                Entries[i].Read(stream, encoding);
            }
        }

        public void Read(ReadOnlySpan<byte> src, Encoding encoding)
        {
            if (!src[..MagicNumber.Length].SequenceEqual(MagicNumber))
                throw new InvalidDataException();
            if (!src[MagicNumber.Length..].Compare(Version))
                throw new InvalidDataException();

            int idx = MagicNumber.Length + 8;

            Compression = (Compression)BinaryPrimitives.ReadInt32LittleEndian(src[idx..]);
            idx += 4;

            int count = BinaryPrimitives.ReadInt32LittleEndian(src[idx..]);
            idx += 4;

            Entries = new AssetArchiveHeaderEntry[count];

            for (int i = 0; i < count; i++)
            {
                idx += Entries[i].Read(src[idx..], encoding);
            }
        }

        public void Write(Stream stream, Encoding encoding)
        {
            stream.Write(MagicNumber);
            stream.WriteUInt64(Version);
            stream.WriteInt((int)Compression);
            stream.WriteInt(Entries.Length);
            for (int i = 0; i < Entries.Length; i++)
            {
                Entries[i].Write(stream, encoding);
            }
        }

        public void Write(Span<byte> dst, Encoding encoding)
        {
            MagicNumber.CopyTo(dst);
            BinaryPrimitives.WriteUInt64LittleEndian(dst[MagicNumber.Length..], Version);
            int idx = MagicNumber.Length + 8;
            BinaryPrimitives.WriteInt32LittleEndian(dst[idx..], (int)Compression);
            idx += 4;
            BinaryPrimitives.WriteInt32LittleEndian(dst[idx..], Entries.Length);
            idx += 4;

            for (int i = 0; i < Entries.Length; i++)
            {
                idx += Entries[i].Write(dst[idx..], encoding);
            }
        }

        public int Size(Encoding encoding)
        {
            return MagicNumber.Length + 8 + 4 + 4 + Entries.Sum(x => x.Size(encoding));
        }
    }

    public class AssetArchive
    {
        private readonly Stream stream;
        private readonly AssetArchiveHeader header;

        public static readonly byte[] MagicNumber = { 0x54, 0x72, 0x61, 0x6e, 0x73, 0x55, 0x77, 0x55, 0x00 };
        public const ulong Version = 1;

        public AssetArchive(string path)
        {
            var fs = Stream.Synchronized(File.OpenRead(path));
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
                switch (header.Compression)
                {
                    case Compression.None:
                        assets[i] = new Asset(entry.Type, entry.Start + contentOffset, entry.ActualLength, entry.Path, fs);
                        break;

                    case Compression.Deflate:
                        MemoryStream ms = new(new byte[entry.ActualLength]);
                        DeflateDecompress(new VirtualStream(fs, entry.Start + contentOffset, entry.Length, false), ms);
                        assets[i] = new Asset(entry.Type, 0, entry.ActualLength, entry.Path, ms);
                        break;
                }
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
                        var buffer = DeflateCompress(asset.GetStream(), level);
                        entry.Length = buffer.Length;
                        entry.ActualLength = asset.Length;
                        fs.Write(buffer);
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

        public static void CreateFrom(string path, Compression compression = Compression.None, CompressionLevel level = CompressionLevel.Optimal)
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

                var fs = File.Create(dir.Name + ".assets");
                var filename = dir.Name + ".assets";

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
                            var buffer = DeflateCompress(ts, level);
                            entry.ActualLength = ts.Length;
                            entry.Length = buffer.Length;
                            fs.Write(buffer);
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
                            var buffer = DeflateCompress(ts, level);
                            entry.ActualLength = ts.Length;
                            entry.Length = buffer.Length;
                            fs.Write(buffer);
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

            if (path.Contains("shaders"))
            {
                trimmed = path.Replace("shaders/", string.Empty).Replace("shaders\\", string.Empty);
                return AssetType.Shader;
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

        private static void DeflateDecompress(Stream input, Stream output)
        {
            using var decompressor = new DeflateStream(input, CompressionMode.Decompress);
            decompressor.CopyTo(output);
            decompressor.Close();
        }
    }
}