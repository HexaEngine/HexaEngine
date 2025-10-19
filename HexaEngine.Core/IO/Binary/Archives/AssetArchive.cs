namespace HexaEngine.Core.IO.Binary.Archives
{
    using Hexa.NET.FreeType;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Security.Cryptography;
    using K4os.Compression.LZ4;
    using K4os.Compression.LZ4.Streams;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.IO.Compression;
    using System.Reflection.Emit;
    using System.Security.Cryptography;
    using System.Text;

    public enum AssetArchiveMode
    {
        OpenRead,
        Create,
    }

    /// <summary>
    /// Represents an archive containing assets, providing methods to load, save, and extract assets.
    /// </summary>
    public class AssetArchive : IDisposable
    {
        private readonly Dictionary<string, AssetNamespace> namespaces = [];
        private readonly Dictionary<string, AssetArchiveEntry> pathToAsset = [];
        private AssetArchiveHeader header = new();
        private readonly string path;
        private readonly AssetArchiveMode mode;
        private Stream? dstStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetArchive"/> class from the specified file path.
        /// </summary>
        /// <param name="path">The path to the asset archive file.</param>
        /// <param name="mode"></param>
        public AssetArchive(string path, AssetArchiveMode mode)
        {
            this.path = path;
            this.mode = mode;

            if (mode == AssetArchiveMode.OpenRead)
            {
                Read();
            }
            else if (mode == AssetArchiveMode.Create)
            {
                header = new(Hexa.NET.Mathematics.Endianness.LittleEndian);
                dstStream = File.Create(path);
            }
        }

        public IReadOnlyDictionary<string, AssetNamespace> Namespaces => namespaces;

        public Compression Compression => header.Compression;

        public long BaseOffset => header.ContentStart;

        public AssetArchiveFlags Flags => header.Flags;

        public string[] Parts => header.Parts;

        public uint CRC32 => header.CRC32;

        public const string AnonymousNamespaceName = "<anonymous>";

        public AssetNamespace AnonymousNamespace
        {
            get
            {
                if (!namespaces.TryGetValue(AnonymousNamespaceName, out var ns))
                {
                    ns = AddNamespace(AnonymousNamespaceName);
                }
                return ns;
            }
        }

        public AssetNamespace AddNamespace(string name)
        {
            var ns = new AssetNamespace(name);
            namespaces.Add(name, ns);
            return ns;
        }

        public bool TryGetNamespace(ReadOnlySpan<char> name, [NotNullWhen(true)] out AssetNamespace? ns)
        {
            return namespaces.GetAlternateLookup<ReadOnlySpan<char>>().TryGetValue(name, out ns);
        }

        public bool TryGetEntry(string path, [NotNullWhen(true)] out AssetArchiveEntry? entry)
        {
            var idx = path.IndexOf(':');
            if (idx == -1)
            {
                return AnonymousNamespace.TryGetAsset(path, out entry);
            }
            else
            {
                var nsName = path.AsSpan()[..idx];
                var assetPath = path.AsSpan()[(idx + 1)..];
                if (TryGetNamespace(nsName, out var ns))
                {
                    return ns.TryGetAsset(assetPath, out entry);
                }
                entry = null;
                return false;
            }
        }

        public bool TryGetEntry(Guid guid, [NotNullWhen(true)] out AssetArchiveEntry? entry)
        {
            foreach (var (name, ns) in namespaces)
            {
                if (ns.TryGetAsset(guid, out entry))
                {
                    return true;
                }
            }
            entry = null;
            return false;
        }

        private void Read()
        {
            using var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            header.Read(fs, Encoding.UTF8);

            var basePath = Path.GetDirectoryName(path) ?? throw new InvalidDataException("Bad archive, couldn't get the base path of the archive");

            if (header.Parts == null)
            {
                throw new InvalidDataException("Bad archive, Header.Parts was null");
            }

            if ((header.Flags & AssetArchiveFlags.Partial) != 0)
            {
                for (int i = 0; i < header.Parts.Length; i++)
                {
                    header.Parts[i] = Path.Combine(basePath, header.Parts[i]);
                }
            }

            uint namespaceCount = fs.ReadUInt32(header.Endianness);
            namespaces.EnsureCapacity((int)namespaceCount);
            for (uint i = 0; i < namespaceCount; ++i)
            {
                var ns = AssetNamespace.Read(fs, header, this);
                namespaces.Add(ns.Name, ns);
            }
        }

        private void Write(RSA? rsa)
        {
            if (dstStream == null) throw new InvalidOperationException("Archive not opened in create mode.");

            SignContent(dstStream, rsa);

            var encoding = header.Encoding;
            int metadataSize = header.SizeOf(encoding);
            foreach (var (_, ns) in namespaces)
            {
                metadataSize += ns.SizeOf(encoding);
            }

            header.ContentStart = AlignUp(metadataSize, header.Alignment);

            MoveContent(dstStream, metadataSize);

            dstStream.Position = 0;
            header.Write(dstStream, encoding);

            dstStream.WriteUInt32((uint)namespaces.Count, header.Endianness);
            foreach (var (_, ns) in namespaces)
            {
                ns.Write(dstStream, encoding, header.Endianness);
            }

            dstStream.Flush();
        }

        private void MoveContent(Stream stream, int offset)
        {
            Span<byte> buffer = stackalloc byte[8192];
            long toMove = stream.Length;
            stream.Position = toMove;
            while (toMove > 0)
            {
                int toRead = (int)Math.Min(toMove, buffer.Length);
                var pos = toMove - toRead;
                stream.Position = pos;
                stream.ReadExactly(buffer[..toRead]);
                stream.Position = pos + offset;
                stream.Write(buffer[..toRead]);
                toMove -= toRead;
            }
        }

        private void SignContent(Stream stream, RSA? rsa)
        {
            stream.Position = 0;
            header.CRC32 = Crc32.HashData(stream);

            Span<byte> shaHash = stackalloc byte[32];
            stream.Position = 0;
            SHA256.HashData(stream, shaHash);
            header.SHA256 = new(shaHash, false);

            if (rsa != null)
            {
                Span<byte> signature = stackalloc byte[rsa.KeySize / 8];
                rsa.TrySignHash(shaHash, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1, out _);
                header.RSASignature = new(RSASignatureMode.SHA256_PKCS1v15, rsa, signature);
            }
        }

        private static long AlignUp(long value, long alignment)
        {
            var align = alignment - 1;
            return (value + align) & ~align;
        }

        public AssetArchiveEntry AddEntry(AssetNamespace ns, Stream stream, string path, string name, AssetType type, Guid guid, Guid parentGuid, CompressionLevel level = CompressionLevel.Optimal)
        {
            if (dstStream == null) throw new InvalidOperationException("Archive not opened in create mode.");

            var offset = dstStream.Position;

            long pos = dstStream.Position;
            switch (header.Compression)
            {
                case Compression.None:
                    stream.CopyTo(dstStream);
                    break;

                case Compression.Deflate:
                    using (var output = DeflateCompressStream(dstStream, level))
                    {
                        stream.CopyTo(output);
                    }
                    break;

                case Compression.LZ4:
                    using (var output = LZ4CompressStream(dstStream, level))
                    {
                        stream.CopyTo(output);
                    }
                    break;
            }

            long actualLength = stream.Length;
            long length = dstStream.Position - pos;
            var lengthAligned = AlignUp(length, header.Alignment);
            var padding = lengthAligned - length;

            AssetArchiveEntry entry = new(this, 0, type, guid, parentGuid, name, path, offset, lengthAligned, actualLength);

            Span<byte> buffer = stackalloc byte[8192];
            while (padding > 0)
            {
                var toWrite = Math.Min(padding, buffer.Length);
                dstStream.Write(buffer[..(int)toWrite]);
                padding -= toWrite;
            }

            ns.AddAsset(entry);
            return entry;
        }

        public AssetArchiveEntry AddArtifact(Artifact artifact, string pathInArchive)
        {
            return AddEntry(AnonymousNamespace, artifact.OpenRead(), pathInArchive, artifact.Name, artifact.Type, artifact.Guid, artifact.ParentGuid);
        }

        /// <summary>
        /// Extracts the assets from the archive to the specified directory.
        /// </summary>
        /// <param name="path">The directory where the assets will be extracted.</param>
        public void Extract(string path)
        {
            path = Path.GetFullPath(path);
            foreach (var (_, ns) in namespaces)
            {
                ns.ExtractEntries(path);
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

        protected virtual void DisposeCore()
        {
            if (mode == AssetArchiveMode.Create && dstStream != null)
            {
                Write(null);
                dstStream!.Dispose();
                dstStream = null;
            }
        }

        public void Dispose()
        {
            DisposeCore();
            GC.SuppressFinalize(this);
        }
    }
}