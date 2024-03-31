namespace HexaEngine.Core.Assets
{
    public class ImportContext
    {
        private readonly IGuidProvider provider;
        private readonly SourceAssetMetadata sourceAsset;
        private readonly List<Artifact> lastArtifacts = [];
        private readonly List<Guid> toRemove = [];
        private readonly string? importSourcePath;
        private readonly IProgress<float>? progress;

        public ImportContext(IGuidProvider provider, SourceAssetMetadata sourceAsset, string? importSourcePath, IProgress<float>? progress)
        {
            this.provider = provider;
            this.sourceAsset = sourceAsset;
            this.importSourcePath = importSourcePath;
            this.progress = progress;
        }

        public ImportContext(IGuidProvider provider, SourceAssetMetadata sourceAsset, List<Artifact> artifacts, string? importSourcePath, IProgress<float>? progress) : this(provider, sourceAsset, importSourcePath, progress)
        {
            lastArtifacts = artifacts;
            toRemove = artifacts.Select(x => x.Guid).ToList();
        }

        public string? ImportSourcePath => importSourcePath;

        public SourceAssetMetadata AssetMetadata => sourceAsset;

        public string SourcePath => Path.Combine(ArtifactDatabase.ProjectRoot, sourceAsset.FilePath);

        public Dictionary<string, object> Additional => sourceAsset.Additional;

        public IList<Guid> ToRemoveArtifacts => toRemove;

        public Guid ParentGuid => provider.ParentGuid;

        private Guid GetGuid(string name)
        {
            return provider.GetGuid(name);
        }

        public void ReportProgress(float value)
        {
            progress?.Report(value);
        }

        public Artifact EmitArtifact(string name, AssetType type, out string path)
        {
            Artifact? last = lastArtifacts.FirstOrDefault(x => x.Name == name);

            if (last != null)
            {
                toRemove.Remove(last.Guid);
                path = last.Path;
                return last;
            }
            else
            {
                Guid guid = GetGuid(name);
                string outputPath = Path.Combine(ArtifactDatabase.CacheFolder, guid.ToString());
                Artifact artifact = new(name, ParentGuid, sourceAsset.Guid, guid, type, outputPath);
                ArtifactDatabase.AddArtifact(artifact);
                path = outputPath;
                return artifact;
            }
        }

        public Artifact EmitArtifact(string name, Guid guid, AssetType type, out string path)
        {
            Artifact? last = lastArtifacts.FirstOrDefault(x => x.Name == name);

            if (last != null)
            {
                toRemove.Remove(last.Guid);
                path = last.Path;
                return last;
            }
            else
            {
                string outputPath = Path.Combine(ArtifactDatabase.CacheFolder, guid.ToString());
                Artifact artifact = new(name, ParentGuid, sourceAsset.Guid, guid, type, outputPath);
                ArtifactDatabase.AddArtifact(artifact);
                path = outputPath;
                return artifact;
            }
        }

        public Artifact EmitArtifact(string name, AssetType type, out FileStream stream)
        {
            var artifact = EmitArtifact(name, type, out string path);
            stream = File.Create(path);
            return artifact;
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