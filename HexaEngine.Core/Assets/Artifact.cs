namespace HexaEngine.Core.Assets
{
    public class Artifact
    {
        public Artifact(string name, Guid sourceGuid, Guid guid, AssetType type, string path)
        {
            DisplayName = $"{name}##{guid}";
            Name = name;
            SourceGuid = sourceGuid;
            Guid = guid;
            Type = type;
            Path = path;
        }

        [JsonIgnore]
        public string DisplayName { get; }

        public string Name { get; }

        public Guid SourceGuid { get; }

        public Guid Guid { get; }

        public AssetType Type { get; }

        public string Path { get; }

        public Stream OpenRead()
        {
            var path = Path;
            return File.OpenRead(path);
        }

        public SourceAssetMetadata? GetSourceMetadata()
        {
            return SourceAssetsDatabase.GetMetadata(SourceGuid);
        }
    }
}