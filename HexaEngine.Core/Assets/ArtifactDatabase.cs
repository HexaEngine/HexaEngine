namespace HexaEngine.Core.Assets
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System.IO;

    public static class ArtifactDatabase
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(ArtifactDatabase));
        private static readonly object _lock = new();
        private static readonly HashSet<Guid> importedSourceAssets = [];
        private static List<Artifact> artifacts = [];
        private static readonly Dictionary<Guid, Artifact> guidToArtifact = [];
#nullable disable
        private static string root;
        private static string cacheFolder;
        private static string dbPath;
#nullable restore
        private static readonly ManualResetEventSlim initLock = new(false);

        internal static void Init(string path)
        {
            root = path;
            string cacheRootFolder = Path.Combine(path, ".cache");
            cacheFolder = Path.Combine(cacheRootFolder, "artifacts");

            Directory.CreateDirectory(cacheRootFolder);
            Directory.CreateDirectory(cacheFolder);

            dbPath = Path.Combine(cacheRootFolder, "artifacts.db");

            if (!File.Exists(dbPath) || !TryRead())
            {
                artifacts = [];
                Save();
            }

            for (int i = 0; i < artifacts.Count; i++)
            {
                var artifact = artifacts[i];

                if (!importedSourceAssets.Contains(artifact.SourceGuid))
                {
                    importedSourceAssets.Add(artifact.SourceGuid);
                }
            }

            Logger.Info($"Initialized '{path}'");
            initLock.Set();
        }

        internal static void Cleanup()
        {
            initLock.Reset();

            lock (_lock)
            {
                bool updated = false;
                for (int i = 0; i < artifacts.Count; i++)
                {
                    Artifact artifact = artifacts[i];

                    if (!SourceAssetsDatabase.Exists(artifact.SourceGuid))
                    {
                        updated = true;
                        try
                        {
                            importedSourceAssets.Remove(artifact.SourceGuid);
                            artifacts.Remove(artifact);
                            guidToArtifact.Remove(artifact.Guid);
                            File.Delete(artifact.Path);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"Failed to cleanup artifact '{artifact}'");
                            Logger.Log(ex);
                        }
                    }
                }

                if (updated)
                {
                    Save();
                }
            }

            initLock.Set();
        }

        private static readonly byte[] MagicNumber = [0x54, 0x72, 0x61, 0x6E, 0x73, 0x41, 0x73, 0x73, 0x65, 0x74, 0x44, 0x42, 0x00];
        private static readonly Version Version = new(1, 0, 0, 0);
        private static readonly Version MinVersion = new(1, 0, 0, 0);

        private static bool TryRead()
        {
            lock (_lock)
            {
                using FileStream fs = File.OpenRead(dbPath);
                try
                {
                    if (!fs.Compare(MagicNumber))
                    {
                        Logger.Error("Magic number mismatch");
                        fs.Close();
                        return false;
                    }

                    if (!fs.CompareVersion(MinVersion, Version, Endianness.LittleEndian, out var version))
                    {
                        Logger.Error($"Version mismatch, file: {version} min: {MinVersion} max: {Version}");
                        fs.Close();
                        return false;
                    }

                    int artifactCount = fs.ReadInt32(Endianness.LittleEndian);
                    if (artifactCount < 0) // indicates data corruption.
                    {
                        fs.Close();
                        return false;
                    }

                    artifacts.Clear();
                    artifacts.Capacity = artifactCount;

                    for (int i = 0; i < artifactCount; i++)
                    {
                        Artifact artifact = Artifact.Read(fs);
                        if (guidToArtifact.TryGetValue(artifact.Guid, out Artifact? value))
                        {
                            Logger.Error($"Duplicate key/data found '{artifact}' and '{value}'");
                            continue;
                        }

                        artifacts.Add(artifact);
                        guidToArtifact.Add(artifact.Guid, artifact);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Critical("Failed to load artifact database.");
                    Logger.Log(ex);
                }
                finally
                {
                    fs.Close();
                }
                return true;
            }
        }

        public static void Save()
        {
            lock (_lock)
            {
                using FileStream fs = File.Create(dbPath);
                try
                {
                    fs.Write(MagicNumber);
                    fs.WriteUInt64(Version, Endianness.LittleEndian);
                    fs.WriteInt32(artifacts.Count, Endianness.LittleEndian);
                    for (int i = 0; i < artifacts.Count; i++)
                    {
                        artifacts[i].Write(fs);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Critical("Failed to save artifact database.");
                    Logger.Log(ex);
                }
                finally
                {
                    fs.Close();
                }
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
                return guidToArtifact.ContainsKey(guid);
            }
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
                return guidToArtifact.TryGetValue(guid, out var artifact) ? artifact : null;
            }
        }

        public static void Clear()
        {
            initLock.Reset();
            lock (_lock)
            {
                importedSourceAssets.Clear();
                artifacts.Clear();
                guidToArtifact.Clear();
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
                if (guidToArtifact.ContainsKey(artifact.Guid))
                {
                    throw new InvalidOperationException($"Artifact with the id '{artifact.Guid}' already exists.");
                }
                artifacts.Add(artifact);
                guidToArtifact.Add(artifact.Guid, artifact);
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
                guidToArtifact.Remove(artifact.Guid);
                File.Delete(artifact.Path);
                Save();
            }
        }

        public static void RemoveArtifact(Guid guid)
        {
            initLock.Wait();
            lock (_lock)
            {
                if (guidToArtifact.TryGetValue(guid, out var artifact))
                {
                    File.Delete(artifact.Path);
                    artifacts.Remove(artifact);
                    guidToArtifact.Remove(guid);
                    Save();
                    return;
                }
            }
        }

        public static void RemoveArtifactsBySource(Guid source)
        {
            initLock.Wait();
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

        public static void RemoveArtifacts(IList<Guid> ids)
        {
            initLock.Wait();
            lock (_lock)
            {
                for (int i = 0; i < ids.Count; i++)
                {
                    var guid = ids[i];
                    if (guidToArtifact.TryGetValue(guid, out var artifact))
                    {
                        File.Delete(artifact.Path);
                        artifacts.Remove(artifact);
                        guidToArtifact.Remove(guid);
                    }
                }
                Save();
            }
        }
    }
}