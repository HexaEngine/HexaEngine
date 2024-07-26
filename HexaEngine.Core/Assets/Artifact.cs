namespace HexaEngine.Core.Assets
{
    using HexaEngine.Core.IO;
    using Hexa.NET.Mathematics;
    using System.Buffers.Binary;
    using System.Text;

    public class Artifact
    {
        private string name;
        private string? path;
        private ArtifactFlags flags;

        public Artifact(string name, Guid parentGuid, Guid sourceGuid, Guid guid, AssetType type)
        {
            DisplayName = $"{name}##{guid}";
            this.name = name;
            ParentGuid = parentGuid;
            SourceGuid = sourceGuid;
            Guid = guid;
            Type = type;
        }

        [JsonIgnore]
        public string DisplayName { get; private set; }

        public string Name
        { get => name; set { name = value; DisplayName = $"{name}##{Guid}"; } }

        public Guid ParentGuid { get; }

        public Guid SourceGuid { get; }

        public Guid Guid { get; }

        public AssetType Type { get; }

        [JsonIgnore]
        public ArtifactFlags Flags { get; internal set; }

        [JsonIgnore]
        public string Path => path ??= System.IO.Path.Combine(ArtifactDatabase.CacheFolder, Guid.ToString());

        public void Write(Stream stream)
        {
            Span<byte> buffer = stackalloc byte[16 + 16 + 16 + 4];
            Guid.TryWriteBytes(buffer[..16]);
            SourceGuid.TryWriteBytes(buffer.Slice(16, 16));
            ParentGuid.TryWriteBytes(buffer.Slice(32, 16));
            BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(48, 4), (int)Type);
            stream.Write(buffer);
            stream.WriteString(Name, Encoding.UTF8, Endianness.LittleEndian);
        }

        public static Artifact Read(Stream stream)
        {
            Span<byte> buffer = stackalloc byte[16 + 16 + 16 + 4];
            stream.Read(buffer);
            var guid = new Guid(buffer[..16]);
            var sourceGuid = new Guid(buffer.Slice(16, 16));
            var parentGuid = new Guid(buffer.Slice(32, 16));
            var type = (AssetType)BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(48, 4));
            var name = stream.ReadString(Encoding.UTF8, Endianness.LittleEndian);
            return new Artifact(name, parentGuid, sourceGuid, guid, type);
        }

        public int Write(Span<byte> destination)
        {
            Guid.TryWriteBytes(destination[..16]);
            SourceGuid.TryWriteBytes(destination.Slice(16, 16));
            ParentGuid.TryWriteBytes(destination.Slice(32, 16));
            BinaryPrimitives.WriteInt32LittleEndian(destination.Slice(48, 4), (int)Type);
            int idx = 52;
            idx += destination[52..].WriteString(Name, Encoding.UTF8);
            return idx;
        }

        public static int Read(ReadOnlySpan<byte> source, out Artifact artifact)
        {
            var guid = new Guid(source[..16]);
            var sourceGuid = new Guid(source.Slice(16, 16));
            var parentGuid = new Guid(source.Slice(32, 16));
            var type = (AssetType)BinaryPrimitives.ReadInt32LittleEndian(source.Slice(48, 4));
            int idx = 52;
            idx += source[52..].ReadString(Encoding.UTF8, out var name);
            artifact = new Artifact(name, parentGuid, sourceGuid, guid, type);
            return idx;
        }

        public Stream OpenRead()
        {
            return File.OpenRead(Path);
        }

        public SourceAssetMetadata? GetSourceMetadata()
        {
            return SourceAssetsDatabase.GetMetadata(SourceGuid);
        }

        public override string ToString()
        {
            return $"GUID: {Guid}, Source GUID {SourceGuid}, Type {Type}, Path {Path}";
        }
    }
}