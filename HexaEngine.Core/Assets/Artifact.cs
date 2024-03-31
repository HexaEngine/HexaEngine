namespace HexaEngine.Core.Assets
{
    public class Artifact
    {
        private string name;

        public Artifact(string name, Guid parentGuid, Guid sourceGuid, Guid guid, AssetType type, string path)
        {
            DisplayName = $"{name}##{guid}";
            this.name = name;
            ParentGuid = parentGuid;
            SourceGuid = sourceGuid;
            Guid = guid;
            Type = type;
            Path = path;
        }

        [JsonIgnore]
        public string DisplayName { get; private set; }

        public string Name
        { get => name; set { name = value; DisplayName = $"{name}##{Guid}"; } }

        public Guid ParentGuid { get; }

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

        public override string ToString()
        {
            return $"GUID: {Guid}, Source GUID {SourceGuid}, Type {Type}, Path {Path}";
        }
    }
}