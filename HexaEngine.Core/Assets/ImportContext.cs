namespace HexaEngine.Core.Assets
{
    public class ImportContext
    {
        private readonly SourceAssetMetadata sourceAsset;
        private readonly List<Artifact> lastArtifacts = [];
        private readonly HashSet<Guid> toRemove = [];

        public ImportContext(SourceAssetMetadata sourceAsset)
        {
            this.sourceAsset = sourceAsset;
        }

        public ImportContext(SourceAssetMetadata sourceAsset, List<Artifact> artifacts) : this(sourceAsset)
        {
            lastArtifacts = artifacts;
            toRemove = artifacts.Select(x => x.Guid).ToHashSet();
        }

        public string SourcePath => Path.Combine(ArtifactDatabase.ProjectRoot, sourceAsset.FilePath);

        public Dictionary<string, object> Additional => sourceAsset.Additional;

        public HashSet<Guid> ToRemoveArtifacts => toRemove;

        public void EmitArtifact(string name, AssetType type, out string path)
        {
            Artifact? last = lastArtifacts.FirstOrDefault(x => x.Name == name);

            if (last != null)
            {
                toRemove.Remove(last.Guid);
                path = last.Path;
            }
            else
            {
                Guid guid = Guid.NewGuid();
                string outputPath = Path.Combine(ArtifactDatabase.CacheFolder, guid.ToString());
                Artifact artifact = new(name, sourceAsset.Guid, guid, type, outputPath);
                ArtifactDatabase.AddArtifact(artifact);
                path = outputPath;
            }
        }

        public void EmitArtifact(string name, Guid guid, AssetType type, out string path)
        {
            Artifact? last = lastArtifacts.FirstOrDefault(x => x.Name == name);

            if (last != null)
            {
                toRemove.Remove(last.Guid);
                path = last.Path;
            }
            else
            {
                string outputPath = Path.Combine(ArtifactDatabase.CacheFolder, guid.ToString());
                Artifact artifact = new(name, sourceAsset.Guid, guid, type, outputPath);
                ArtifactDatabase.AddArtifact(artifact);
                path = outputPath;
            }
        }

        public void EmitArtifact(string name, AssetType type, out FileStream stream)
        {
            EmitArtifact(name, type, out string path);
            stream = File.Create(path);
        }

        public T? GetAdditionalMetadata<T>(string key) where T : class
        {
            if (Additional.TryGetValue(key, out var metadata) && metadata is T t)
            {
                return t;
            }

            return null;
        }

        public T GetOrCreateAdditionalMetadata<T>(string key) where T : class, new()
        {
            if (Additional.TryGetValue(key, out var metadata) && metadata is T t)
            {
                return t;
            }
            t = new();
            Additional.TryAdd(key, t);
            return t;
        }
    }
}