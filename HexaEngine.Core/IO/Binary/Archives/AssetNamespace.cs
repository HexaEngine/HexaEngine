using Hexa.NET.Mathematics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace HexaEngine.Core.IO.Binary.Archives
{
    public class AssetNamespace : IAssetNamespace
    {
        private string name;
        private readonly Dictionary<string, AssetArchiveEntry> assets = new(PathComparer.Instance);
        private readonly Dictionary<Guid, AssetArchiveEntry> guidToAsset = [];

        public AssetNamespace(string name)
        {
            this.name = name;
        }

        public string Name { get => name; set => name = value; }

        public IReadOnlyDictionary<string, AssetArchiveEntry> Assets => assets;

        public bool TryGetAsset(ReadOnlySpan<char> path, [NotNullWhen(true)] out AssetArchiveEntry? entry)
        {
            return assets.GetAlternateLookup<ReadOnlySpan<char>>().TryGetValue(path, out entry);
        }

        public bool TryGetAsset(Guid guid, [NotNullWhen(true)] out AssetArchiveEntry? entry)
        {
            return guidToAsset.TryGetValue(guid, out entry);
        }

        public void AddAsset(AssetArchiveEntry entry)
        {
            assets[entry.PathInArchive] = entry;
            guidToAsset[entry.Guid] = entry;
        }

        public bool RemoveAsset(string path)
        {
            return assets.Remove(path);
        }

        public void Clear()
        {
            assets.Clear();
        }

        public static AssetNamespace Read(Stream stream, in AssetArchiveHeader header, AssetArchive archive)
        {
            var endianness = header.Endianness;
            var encoding = header.Encoding;

            var name = stream.ReadString(encoding, endianness)!;
            AssetNamespace ns = new(name);

            var count = stream.ReadUInt32(endianness);
            ns.assets.EnsureCapacity((int)count);

            for (uint i = 0; i < count; ++i)
            {
                var entry = AssetArchiveEntry.ReadFrom(stream, encoding, endianness, archive);
                ns.AddAsset(entry);
            }

            return ns;
        }

        public void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            stream.WriteString(name, encoding, endianness);
            stream.WriteUInt32((uint)assets.Count, endianness);
            foreach (var asset in assets.Values)
            {
                asset.Write(stream, encoding, endianness);
            }
        }

        public int SizeOf(Encoding encoding)
        {
            int size = 0;
            size += name.SizeOf(encoding);
            size += sizeof(uint);
            foreach (var asset in assets.Values)
            {
                size += asset.SizeOf(encoding);
            }
            return size;
        }

        public void ExtractEntries(string path)
        {
            foreach (var (assetPath, asset) in assets)
            {
                string fullPath = Path.Combine(path, assetPath);
                string? dirName = Path.GetDirectoryName(fullPath);
                if (dirName != null)
                {
                    Directory.CreateDirectory(dirName);
                }

                using var fs = File.Create(fullPath);
                asset.CopyTo(fs);
            }
        }

        public bool TryGetAsset(Guid guid, [NotNullWhen(true)] out IAssetEntry? entry)
        {
            var result = TryGetAsset(guid, out AssetArchiveEntry? e);
            entry = e;
            return result;
        }

        public bool TryGetAsset(ReadOnlySpan<char> path, [NotNullWhen(true)] out IAssetEntry? entry)
        {
            var result = TryGetAsset(path, out AssetArchiveEntry? e);
            entry = e;
            return result;
        }
    }
}