namespace HexaEngine.Core.IO.Binary.Archives
{
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.IO;
    using K4os.Compression.LZ4;
    using K4os.Compression.LZ4.Streams;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.IO.Compression;
    using System.IO.Hashing;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Represents an archive containing assets, providing methods to load, save, and extract assets.
    /// </summary>
    public class AssetArchive
    {
        private readonly List<AssetArchiveEntry> entries;
        private readonly Dictionary<string, AssetArchiveEntry> pathToAsset = [];
        private readonly long baseOffset;
        private readonly AssetArchiveFlags flags;
        private readonly string[] parts;
        private readonly uint crc32;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetArchive"/> class.
        /// </summary>
        public AssetArchive()
        {
            entries = [];
            parts = [];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetArchive"/> class from the specified file path.
        /// </summary>
        /// <param name="path">The path to the asset archive file.</param>
        public AssetArchive(string path)
        {
            var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            AssetArchiveHeader header = new();
            header.Read(fs, Encoding.UTF8);

            var basePath = Path.GetDirectoryName(path) ?? throw new InvalidDataException("Bad archive, couldn't get the base path of the archive");

            if (header.Parts == null)
            {
                throw new InvalidDataException("Bad archive, Header.Parts was null");
            }

            baseOffset = header.ContentStart;
            flags = header.Flags;
            parts = header.Parts;
            crc32 = header.CRC32;
            entries = new(header.EntryCount);
            for (int i = 0; i < header.EntryCount; i++)
            {
                Unsafe.SkipInit(out AssetArchiveEntry entry);
                entry.Read(fs, Encoding.UTF8, header.Endianness);
                var archivePath = path;

                if ((flags & AssetArchiveFlags.Partial) != 0)
                {
                    archivePath = Path.Combine(basePath, header.Parts[entry.PartIndex]);
                }

                entry.ArchivePath = archivePath;
                entry.Compression = header.Compression;
                entry.BaseOffset = baseOffset;
                entries.Add(entry);
                pathToAsset.Add(entry.PathInArchive, entry);
            }
        }

        /// <summary>
        /// Gets an list of <see cref="AssetArchiveEntry"/> representing the assets in the archive.
        /// </summary>
        public List<AssetArchiveEntry> Assets => entries;

        public long BaseOffset => baseOffset;

        public AssetArchiveFlags Flags => flags;

        public string[] Parts => parts;

        public uint CRC32 => crc32;

        public AssetArchiveEntry this[string path]
        {
            get => pathToAsset[path];
            set => pathToAsset[path] = value;
        }

        public bool TryGetEntry(string path, [NotNullWhen(true)] out AssetArchiveEntry entry)
        {
            return pathToAsset.TryGetValue(path, out entry);
        }

        /// <summary>
        /// Saves the asset archive to the specified path with optional compression.
        /// </summary>
        /// <param name="path">The path where the asset archive will be saved.</param>
        /// <param name="compression">The compression algorithm to use.</param>
        /// <param name="level">The compression level to apply.</param>
        /// <param name="certificate"></param>
        public void Save(string path, Compression compression = Compression.None, CompressionLevel level = CompressionLevel.Optimal, RSA? rsa = null)
        {
            var fs = File.Create(path);
            AssetArchiveHeader header = new()
            {
                Endianness = Hexa.NET.Mathematics.Endianness.LittleEndian,
                Compression = compression,
                Flags = AssetArchiveFlags.Normal,
                Parts = [],
                EntryCount = entries.Count,
            };

            int entryTableSize = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                entryTableSize += entries[i].Size(Encoding.UTF8);
            }

            var headerLength = header.Size(Encoding.UTF8);
            header.ContentStart = headerLength + entryTableSize;
            fs.Position = header.ContentStart;

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];

                using var input = entry.GetStream();

                long pos = fs.Position;
                switch (compression)
                {
                    case Compression.None:
                        input.CopyTo(fs);
                        break;

                    case Compression.Deflate:
                        using (var output = DeflateCompressStream(fs, level))
                        {
                            input.CopyTo(output);
                        }
                        break;

                    case Compression.LZ4:
                        using (var output = LZ4CompressStream(fs, level))
                        {
                            input.CopyTo(output);
                        }
                        break;
                }

                long length = fs.Position - pos;
                entry.Start = pos - header.ContentStart;
                entry.Length = length;
                entry.ActualLength = input.Length;
                entries[i] = entry;
            }

            long contentEnd = fs.Position;

            fs.Position = header.ContentStart;

            uint crc32 = 0xffffffffu;
            Span<byte> inBuffer = stackalloc byte[8192];
            Span<byte> hash0 = stackalloc byte[32];
            Span<byte> hash1 = stackalloc byte[32];
            Span<byte> hash2 = stackalloc byte[64];
            bool first = true;

            while (fs.Position != contentEnd)
            {
                int read = fs.Read(inBuffer);
                crc32 ^= Crc32.HashToUInt32(inBuffer[..read]);

                SHA256.TryHashData(inBuffer[..read], hash1, out _);

                if (first)
                {
                    first = false;
                    hash1.CopyTo(hash2[32..]);
                }
                else
                {
                    hash1.CopyTo(hash2);

                    SHA256.TryHashData(hash2, hash0, out _);

                    hash0.CopyTo(hash2[32..]);
                }
            }

            header.CRC32 = ~crc32;
            header.SHA256 = new(hash0, false);

            if (rsa != null)
            {
                Span<byte> signature = stackalloc byte[rsa.KeySize / 8];
                rsa.TrySignHash(hash0, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1, out var bytesWritten);
            }

            fs.Position = 0;
            header.Write(fs, Encoding.UTF8);

            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].Write(fs, Encoding.UTF8, header.Endianness);
            }

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
            path = root.FullName;
            foreach (AssetArchiveEntry asset in entries)
            {
                string fullPath = Path.Combine(path, asset.PathInArchive);
                string? dirName = Path.GetDirectoryName(fullPath);
                if (dirName != null)
                {
                    Directory.CreateDirectory(dirName);
                }

                var fs = File.Create(fullPath);
                fs.Write(asset.GetData());
                fs.Flush();
                fs.Close();
                fs.Dispose();
            }
        }

        private static DeflateStream DeflateCompressStream(Stream output, CompressionLevel level)
        {
            return new DeflateStream(output, level, true);
        }

        private static LZ4EncoderStream LZ4CompressStream(Stream output, CompressionLevel level)
        {
            LZ4Level lzlevel = level switch
            {
                CompressionLevel.Optimal => LZ4Level.L11_OPT,
                CompressionLevel.Fastest => LZ4Level.L00_FAST,
                CompressionLevel.NoCompression => throw new NotSupportedException(),
                CompressionLevel.SmallestSize => LZ4Level.L12_MAX,
                _ => default,
            };

            return LZ4Stream.Encode(output, lzlevel, 0, true);
        }

        public AssetArchiveEntry AddArtifact(Artifact artifact, string pathInArchive)
        {
            FileInfo info = new(artifact.Path);
            AssetArchiveEntry asset = new(Compression.None, info.FullName, 0, artifact.Type, artifact.Guid, artifact.ParentGuid, artifact.Name, pathInArchive, 0, 0, 0);
            entries.Add(asset);
            pathToAsset.Add(asset.PathInArchive, asset);
            return asset;
        }
    }
}