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
        private static string rootAssetsFolder;
        private static string cacheRootFolder;
        private static string cacheFolder;
        private static FileSystemWatcher? watcher;
        private static readonly List<SourceAssetMetadata> sourceAssets = [];
        private static readonly ManualResetEventSlim initLock = new(false);

        public static void Init(string path)
        {
            rootFolder = path;
            rootAssetsFolder = Path.Combine(path, "assets");
            cacheRootFolder = Path.Combine(path, ".cache");
            cacheFolder = Path.Combine(cacheRootFolder, "artifacts");

            Directory.CreateDirectory(cacheRootFolder);
            Directory.CreateDirectory(cacheFolder);

            ArtifactDatabase.Init(path);

            watcher = new(path);
            watcher.Changed += WatcherChanged;
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Attributes | NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Security;

            foreach (string file in Directory.GetFiles(path, "*.meta", SearchOption.AllDirectories))
            {
                var metadata = SourceAssetMetadata.LoadMetadata(file);

                if (metadata != null)
                {
                    sourceAssets.Add(metadata);
                }
            }

            List<Task> tasks = [];

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
                        tasks.Add(UpdateFileAsync(file, asset));
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

            Task.WaitAll([.. tasks]);

            initLock.Set();
        }

        private static void WatcherChanged(object sender, System.IO.FileSystemEventArgs e)
        {
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    break;

                case WatcherChangeTypes.Deleted:
                    break;

                case WatcherChangeTypes.Changed:
                    break;

                case WatcherChangeTypes.Renamed:
                    break;

                case WatcherChangeTypes.All:
                    break;
            }
        }

        private static void UpdateFile(string file, SourceAssetMetadata asset)
        {
            var crc = FileSystem.GetCrc32HashExtern(file);
            if (!ArtifactDatabase.IsImported(asset.Guid) || crc != asset.CRC32)
            {
                var artifacts = ArtifactDatabase.GetArtifactsForSource(asset.Guid).ToList();
                ImportInternal(file, asset, artifacts);
            }

            asset.CRC32 = crc;
            asset.Save();
        }

        private static async Task UpdateFileAsync(string file, SourceAssetMetadata asset)
        {
            var crc = FileSystem.GetCrc32HashExtern(file);
            if (!ArtifactDatabase.IsImported(asset.Guid) || crc != asset.CRC32)
            {
                var artifacts = ArtifactDatabase.GetArtifactsForSource(asset.Guid).ToList();
                await ImportInternalAsync(file, asset, artifacts);
            }

            asset.CRC32 = crc;
            asset.Save();
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
            if (watcher != null)
            {
                watcher.Changed -= WatcherChanged;
                watcher.Dispose();
                watcher = null;
            }
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

        internal static async Task<SourceAssetMetadata> AddFileAsync(string path, string metadataFile)
        {
            var crc32 = FileSystem.GetCrc32HashExtern(path);
            var lastModified = File.GetLastWriteTime(path);
            SourceAssetMetadata sourceAsset = SourceAssetMetadata.Create(Path.GetRelativePath(rootFolder, path), lastModified, crc32, metadataFile);

            lock (_lock)
            {
                sourceAssets.Add(sourceAsset);
            }

            await ImportInternalAsync(path, sourceAsset);

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

        private static async Task ImportInternalAsync(string path, SourceAssetMetadata sourceAsset, List<Artifact> artifacts)
        {
            var extension = Path.GetExtension(path);

            if (!AssetImporterRegistry.TryGetImporterForFile(extension, out var importer))
            {
                Logger.Trace($"Unrecognised file format, {extension} ({path})");
                return;
            }

            ImportContext context = new(sourceAsset, artifacts);

            await importer.ImportAsync(TargetPlatform.Windows, context);

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

        private static async Task ImportInternalAsync(string path, SourceAssetMetadata sourceAsset)
        {
            var extension = Path.GetExtension(path);

            if (!AssetImporterRegistry.TryGetImporterForFile(extension, out var importer))
            {
                Logger.Trace($"Unrecognised file format, {extension} ({path})");
                return;
            }

            ImportContext context = new(sourceAsset);

            await importer.ImportAsync(TargetPlatform.Windows, context);

            sourceAsset.Save();
        }

        public static void ImportFile(string path)
        {
            initLock.Wait();
            var filename = Path.GetFileName(path);

            if (!path.StartsWith(rootFolder))
            {
                var newPath = Path.Combine(rootAssetsFolder, filename);

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

        public static void ImportFiles(params string[] files)
        {
            initLock.Wait();
            for (int i = 0; i < files.Length; i++)
            {
                ImportFile(files[i]);
            }
        }

        public static void ImportFolder(string folder)
        {
            initLock.Wait();
            ImportFiles(Directory.GetFiles(folder));
        }

        public static async Task ImportFileAsync(string path)
        {
            initLock.Wait();
            var filename = Path.GetFileName(path);

            if (!path.StartsWith(rootFolder))
            {
                var newPath = Path.Combine(rootAssetsFolder, filename);

                File.Copy(path, newPath);

                var metadataFile = SourceAssetMetadata.GetMetadataFilePath(newPath);

                if (metadataFile == null)
                {
                    File.Delete(newPath);
                    return;
                }

                await AddFileAsync(newPath, metadataFile);
            }
            else
            {
                var metadataFile = SourceAssetMetadata.GetMetadataFilePath(path);

                if (metadataFile == null)
                {
                    return;
                }

                await AddFileAsync(path, metadataFile);
            }
        }

        public static async Task ImportFilesAsync(params string[] files)
        {
            initLock.Wait();

            Task[] tasks = new Task[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                tasks[i] = ImportFileAsync(files[i]);
            }

            await Task.WhenAll(tasks);
        }

        public static async Task ImportFolderAsync(string folder)
        {
            initLock.Wait();
            await ImportFilesAsync(Directory.GetFiles(folder));
        }

        public static SourceAssetMetadata CreateFile(string name)
        {
            initLock.Wait();

            var path = Path.Combine(rootAssetsFolder, name);

            var metadataFile = SourceAssetMetadata.GetMetadataFilePath(path) ?? throw new InvalidOperationException();
            var metadata = AddFile(path, metadataFile);

            return metadata;
        }

        public static void Update(string path)
        {
            SourceAssetMetadata? metadata;

            string fileToUpdate;

            if (path.StartsWith(rootFolder))
            {
                fileToUpdate = path;
                string relative = Path.GetRelativePath(rootFolder, path);
                metadata = GetMetadata(relative);
            }
            else
            {
                fileToUpdate = Path.Combine(rootFolder, path);
                metadata = GetMetadata(path);
            }

            if (metadata == null)
            {
                throw new MetadataNotFoundException(path);
            }

            UpdateFile(fileToUpdate, metadata);
        }

        public static Task UpdateAsync(string path)
        {
            SourceAssetMetadata? metadata;

            string fileToUpdate;

            if (path.StartsWith(rootFolder))
            {
                fileToUpdate = path;
                string relative = Path.GetRelativePath(rootFolder, path);
                metadata = GetMetadata(relative);
            }
            else
            {
                fileToUpdate = Path.Combine(rootFolder, path);
                metadata = GetMetadata(path);
            }

            if (metadata == null)
            {
                throw new MetadataNotFoundException(path);
            }

            return UpdateFileAsync(fileToUpdate, metadata);
        }

        public static void Update(SourceAssetMetadata metadata)
        {
            UpdateFile(Path.Combine(rootFolder, metadata.FilePath), metadata);
        }

        public static Task UpdateAsync(SourceAssetMetadata metadata)
        {
            return UpdateFileAsync(Path.Combine(rootFolder, metadata.FilePath), metadata);
        }

        public static string GetFreeName(string filename)
        {
            var extension = Path.GetExtension(filename);
            var name = Path.GetFileNameWithoutExtension(filename);
            lock (_lock)
            {
                var path = Path.Combine(rootAssetsFolder, filename);
                if (!File.Exists(path))
                {
                    return filename;
                }

                int i = 1;
                while (true)
                {
                    string newName = $"{name} {i++}{extension}";
                    var newPath = Path.Combine(rootAssetsFolder, newName);
                    if (!File.Exists(newPath))
                    {
                        return newName;
                    }
                }
            }
        }

        public static void Move(string path, string newPath)
        {
            initLock.Wait();

            string fileToMove;
            string targetLocation;
            string relativeTargetLocation;
            SourceAssetMetadata? metadata;

            if (path.StartsWith(rootFolder))
            {
                fileToMove = path;
                string relative = Path.GetRelativePath(rootFolder, path);
                metadata = GetMetadata(relative);
            }
            else
            {
                fileToMove = Path.Combine(rootFolder, path);
                metadata = GetMetadata(path);
            }

            if (newPath.StartsWith(rootFolder))
            {
                targetLocation = newPath;
                relativeTargetLocation = Path.GetRelativePath(rootFolder, newPath);
            }
            else
            {
                targetLocation = Path.Combine(rootFolder, newPath);
                relativeTargetLocation = newPath;
            }

            if (metadata == null)
            {
                throw new MetadataNotFoundException(path);
            }

            string oldMetadataLocation = metadata.MetadataFilePath;
            string? metadataLocation = SourceAssetMetadata.GetMetadataFilePath(targetLocation) ?? throw new MetadataPathNotFoundException(targetLocation);

            metadata.MetadataFilePath = metadataLocation;
            metadata.FilePath = relativeTargetLocation;
            File.Move(fileToMove, targetLocation);
            File.Move(oldMetadataLocation, metadataLocation);
            metadata.Save();
        }

        public static void Copy(string path, string target, bool overwrite)
        {
            initLock.Wait();

            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }

            string fileToCopy;
            string targetLocation;
            SourceAssetMetadata? metadata;

            if (path.StartsWith(rootFolder))
            {
                fileToCopy = path;
                string relative = Path.GetRelativePath(rootFolder, path);
                metadata = GetMetadata(relative);
            }
            else
            {
                fileToCopy = Path.Combine(rootFolder, path);
                metadata = GetMetadata(path);
            }

            if (metadata == null)
            {
                throw new MetadataNotFoundException(path);
            }

            targetLocation = target.StartsWith(rootFolder) ? path : Path.Combine(rootFolder, target);

            string? metadataLocation = SourceAssetMetadata.GetMetadataFilePath(targetLocation) ?? throw new MetadataPathNotFoundException(targetLocation);

            File.Copy(fileToCopy, targetLocation, overwrite);

            SourceAssetMetadata newMetadata = SourceAssetMetadata.Create(Path.GetRelativePath(rootFolder, path), File.GetLastWriteTime(targetLocation), metadata.CRC32, metadataLocation);
            newMetadata.Additional = metadata.Additional.ToDictionary();
            newMetadata.Save();

            ImportInternal(targetLocation, newMetadata);
        }

        public static async Task CopyAsync(string path, string target, bool overwrite)
        {
            initLock.Wait();

            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }

            string fileToCopy;
            string targetLocation;
            SourceAssetMetadata? metadata;

            if (path.StartsWith(rootFolder))
            {
                fileToCopy = path;
                string relative = Path.GetRelativePath(rootFolder, path);
                metadata = GetMetadata(relative);
            }
            else
            {
                fileToCopy = Path.Combine(rootFolder, path);
                metadata = GetMetadata(path);
            }

            if (metadata == null)
            {
                throw new MetadataNotFoundException(path);
            }

            targetLocation = target.StartsWith(rootFolder) ? path : Path.Combine(rootFolder, target);

            string? metadataLocation = SourceAssetMetadata.GetMetadataFilePath(targetLocation) ?? throw new MetadataPathNotFoundException(targetLocation);

            File.Copy(fileToCopy, targetLocation, overwrite);

            SourceAssetMetadata newMetadata = SourceAssetMetadata.Create(Path.GetRelativePath(rootFolder, path), File.GetLastWriteTime(targetLocation), metadata.CRC32, metadataLocation);
            newMetadata.Additional = metadata.Additional.ToDictionary();
            newMetadata.Save();

            await ImportInternalAsync(targetLocation, newMetadata);
        }

        public static void Delete(string file)
        {
            initLock.Wait();

            if (!File.Exists(file))
            {
                throw new FileNotFoundException(file);
            }

            string fileToDelete;
            SourceAssetMetadata? metaToDelete;

            if (file.StartsWith(rootFolder))
            {
                fileToDelete = file;
                string relative = Path.GetRelativePath(rootFolder, file);
                metaToDelete = GetMetadata(relative);
            }
            else
            {
                fileToDelete = Path.Combine(rootFolder, file);
                metaToDelete = GetMetadata(file);
            }

            if (metaToDelete == null)
            {
                return;
            }

            ArtifactDatabase.RemoveArtifactsBySource(metaToDelete.Guid);

            lock (_lock)
            {
                sourceAssets.Remove(metaToDelete);
            }

            File.Delete(fileToDelete);
            File.Delete(metaToDelete.MetadataFilePath);
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

        public static SourceAssetMetadata? GetMetadata(string file)
        {
            initLock.Wait();
            lock (_lock)
            {
                for (int i = 0; i < sourceAssets.Count; i++)
                {
                    var asset = sourceAssets[i];
                    if (asset.FilePath == file)
                        return asset;
                }
            }
            return null;
        }
    }
}