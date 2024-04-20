using HexaEngine.Core.Graphics.Reflection;
using HexaEngine.Core.IO;
using HexaEngine.Mathematics;
using System.Buffers.Binary;
using System.Text;

namespace HexaEngine.Core.Assets
{
    public class Artifact
    {
        private string name;
        private string? path;

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