namespace HexaEngine.Core.Assets
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.IO;
    using System;
    using System.IO;

    public static class SourceAssetsDatabase
    {
        private static readonly object _lock = new();
        private static string rootFolder;
        private static string cacheRootFolder;
        private static string cacheFolder;
        private static readonly List<SourceAssetMetadata> sourceAssets = [];
        private static readonly ManualResetEventSlim initLock = new(false);

        public static void Init(string path)
        {
            rootFolder = path;
            cacheRootFolder = Path.Combine(path, ".cache");
            cacheFolder = Path.Combine(cacheRootFolder, "artifacts");

            Directory.CreateDirectory(cacheRootFolder);
            Directory.CreateDirectory(cacheFolder);

            ArtifactDatabase.Init(path);

            foreach (string file in Directory.GetFiles(path, "*.meta", SearchOption.AllDirectories))
            {
                var metadata = SourceAssetMetadata.LoadMetadata(file);

                if (metadata != null)
                {
                    sourceAssets.Add(metadata);
                }
            }

            foreach (string file in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
            {
                var span = file.AsSpan();

                if (IgnoreFile(span))
                {
                    continue;
                }

                var relative = Path.GetRelativePath(path, file);

                bool found = false;
                for (int i = 0; i < sourceAssets.Count; i++)
                {
                    var asset = sourceAssets[i];
                    if (asset.FilePath == relative)
                    {
                        var crc = FileSystem.GetCrc32HashExtern(file);
                        if (!ArtifactDatabase.IsImported(asset.Guid) || crc != asset.CRC32)
                        {
                            var artifacts = ArtifactDatabase.GetArtifactsForSource(asset.Guid).ToList();
                            ImportInternal(file, asset, artifacts);
                        }

                        asset.CRC32 = crc;
                        asset.Save();

                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    var metadataFile = SourceAssetMetadata.GetMetadataFilePath(file);

                    if (metadataFile == null)
                    {
                        continue;
                    }

                    Logger.Warn($"Couldn't find metadata for file, {file}");
                    AddFile(file, metadataFile);
                }
            }
            initLock.Set();
        }

        private static bool IgnoreFile(ReadOnlySpan<char> span)
        {
            var extension = Path.GetExtension(span);
            return extension == ".meta";
        }

        public static void Clear()
        {
            initLock.Reset();
            ArtifactDatabase.Clear();
            sourceAssets.Clear();
        }

        public static string RootFolder => rootFolder;

        public static string CacheFolder => cacheFolder;

        internal static SourceAssetMetadata AddFile(string path, string metadataFile)
        {
            var crc32 = FileSystem.GetCrc32HashExtern(path);
            var lastModified = File.GetLastWriteTime(path);
            SourceAssetMetadata sourceAsset = SourceAssetMetadata.Create(Path.GetRelativePath(rootFolder, path), lastModified, crc32, metadataFile);

            lock (_lock)
            {
                sourceAssets.Add(sourceAsset);
            }

            ImportInternal(path, sourceAsset);
            return sourceAsset;
        }

        private static void ImportInternal(string path, SourceAssetMetadata sourceAsset, List<Artifact> artifacts)
        {
            var extension = Path.GetExtension(path);

            if (!AssetImporterRegistry.TryGetImporterForFile(extension, out var importer))
            {
                Logger.Trace($"Unrecognised file format, {extension} ({path})");
                return;
            }

            ImportContext context = new(sourceAsset, artifacts);

            importer.Import(TargetPlatform.Windows, context);

            ArtifactDatabase.RemoveArtifacts(context.ToRemoveArtifacts);

            sourceAsset.Save();
        }

        private static void ImportInternal(string path, SourceAssetMetadata sourceAsset)
        {
            var extension = Path.GetExtension(path);

            if (!AssetImporterRegistry.TryGetImporterForFile(extension, out var importer))
            {
                Logger.Trace($"Unrecognised file format, {extension} ({path})");
                return;
            }

            ImportContext context = new(sourceAsset);

            importer.Import(TargetPlatform.Windows, context);

            sourceAsset.Save();
        }

        public static void ImportFile(string path)
        {
            initLock.Wait();
            var filename = Path.GetFileName(path);

            if (!path.StartsWith(rootFolder))
            {
                var newPath = Path.Combine(rootFolder, filename);

                File.Copy(path, newPath);

                var metadataFile = SourceAssetMetadata.GetMetadataFilePath(newPath);

                if (metadataFile == null)
                {
                    File.Delete(newPath);
                    return;
                }

                AddFile(newPath, metadataFile);
            }
            else
            {
                var metadataFile = SourceAssetMetadata.GetMetadataFilePath(path);

                if (metadataFile == null)
                {
                    return;
                }

                AddFile(path, metadataFile);
            }
        }

        public static SourceAssetMetadata CreateFile(string name)
        {
            initLock.Wait();

            var path = Path.Combine(rootFolder, name);

            var metadataFile = SourceAssetMetadata.GetMetadataFilePath(path) ?? throw new InvalidOperationException();
            var metadata = AddFile(path, metadataFile);

            return metadata;
        }

        public static string GetFreeName(string filename)
        {
            var extension = Path.GetExtension(filename);
            var name = Path.GetFileNameWithoutExtension(filename);
            lock (_lock)
            {
                var path = Path.Combine(rootFolder, filename);
                if (!File.Exists(path))
                {
                    return filename;
                }

                int i = 1;
                while (true)
                {
                    string newName = $"{name} {i++}.{extension}";
                    var newPath = Path.Combine(rootFolder, newName);
                    if (!File.Exists(newPath))
                    {
                        return newName;
                    }
                }
            }
        }

        public static void ImportFolder(string path)
        {
            initLock.Wait();
        }

        public static void ImportFileAsync(string path)
        {
            initLock.Wait();
        }

        public static void ImportFolderAsync(string path)
        {
            initLock.Wait();
        }

        public static void Move(string path, string newPath)
        {
            initLock.Wait();
        }

        public static void Delete(string path)
        {
            initLock.Wait();
        }

        public static SourceAssetMetadata? GetMetadata(Guid guid)
        {
            initLock.Wait();
            lock (_lock)
            {
                for (int i = 0; i < sourceAssets.Count; i++)
                {
                    var asset = sourceAssets[i];
                    if (asset.Guid == guid)
                        return asset;
                }
            }
            return null;
        }
    }
}