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