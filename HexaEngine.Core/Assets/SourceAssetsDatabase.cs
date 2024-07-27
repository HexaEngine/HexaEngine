namespace HexaEngine.Core.Assets
{
    using Hexa.NET.Logging;
    using HexaEngine.Core.IO;
    using System;
    using System.IO;

    public static class FileHandleManager
    {
        private static readonly object _lock = new();
        private static readonly Dictionary<string, AccessHandle> handles = [];
        private static readonly Stack<SemaphoreSlim> pool = new();

        private struct AccessHandle
        {
            public SemaphoreSlim Semaphore;
            public int Refs;
        }

        public static SemaphoreSlim Open(string file)
        {
            Monitor.Enter(_lock);
            if (handles.TryGetValue(file, out var accessHandle))
            {
                accessHandle.Refs++;
                handles[file] = accessHandle;
                Monitor.Exit(_lock);
                accessHandle.Semaphore.Wait();
                return accessHandle.Semaphore;
            }
            else
            {
                pool.TryPop(out accessHandle.Semaphore);
                accessHandle.Semaphore ??= new(1);
                accessHandle.Refs++;
                handles[file] = accessHandle;
                Monitor.Exit(_lock);
                accessHandle.Semaphore.Wait();
                return accessHandle.Semaphore;
            }
        }

        public static void Close(string file, SemaphoreSlim semaphore)
        {
            semaphore.Release();
            Monitor.Enter(_lock);

            AccessHandle handle = handles[file];
            handle.Refs--;
            if (handle.Refs == 0)
            {
                handles.Remove(file);
                if (pool.Count < 256)
                {
                    pool.Push(semaphore);
                }
                else
                {
                    semaphore.Dispose();
                }
            }
            else
            {
                handles[file] = handle;
            }

            Monitor.Exit(_lock);
        }
    }

    public static class SourceAssetsDatabase
    {
        private static readonly ILogger logger = LoggerFactory.GetLogger(nameof(SourceAssetsDatabase));
        private static readonly object _lock = new();
        private static string rootFolder;
        private static string rootAssetsFolder;
        private static string cacheRootFolder;
        private static string cacheFolder;
        private static ThumbnailCache thumbnailCache;
        private static FileSystemWatcher? watcher;
        private static readonly List<SourceAssetMetadata> sourceAssets = [];
        private static readonly Dictionary<Guid, SourceAssetMetadata> guidToSourceAsset = [];
        private static readonly HashSet<string> importedFiles = [];
        private static readonly ManualResetEventSlim initLock = new(false);

        public static Task Init(string path, IProgress<float> progress)
        {
            rootFolder = path;
            rootAssetsFolder = Path.Combine(path, "assets");
            cacheRootFolder = Path.Combine(path, ".cache");
            cacheFolder = Path.Combine(cacheRootFolder, "artifacts");
            thumbnailCache = new(Application.GraphicsDevice, Path.Combine(cacheRootFolder, "thumbcache.bin"), Path.Combine(cacheRootFolder, "thumbcache.index"));

            Directory.CreateDirectory(cacheRootFolder);
            Directory.CreateDirectory(cacheFolder);

            ArtifactDatabase.Init(path);

            watcher = new(path);
            watcher.Changed += WatcherChanged;
            watcher.Created += WatcherChanged;
            watcher.Deleted += WatcherChanged;
            watcher.Renamed += WatcherChanged;
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Attributes | NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Security;

            string[] metafiles = Directory.GetFiles(path, "*.meta", SearchOption.AllDirectories);
            string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            List<Task> tasks = [];

            int progressMax = metafiles.Length + files.Length * 2 - 1;
            int progressValue = 0;

            foreach (string file in metafiles)
            {
                var metadata = SourceAssetMetadata.LoadMetadata(file);

                if (metadata != null)
                {
                    Insert(metadata);
                }

                progress.Report(Interlocked.Increment(ref progressValue) / (float)progressMax);
            }

            foreach (string file in files)
            {
                var span = file.AsSpan();

                progress.Report(Interlocked.Increment(ref progressValue) / (float)progressMax);

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
                        tasks.Add(new(async () =>
                        {
                            await UpdateFileAsync(file, asset, false);
                            progress.Report(Interlocked.Increment(ref progressValue) / (float)progressMax);
                        }));

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

                    logger.Warn($"Couldn't find metadata for file, '{file}'");
                    AddFile(null, file, metadataFile, null, null);
                }

                importedFiles.Add(file);
            }

            progressMax = progressMax - files.Length + tasks.Count;
            progress.Report(progressValue / (float)progressMax);

            for (int i = 0; i < tasks.Count; i++)
            {
                tasks[i].Start();
            }

            return Task.WhenAll(tasks).ContinueWith(x =>
            {
                logger.Info($"Initialized '{path}'");
                initLock.Set();
                ArtifactDatabase.Cleanup();
                ArtifactDatabase.RefreshContent();
            });
        }

        public static ThumbnailCache ThumbnailCache => thumbnailCache;

        public static void Ignore(string path)
        {
            lock (_lock)
            {
                importedFiles.Add(path);
            }
        }

        public static void Unignore(string path)
        {
            lock (_lock)
            {
                importedFiles.Remove(path);
            }
        }

        private static void Insert(SourceAssetMetadata metadata)
        {
            sourceAssets.Add(metadata);
            guidToSourceAsset.Add(metadata.Guid, metadata);
        }

        private static void Remove(SourceAssetMetadata metadata, bool unlinkChildren)
        {
            if (unlinkChildren)
            {
                for (int i = 0; i < sourceAssets.Count; i++)
                {
                    var asset = sourceAssets[i];
                    if (asset.ParentGuid == metadata.Guid)
                    {
                        asset.ParentGuid = default;
                    }
                }
            }

            sourceAssets.Remove(metadata);
            guidToSourceAsset.Remove(metadata.Guid);
        }

        private static void WatcherChanged(object sender, System.IO.FileSystemEventArgs e)
        {
            if (IsIgnored(e.FullPath))
            {
                logger.Trace($"Ignored {e.ChangeType} event for '{e.FullPath}'");
                return;
            }

            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    if (!File.Exists(e.FullPath))
                    {
                        return;
                    }

                    logger.Trace($"Created '{e.FullPath}'");
                    _ = ImportFileAsync(e.FullPath);
                    break;

                case WatcherChangeTypes.Deleted:
                    logger.Trace($"Deleted '{e.FullPath}'");
                    DeleteLeftovers(e.FullPath, DeleteBehavior.UnlinkChildren);
                    break;

                case WatcherChangeTypes.Changed:
                    if (!File.Exists(e.FullPath))
                    {
                        return;
                    }

                    logger.Trace($"Updated '{e.FullPath}'");
                    _ = UpdateAsync(e.FullPath);
                    break;

                case WatcherChangeTypes.Renamed:
                    if (!File.Exists(e.FullPath))
                    {
                        return;
                    }

                    logger.Trace($"Renamed '{e.FullPath}'");
                    System.IO.RenamedEventArgs renamedEvent = (System.IO.RenamedEventArgs)e;
                    Rename(renamedEvent.OldFullPath, renamedEvent.FullPath);
                    break;

                default:
                    return;
            }

            Changed?.Invoke(e);
        }

        public static event Action<System.IO.FileSystemEventArgs>? Changed;

        private static void UpdateFile(string file, SourceAssetMetadata asset, bool force)
        {
            if (asset.IsLocked) // no need for updating. If it's locked it means there is a import/update operation already going on.
            {
                return;
            }

            asset.Lock();
            try
            {
                var crc = FileSystem.GetCrc32HashExtern(file);
                if (!ArtifactDatabase.IsImported(asset.Guid) || crc != asset.CRC32 || force)
                {
                    var artifacts = ArtifactDatabase.GetArtifactsForSource(asset.Guid).ToList();
                    ImportInternal(null, file, asset, artifacts, null, null);
                }

                asset.CRC32 = crc;
                asset.Save();
            }
            finally
            {
                asset.ReleaseLock();
            }
        }

        private static async Task UpdateFileAsync(string file, SourceAssetMetadata asset, bool force)
        {
            if (asset.IsLocked) // no need for updating. If it's locked it means there is a import/update operation already going on.
            {
                return;
            }

            asset.Lock();
            try
            {
                var crc = FileSystem.GetCrc32HashExtern(file);
                if (force || !ArtifactDatabase.IsImported(asset.Guid) || crc != asset.CRC32)
                {
                    var artifacts = ArtifactDatabase.GetArtifactsForSource(asset.Guid).ToList();
                    await ImportInternalAsync(null, file, asset, artifacts, null, null);
                }

                asset.CRC32 = crc;
                asset.Save();
            }
            finally
            {
                asset.ReleaseLock();
            }
        }

        private static bool IgnoreFile(ReadOnlySpan<char> span)
        {
            var extension = Path.GetExtension(span);
            return extension.SequenceEqual(".meta") || extension.SequenceEqual(".tmp", CharComparerIgnoreCase.Instance) || extension.SequenceEqual(".bundle");
        }

        public static bool IsIgnored(string file)
        {
            var subDirName = Path.GetFileName(file.AsSpan());

            if (subDirName.StartsWith(".") || subDirName.SequenceEqual("bin") || subDirName.SequenceEqual("obj") || subDirName.EndsWith("~"))
            {
                return true;
            }

            var dir = Path.GetDirectoryName(file.AsSpan());
            while (!dir.IsEmpty)
            {
                var dirName = Path.GetFileName(dir);
                if (dirName.IsEmpty)
                {
                    break;
                }

                if (dirName.StartsWith(".") || dirName.SequenceEqual("bin") || dirName.SequenceEqual("obj"))
                {
                    return true;
                }

                dir = Path.GetDirectoryName(dir);
            }

            var extension = Path.GetExtension(subDirName);

            if (extension.SequenceEqual(".meta", CharComparerIgnoreCase.Instance) || extension.SequenceEqual(".tmp", CharComparerIgnoreCase.Instance) || extension.SequenceEqual(".bundle", CharComparerIgnoreCase.Instance))
            {
                return true;
            }

            if (!File.Exists(file))
            {
                return false;
            }

            try
            {
                var attributes = File.GetAttributes(file);

                if ((attributes & (FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReadOnly | FileAttributes.Device)) != 0)
                {
                    return true;
                }
            }
            catch (Exception)
            {
            }

            return false;
        }

        public static bool Exists(Guid guid)
        {
            initLock.Wait();
            lock (_lock)
            {
                return guidToSourceAsset.ContainsKey(guid);
            }
        }

        public static void Clear()
        {
            initLock.Reset();
            ArtifactDatabase.Clear();
            sourceAssets.Clear();
            guidToSourceAsset.Clear();
            thumbnailCache?.Dispose();
            if (watcher != null)
            {
                watcher.Changed -= WatcherChanged;
                watcher.Dispose();
                watcher = null;
            }
        }

        public static string RootFolder => rootFolder;

        public static string RootAssetsFolder => rootAssetsFolder;

        public static string CacheFolder => cacheFolder;

        internal static SourceAssetMetadata AddFile(string? sourcePath, string path, string metadataFile, IGuidProvider? provider, IProgress<float>? progress)
        {
            var crc32 = FileSystem.GetCrc32HashExtern(path);
            var lastModified = File.GetLastWriteTime(path);
            SourceAssetMetadata sourceAsset = SourceAssetMetadata.Create(Path.GetRelativePath(rootFolder, path), provider?.ParentGuid ?? default, lastModified, crc32, metadataFile);
            sourceAsset.Lock();

            lock (_lock)
            {
                Insert(sourceAsset);
            }

            ImportInternal(sourcePath, path, sourceAsset, provider, progress);

            sourceAsset.ReleaseLock();
            return sourceAsset;
        }

        internal static async Task<SourceAssetMetadata> AddFileAsync(string? sourcePath, string path, string metadataFile, IGuidProvider? provider, IProgress<float>? progress)
        {
            var crc32 = FileSystem.GetCrc32HashExtern(path);
            var lastModified = File.GetLastWriteTime(path);
            SourceAssetMetadata sourceAsset = SourceAssetMetadata.Create(Path.GetRelativePath(rootFolder, path), provider?.ParentGuid ?? default, lastModified, crc32, metadataFile);
            sourceAsset.Lock();

            lock (_lock)
            {
                Insert(sourceAsset);
            }

            await ImportInternalAsync(sourcePath, path, sourceAsset, provider, progress);

            sourceAsset.ReleaseLock();
            return sourceAsset;
        }

        private static void ImportInternal(string? sourcePath, string path, SourceAssetMetadata sourceAsset, List<Artifact> artifacts, IGuidProvider? provider, IProgress<float>? progress)
        {
            var extension = Path.GetExtension(path);

            if (!AssetImporterRegistry.TryGetImporterForFile(extension, out var importer))
            {
                logger.Trace($"Unrecognised file format, {extension} '{path}'");
                return;
            }

            ImportContext context = new(provider ?? DefaultGuidProvider.Instance, sourceAsset, artifacts, sourcePath, progress);

            importer.Import(TargetPlatform.Windows, context);

            ArtifactDatabase.RemoveArtifacts(context.ToRemoveArtifacts);

            context.Commit();

            sourceAsset.Save();
        }

        private static async Task ImportInternalAsync(string? sourcePath, string path, SourceAssetMetadata sourceAsset, List<Artifact> artifacts, IGuidProvider? provider, IProgress<float>? progress)
        {
            var extension = Path.GetExtension(path);

            if (!AssetImporterRegistry.TryGetImporterForFile(extension, out var importer))
            {
                logger.Trace($"Unrecognised file format, {extension} '{path}'");
                return;
            }

            ImportContext context = new(provider ?? DefaultGuidProvider.Instance, sourceAsset, artifacts, sourcePath, progress);
            try
            {
                await importer.ImportAsync(TargetPlatform.Windows, context);
            }
            catch (Exception)
            {
                context.Rollback();
                sourceAsset.Delete(DeleteBehavior.DeleteChildren);
                return;
            }

            ArtifactDatabase.RemoveArtifacts(context.ToRemoveArtifacts);

            context.Commit();

            sourceAsset.Save();
        }

        private static void ImportInternal(string? sourcePath, string path, SourceAssetMetadata sourceAsset, IGuidProvider? provider, IProgress<float>? progress)
        {
            var extension = Path.GetExtension(path);

            if (!AssetImporterRegistry.TryGetImporterForFile(extension, out var importer))
            {
                logger.Trace($"Unrecognised file format, {extension} '{path}'");
                return;
            }

            ImportContext context = new(provider ?? DefaultGuidProvider.Instance, sourceAsset, sourcePath, progress);
            try
            {
                importer.Import(TargetPlatform.Windows, context);
            }
            catch (Exception)
            {
                context.Rollback();
                sourceAsset.Delete(DeleteBehavior.DeleteChildren);
                return;
            }

            context.Commit();

            sourceAsset.Save();
        }

        private static async Task ImportInternalAsync(string? sourcePath, string path, SourceAssetMetadata sourceAsset, IGuidProvider? provider, IProgress<float>? progress)
        {
            var extension = Path.GetExtension(path);

            if (!AssetImporterRegistry.TryGetImporterForFile(extension, out var importer))
            {
                logger.Trace($"Unrecognised file format, {extension} '{path}'");
                return;
            }

            ImportContext context = new(provider ?? DefaultGuidProvider.Instance, sourceAsset, sourcePath, progress);
            try
            {
                await importer.ImportAsync(TargetPlatform.Windows, context);
            }
            catch (Exception)
            {
                context.Rollback();
                sourceAsset.Delete(DeleteBehavior.DeleteChildren);
                return;
            }

            context.Commit();

            sourceAsset.Save();
        }

        public static SourceAssetMetadata? ImportFile(string path, IGuidProvider? provider = null, IProgress<float>? progress = null)
        {
            initLock.Wait();
            var filename = Path.GetFileName(path);

            string filePath = path;
            bool needCopy = false;
            if (!path.StartsWith(rootFolder))
            {
                needCopy = true;
                filePath = Path.Combine(rootAssetsFolder, filename);
            }

            lock (_lock)
            {
                if (importedFiles.Contains(filePath))
                {
                    return GetMetadata(filePath);
                }
                importedFiles.Add(filePath);
            }

            if (needCopy)
            {
                File.Copy(path, filePath);
            }

            var metadataFile = SourceAssetMetadata.GetMetadataFilePath(filePath);

            if (metadataFile == null)
            {
                if (needCopy)
                {
                    File.Delete(filePath);
                }

                throw new($"Failed to import file '{path}', couldn't create metadata.");
            }

            return AddFile(path, filePath, metadataFile, provider, progress);
        }

        public static SourceAssetMetadata? ImportFile(string path, string outputDir, IGuidProvider? provider = null, IProgress<float>? progress = null)
        {
            initLock.Wait();

            if (!outputDir.StartsWith(rootFolder))
            {
                throw new ArgumentException($"Argument '{nameof(outputDir)}' must be located in '{rootFolder}', but was in '{outputDir}'");
            }

            var filename = Path.GetFileName(path);

            string filePath = path;
            bool needCopy = false;
            if (!path.StartsWith(rootFolder))
            {
                needCopy = true;
                filePath = Path.Combine(rootAssetsFolder, filename);
            }

            lock (_lock)
            {
                if (importedFiles.Contains(filePath))
                {
                    return GetMetadata(filePath);
                }
                importedFiles.Add(filePath);
            }

            if (needCopy)
            {
                File.Copy(path, filePath);
            }

            var metadataFile = SourceAssetMetadata.GetMetadataFilePath(filePath);

            if (metadataFile == null)
            {
                if (needCopy)
                {
                    File.Delete(filePath);
                }

                return null;
            }

            return AddFile(path, filePath, metadataFile, provider, progress);
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

        public static Task<SourceAssetMetadata> ImportFileAsync(string path, IGuidProvider? provider = null, IProgress<float>? progress = null)
        {
            initLock.Wait();
            var filename = Path.GetFileName(path);

            string filePath = path;
            bool needCopy = false;
            if (!path.StartsWith(rootFolder))
            {
                needCopy = true;
                filePath = Path.Combine(rootAssetsFolder, filename);
            }

            lock (_lock)
            {
                if (importedFiles.Contains(filePath))
                {
                    return Task.FromResult(GetMetadata(filePath));
                }
                importedFiles.Add(filePath);
            }

            if (needCopy)
            {
                File.Copy(path, filePath);
            }

            var metadataFile = SourceAssetMetadata.GetMetadataFilePath(filePath);

            if (metadataFile == null)
            {
                if (needCopy)
                {
                    File.Delete(filePath);
                }

                return Task.FromResult<SourceAssetMetadata>(null);
            }

            return AddFileAsync(null, filePath, metadataFile, provider, progress);
        }

        public static Task<SourceAssetMetadata> ImportFileAsync(string path, string outputDir, IGuidProvider? provider = null, IProgress<float>? progress = null)
        {
            initLock.Wait();

            if (!outputDir.StartsWith(rootFolder))
            {
                throw new ArgumentException($"Argument '{nameof(outputDir)}' must be located in '{rootFolder}', but was in '{outputDir}'");
            }

            var filename = Path.GetFileName(path);

            string filePath = path;
            bool needCopy = false;
            if (!path.StartsWith(rootFolder))
            {
                needCopy = true;
                filePath = Path.Combine(rootAssetsFolder, filename);
            }

            lock (_lock)
            {
                if (importedFiles.Contains(filePath))
                {
                    return Task.FromResult(GetMetadata(filePath));
                }
                importedFiles.Add(filePath);
            }

            if (needCopy)
            {
                File.Copy(path, filePath);
            }

            var metadataFile = SourceAssetMetadata.GetMetadataFilePath(filePath);

            if (metadataFile == null)
            {
                if (needCopy)
                {
                    File.Delete(filePath);
                }

                return Task.FromResult<SourceAssetMetadata>(null);
            }

            return AddFileAsync(path, filePath, metadataFile, provider, progress);
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
            var metadata = AddFile(null, path, metadataFile, null, null);

            return metadata;
        }

        public static void Update(string path, bool force)
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

            UpdateFile(fileToUpdate, metadata, force);
        }

        public static async Task UpdateAsync(string path, bool force = false)
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

            await UpdateFileAsync(fileToUpdate, metadata, force);
        }

        public static void Update(SourceAssetMetadata metadata, bool force)
        {
            UpdateFile(Path.Combine(rootFolder, metadata.FilePath), metadata, force);
        }

        public static Task UpdateAsync(SourceAssetMetadata metadata, bool force)
        {
            return UpdateFileAsync(Path.Combine(rootFolder, metadata.FilePath), metadata, force);
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

        public static string GetFullPath(string filename)
        {
            return Path.Combine(rootAssetsFolder, filename);
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

        public static void MoveFolder(string folder, string newPath)
        {
            initLock.Wait();

            foreach (var subFolder in Directory.EnumerateDirectories(folder))
            {
                var newSubPath = Path.Combine(newPath, Path.GetFileName(subFolder));
                Directory.CreateDirectory(newSubPath);
                MoveFolder(subFolder, newSubPath);
            }

            foreach (var file in Directory.EnumerateFiles(folder))
            {
                if (Path.GetExtension(file.AsSpan()).SequenceEqual(".meta"))
                {
                    continue;
                }

                Move(file, Path.Combine(newPath, Path.GetFileName(file)));
            }

            Directory.Delete(folder);
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

            SourceAssetMetadata newMetadata = SourceAssetMetadata.Create(Path.GetRelativePath(rootFolder, path), default, File.GetLastWriteTime(targetLocation), metadata.CRC32, metadataLocation);
            newMetadata.Additional = metadata.Additional.ToDictionary();
            newMetadata.Save();

            ImportInternal(null, targetLocation, newMetadata, null, null);
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

            SourceAssetMetadata newMetadata = SourceAssetMetadata.Create(Path.GetRelativePath(rootFolder, path), default, File.GetLastWriteTime(targetLocation), metadata.CRC32, metadataLocation);
            newMetadata.Additional = metadata.Additional.ToDictionary();
            newMetadata.Save();

            await ImportInternalAsync(null, targetLocation, newMetadata, null, null);
        }

        public static void DeleteLeftovers(string file, DeleteBehavior behavior)
        {
            initLock.Wait();

            SourceAssetMetadata? metaToDelete;

            if (file.StartsWith(rootFolder))
            {
                string relative = Path.GetRelativePath(rootFolder, file);
                metaToDelete = GetMetadata(relative);
            }
            else
            {
                metaToDelete = GetMetadata(file);
            }

            if (metaToDelete == null)
            {
                return;
            }

            DeleteLeftovers(metaToDelete, behavior);
        }

        public static void Delete(string file, DeleteBehavior behavior)
        {
            initLock.Wait();

            if (!File.Exists(file))
            {
                return;
            }

            SourceAssetMetadata? metaToDelete;

            if (file.StartsWith(rootFolder))
            {
                string relative = Path.GetRelativePath(rootFolder, file);
                metaToDelete = GetMetadata(relative);
            }
            else
            {
                metaToDelete = GetMetadata(file);
            }

            if (metaToDelete == null)
            {
                throw new MetadataNotFoundException(file);
            }

            Delete(metaToDelete, behavior);
        }

        public static void Delete(SourceAssetMetadata metaToDelete, DeleteBehavior behavior = DeleteBehavior.UnlinkChildren)
        {
            initLock.Wait();

            string fileToDelete = metaToDelete.GetFullPath();

            if (File.Exists(fileToDelete))
            {
                File.Delete(fileToDelete);
                File.Delete(metaToDelete.MetadataFilePath);
            }

            ArtifactDatabase.RemoveArtifactsBySource(metaToDelete.Guid);

            lock (_lock)
            {
                importedFiles.Remove(fileToDelete);
                if (behavior == DeleteBehavior.DeleteChildren)
                {
                    for (int i = 0; i < sourceAssets.Count; i++)
                    {
                        var asset = sourceAssets[i];
                        if (asset.ParentGuid == metaToDelete.Guid)
                        {
                            Delete(asset, behavior);
                            // needs to be a full reset, since we recursively remove files.
                            i = 0;
                        }
                    }
                }
                Remove(metaToDelete, behavior == DeleteBehavior.UnlinkChildren);
            }
        }

        public static void DeleteLeftovers(SourceAssetMetadata metaToDelete, DeleteBehavior behavior = DeleteBehavior.UnlinkChildren)
        {
            initLock.Wait();

            string fileToDelete = metaToDelete.GetFullPath();

            ArtifactDatabase.RemoveArtifactsBySource(metaToDelete.Guid);

            lock (_lock)
            {
                importedFiles.Remove(fileToDelete);
                if (behavior == DeleteBehavior.DeleteChildren)
                {
                    for (int i = 0; i < sourceAssets.Count; i++)
                    {
                        var asset = sourceAssets[i];
                        if (asset.ParentGuid == metaToDelete.Guid)
                        {
                            Delete(asset, behavior);
                            // needs to be a full reset, since we recursively remove files.
                            i = 0;
                        }
                    }
                }
                Remove(metaToDelete, behavior == DeleteBehavior.UnlinkChildren);
            }

            if (File.Exists(fileToDelete))
            {
                File.Delete(fileToDelete);
            }

            File.Delete(metaToDelete.MetadataFilePath);
        }

        public static void Rename(string oldPath, string newPath)
        {
            SourceAssetMetadata? metadata = GetMetadata(oldPath);
            if (metadata == null)
            {
                return;
            }

            string oldMetadataLocation = metadata.MetadataFilePath;
            string? metadataLocation = SourceAssetMetadata.GetMetadataFilePath(newPath) ?? throw new MetadataPathNotFoundException(newPath);
            string relativeTargetLocation = Path.GetRelativePath(rootFolder, newPath);

            metadata.MetadataFilePath = metadataLocation;
            metadata.FilePath = relativeTargetLocation;

            File.Move(oldMetadataLocation, metadataLocation);
            metadata.Save();
        }

        public static SourceAssetMetadata? GetMetadata(Guid guid)
        {
            initLock.Wait();
            lock (_lock)
            {
                return guidToSourceAsset.TryGetValue(guid, out var metadata) ? metadata : null;
            }
        }

        public static SourceAssetMetadata? GetMetadata(string file)
        {
            initLock.Wait();
            if (Path.IsPathFullyQualified(file))
            {
                file = Path.GetRelativePath(rootFolder, file);
            }
            lock (_lock)
            {
                for (int i = 0; i < sourceAssets.Count; i++)
                {
                    var asset = sourceAssets[i];
                    if (asset.FilePath == file)
                    {
                        return asset;
                    }
                }
            }
            return null;
        }
    }
}