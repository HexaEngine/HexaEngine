using HexaEngine.Core.Debugging;

namespace HexaEngine.Core.Assets
{
    using HexaEngine.Core.Debugging;

    public class ImportContext
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(ImportContext));
        private readonly IGuidProvider provider;
        private readonly SourceAssetMetadata sourceAsset;
        private readonly List<Artifact> lastArtifacts = [];
        private readonly List<Guid> toRemove = [];
        private readonly string? importSourcePath;
        private readonly IProgress<float>? progress;

        private readonly string tempDir;

        private struct ImportFileOperation(string tempPath, string target, FileOperationMode mode, IGuidProvider? guidProvider, IProgress<float>? progress)
        {
            public string TempPath = tempPath;
            public string TargetPath = target;
            public FileOperationMode Mode = mode;
            public IGuidProvider? GuidProvider = guidProvider;
            public IProgress<float>? Progress = progress;
        }

        private struct DatabaseOperation(Artifact artifact, DatabaseOperationMode mode)
        {
            public Artifact Artifact = artifact;
            public DatabaseOperationMode Mode = mode;
        }

        private enum DatabaseOperationMode
        {
            Insert,
            Update,
            Delete
        }

        private enum FileOperationMode
        {
            Move,
            MoveOverwrite,
            Import
        }

        private readonly List<ImportFileOperation> fileOperations = [];
        private readonly Dictionary<string, string> tempFileMap = [];
        private readonly List<DatabaseOperation> databaseOperations = [];

        public ImportContext(IGuidProvider provider, SourceAssetMetadata sourceAsset, string? importSourcePath, IProgress<float>? progress)
        {
            this.provider = provider;
            this.sourceAsset = sourceAsset;
            this.importSourcePath = importSourcePath;
            this.progress = progress;
            tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            Logger.Info($"Using temp dir '{tempDir}' for sandbox.");
        }

        public ImportContext(IGuidProvider provider, SourceAssetMetadata sourceAsset, List<Artifact> artifacts, string? importSourcePath, IProgress<float>? progress) : this(provider, sourceAsset, importSourcePath, progress)
        {
            lastArtifacts = artifacts;
            toRemove = artifacts.Select(x => x.Guid).ToList();
            tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            Logger.Info($"Using temp dir '{tempDir}' for sandbox.");
        }

        public string? ImportSourcePath => importSourcePath;

        public SourceAssetMetadata AssetMetadata => sourceAsset;

        public string SourcePath => Path.Combine(ArtifactDatabase.ProjectRoot, sourceAsset.FilePath);

        public Dictionary<string, object> Additional => sourceAsset.Additional;

        public IList<Guid> ToRemoveArtifacts => toRemove;

        public Guid ParentGuid => provider.ParentGuid;

        private int moveProgress = 0;

        internal void Commit()
        {
            try
            {
                for (int i = 0; i < fileOperations.Count; i++)
                {
                    ImportFileOperation operation = fileOperations[i];

                    switch (operation.Mode)
                    {
                        case FileOperationMode.Move:
                            MoveTempFile(operation, false);
                            break;

                        case FileOperationMode.MoveOverwrite:
                            MoveTempFile(operation, true);
                            break;

                        case FileOperationMode.Import:
                            SourceAssetsDatabase.ImportFile(operation.TempPath, operation.TargetPath, operation.GuidProvider, operation.Progress);
                            break;
                    }

                    moveProgress++;
                }

                for (int i = 0; i < databaseOperations.Count; i++)
                {
                    var operation = databaseOperations[i];
                    switch (operation.Mode)
                    {
                        case DatabaseOperationMode.Insert:
                            ArtifactDatabase.AddArtifact(operation.Artifact);
                            break;

                        case DatabaseOperationMode.Update:
                            // do nothing here.
                            break;

                        case DatabaseOperationMode.Delete:
                            ArtifactDatabase.RemoveArtifact(operation.Artifact);
                            break;
                    }
                }
            }
            catch (Exception)
            {
                Rollback();
            }

            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }

            Logger.Info($"Committed sandbox '{sourceAsset}' '{tempDir}'.");
        }

        private static void MoveTempFile(ImportFileOperation tempFile, bool overwrite)
        {
            if (!overwrite && File.Exists(tempFile.TargetPath))
            {
                throw new InvalidOperationException($"Target file '{tempFile.TargetPath}' already exists.");
            }
            File.Move(tempFile.TempPath, tempFile.TargetPath, overwrite);
        }

        internal void Rollback()
        {
            for (int i = 0; i < databaseOperations.Count; i++)
            {
                var operation = databaseOperations[i];
                if (operation.Mode == DatabaseOperationMode.Insert)
                {
                    ArtifactDatabase.RemoveArtifact(operation.Artifact);
                }
            }

            for (int i = 0; i < moveProgress; i++)
            {
                ImportFileOperation operation = fileOperations[i];
                switch (operation.Mode)
                {
                    case FileOperationMode.Move:
                    case FileOperationMode.MoveOverwrite:
                        File.Delete(operation.TargetPath);
                        break;

                    case FileOperationMode.Import:
                        SourceAssetsDatabase.Delete(operation.TempPath, DeleteBehavior.DeleteChildren);
                        break;
                }
            }

            Directory.Delete(tempDir, true);

            Logger.Info($"Rolled back sandbox '{sourceAsset}' '{tempDir}'.");
        }

        private Guid GetGuid(string name)
        {
            return provider.GetGuid(name);
        }

        public void ReportProgress(float value)
        {
            progress?.Report(value);
        }

        private string MapFileToTempFile(string path, FileOperationMode mode, IGuidProvider? guidProvider, IProgress<float>? progress)
        {
            var tempFile = Path.Combine(tempDir, Path.GetFileName(path));
            fileOperations.Add(new(tempFile, path, mode, guidProvider, progress));
            tempFileMap.Add(tempFile, path);
            return tempFile;
        }

        public Artifact EmitArtifact(string name, AssetType type, out string path)
        {
            Artifact? last = lastArtifacts.FirstOrDefault(x => x.Name == name);

            if (last != null)
            {
                databaseOperations.Add(new(last, DatabaseOperationMode.Update));
                toRemove.Remove(last.Guid);
                path = MapFileToTempFile(last.Path, FileOperationMode.MoveOverwrite, null, null);
                return last;
            }
            else
            {
                Guid guid = GetGuid(name);
                Artifact artifact = new(name, ParentGuid, sourceAsset.Guid, guid, type);
                databaseOperations.Add(new(artifact, DatabaseOperationMode.Insert));
                path = MapFileToTempFile(artifact.Path, FileOperationMode.Move, null, null);
                return artifact;
            }
        }

        public Artifact EmitArtifact(string name, Guid guid, AssetType type, out string path)
        {
            Artifact? last = lastArtifacts.FirstOrDefault(x => x.Name == name);

            if (last != null)
            {
                databaseOperations.Add(new(last, DatabaseOperationMode.Update));
                toRemove.Remove(last.Guid);
                path = MapFileToTempFile(last.Path, FileOperationMode.MoveOverwrite, null, null);
                return last;
            }
            else
            {
                Artifact artifact = new(name, ParentGuid, sourceAsset.Guid, guid, type);
                databaseOperations.Add(new(artifact, DatabaseOperationMode.Insert));
                path = MapFileToTempFile(artifact.Path, FileOperationMode.Move, null, null);
                return artifact;
            }
        }

        public Artifact EmitArtifact(string name, AssetType type, out FileStream stream)
        {
            var artifact = EmitArtifact(name, type, out string path);
            stream = File.Create(path);
            return artifact;
        }

        public void ImportChild(string target, Guid childGuid, out string path)
        {
            GuidProvider guidProvider = new(childGuid, sourceAsset.Guid);
            path = MapFileToTempFile(target, FileOperationMode.Import, guidProvider, null);
        }

        public void ImportChild(string target, IGuidProvider guidProvider, out string path)
        {
            path = MapFileToTempFile(target, FileOperationMode.Import, guidProvider, null);
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