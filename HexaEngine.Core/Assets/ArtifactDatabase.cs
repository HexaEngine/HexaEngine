namespace HexaEngine.Core.Assets
{
    using System.IO;

    public static class ArtifactDatabase
    {
        private static readonly object _lock = new();
        private static readonly HashSet<Guid> importedSourceAssets = [];
        private static List<Artifact> artifacts = [];
        private static string root;
        private static string cacheRootFolder;
        private static string cacheFolder;
        private static string dbPath;

        private static readonly ManualResetEventSlim initLock = new(false);

        public static void Init(string path)
        {
            root = path;
            cacheRootFolder = Path.Combine(path, ".cache");
            cacheFolder = Path.Combine(cacheRootFolder, "artifacts");

            Directory.CreateDirectory(cacheRootFolder);
            Directory.CreateDirectory(cacheFolder);

            dbPath = Path.Combine(cacheRootFolder, "artifacts.json");

            if (File.Exists(dbPath))
            {
                artifacts = JsonConvert.DeserializeObject<List<Artifact>>(File.ReadAllText(dbPath));
            }

            for (int i = 0; i < artifacts.Count; i++)
            {
                var artifact = artifacts[i];

                if (!importedSourceAssets.Contains(artifact.SourceGuid))
                {
                    importedSourceAssets.Add(artifact.SourceGuid);
                }
            }

            initLock.Set();
        }

        public static void Save()
        {
            lock (_lock)
            {
                File.WriteAllText(dbPath, JsonConvert.SerializeObject(artifacts));
            }
        }

        public static string ProjectRoot => root;

        public static string CacheFolder => cacheFolder;

        public static bool IsImported(Guid source)
        {
            initLock.Wait();
            lock (_lock)
            {
                return importedSourceAssets.Contains(source);
            }
        }

        public static bool Exists(Guid guid)
        {
            initLock.Wait();
            lock (_lock)
            {
                foreach (Artifact artifact in artifacts)
                {
                    if (artifact.Guid == guid)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static IEnumerable<Artifact> GetArtifactsForSource(Guid guid)
        {
            initLock.Wait();
            lock (_lock)
            {
                foreach (Artifact artifact in artifacts)
                {
                    if (artifact.SourceGuid == guid)
                    {
                        yield return artifact;
                    }
                }
            }
        }

        public static Artifact? GetArtifactForSource(Guid guid)
        {
            initLock.Wait();
            lock (_lock)
            {
                foreach (Artifact artifact in artifacts)
                {
                    if (artifact.SourceGuid == guid)
                    {
                        return artifact;
                    }
                }
            }
            return null;
        }

        public static IEnumerable<Artifact> GetArtifactsFromType(AssetType type)
        {
            initLock.Wait();
            lock (_lock)
            {
                foreach (Artifact artifact in artifacts)
                {
                    if (artifact.Type == type)
                    {
                        yield return artifact;
                    }
                }
            }
        }

        public static Artifact? GetArtifact(Guid guid)
        {
            initLock.Wait();
            lock (_lock)
            {
                foreach (Artifact artifact in artifacts)
                {
                    if (artifact.Guid == guid)
                    {
                        return artifact;
                    }
                }
            }
            return null;
        }

        public static void Clear()
        {
            initLock.Reset();
            lock (_lock)
            {
                importedSourceAssets.Clear();
                artifacts.Clear();
            }
        }

        public static void AddArtifact(Artifact artifact)
        {
            initLock.Wait();
            lock (_lock)
            {
                if (!importedSourceAssets.Contains(artifact.SourceGuid))
                {
                    importedSourceAssets.Add(artifact.SourceGuid);
                }
                artifacts.Add(artifact);
                Save();
            }
        }

        public static void RemoveArtifact(Artifact artifact)
        {
            initLock.Wait();
            lock (_lock)
            {
                importedSourceAssets.Remove(artifact.SourceGuid);
                artifacts.Remove(artifact);
                File.Delete(artifact.Path);
                Save();
            }
        }

        public static void RemoveArtifact(Guid guid)
        {
            initLock.Wait();
            lock (_lock)
            {
                for (var i = 0; i < artifacts.Count; i++)
                {
                    var artifact = artifacts[i];
                    if (artifact.Guid == guid)
                    {
                        File.Delete(artifact.Path);
                        artifacts.RemoveAt(i);
                        Save();
                        return;
                    }
                }
            }
        }

        public static void RemoveArtifactsBySource(Guid source)
        {
            lock (_lock)
            {
                for (var i = 0; i < artifacts.Count; i++)
                {
                    var artifact = artifacts[i];
                    if (artifact.SourceGuid == source)
                    {
                        File.Delete(artifact.Path);
                        artifacts.RemoveAt(i);
                        i--;
                    }
                }
                Save();
            }
        }

        public static void RemoveArtifacts(HashSet<Guid> ids)
        {
            lock (_lock)
            {
                for (var i = 0; i < artifacts.Count; i++)
                {
                    var artifact = artifacts[i];
                    if (ids.Contains(artifact.SourceGuid))
                    {
                        File.Delete(artifact.Path);
                        artifacts.RemoveAt(i);
                        i--;
                    }
                }
                Save();
            }
        }
    }
}