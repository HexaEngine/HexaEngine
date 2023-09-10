namespace HexaEngine.Core.IO
{
    using HexaEngine.Core.IO.Assets;
    using System;
    using System.Buffers.Binary;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.IO.Hashing;
    using System.Linq;

    /// <summary>
    /// Provides utility methods for interacting with the file system.
    /// </summary>
    public class FileSystem
    {
        private static readonly List<BundleAsset> bundleAssets = new();
        private static readonly Dictionary<string, string> fileIndices = new();
        private static readonly HashSet<string> fileIndicesHashes = new();
        private static readonly List<string> sources = new();

        private static readonly List<FileSystemWatcher> watchers = new();

        private static readonly ConcurrentDictionary<string, Asset> pathToAssets = new();
        private static readonly List<Asset> assets = new();

        public static event Action? Initialized;

        public static event Action? Refreshed;

        public static event Action<FileSystemEventArgs>? Changed;

        public static event Action<FileSystemEventArgs>? FileCreated;

        public static event Action<FileSystemEventArgs>? FileChanged;

        public static event Action<FileSystemEventArgs>? FileDeleted;

        public static event Action<RenamedEventArgs>? FileRenamed;

        public static event Action<string>? SourceAdded;

        public static event Action<string>? SourceRemoved;

        /// <summary>
        /// Initializes the file system by loading asset bundles and creating file indices.
        /// </summary>
        public static void Initialize()
        {
            AddFileSystemWatcher("assets\\");

            // Load asset bundles and populate the bundleAssets list
            string[] bundles = Directory.GetFiles("assets\\", "*.assets", SearchOption.TopDirectoryOnly);

            for (int i = 0; i < bundles.Length; i++)
            {
                string file = bundles[i];
                bundleAssets.AddRange(new AssetArchive(file).Assets);
            }

            // Create file indices for the files in the "assets" directory and its subdirectories
            string[] directories = Directory.GetDirectories("assets\\", "*", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < directories.Length; i++)
            {
                string dir = directories[i];
                string[] files = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);
                for (int j = 0; j < files.Length; j++)
                {
                    string file = files[j];
                    var abs = Path.GetFullPath(file);
                    var rel = Path.GetRelativePath(dir, abs);
                    var log = Path.Combine("assets\\", rel);

                    if (!fileIndices.TryAdd(log, abs))
                    {
                        fileIndices[log] = abs;
                    }
                    else
                    {
                        fileIndicesHashes.Add(log);
                    }
                }
            }

            for (int i = 0; i < assets.Count; i++)
            {
                assets[i].Refresh();
            }

            Initialized?.Invoke();
        }

        /// <summary>
        /// Refreshes the file system by recreating the file indices.
        /// </summary>
        public static void Refresh()
        {
            // Clear existing file indices
            fileIndices.Clear();
            fileIndicesHashes.Clear();

            {
                // Create file indices for the files in the "assets" directory and its subdirectories
                string[] directories = Directory.GetDirectories("assets\\", "*", SearchOption.TopDirectoryOnly);
                for (int i = 0; i < directories.Length; i++)
                {
                    string dir = directories[i];
                    string[] files = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);
                    for (int j = 0; j < files.Length; j++)
                    {
                        string file = files[j];
                        var abs = Path.GetFullPath(file);
                        var rel = Path.GetRelativePath(dir, abs);
                        var log = Path.Combine("assets\\", rel);

                        if (!fileIndices.TryAdd(log, abs))
                        {
                            fileIndices[log] = abs;
                        }
                        else
                        {
                            fileIndicesHashes.Add(log);
                        }
                    }
                }
            }

            // Create file indices for the files in additional sources
            for (int i = 0; i < sources.Count; i++)
            {
                string source = sources[i];
                string[] directories = Directory.GetDirectories(source, "*", SearchOption.TopDirectoryOnly);
                for (int j = 0; j < directories.Length; j++)
                {
                    string dir = directories[j];
                    string[] files = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);
                    for (int ii = 0; ii < files.Length; ii++)
                    {
                        string file = files[ii];
                        var abs = Path.GetFullPath(file);
                        var rel = Path.GetRelativePath(source, abs);
                        var log = Path.Combine("assets\\", rel);

                        if (!fileIndices.TryAdd(log, abs))
                        {
                            fileIndices[log] = abs;
                        }
                        else
                        {
                            fileIndicesHashes.Add(log);
                        }
                    }
                }
            }

            for (int i = 0; i < assets.Count; i++)
            {
                assets[i].Refresh();
            }

            Refreshed?.Invoke();
        }

        private static void AddFileSystemWatcher(string path)
        {
            FileSystemWatcher watcher = new(path);
            watcher.EnableRaisingEvents = true;
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.Security;
            watcher.Changed += FileSystemWatcherChanged;
            watchers.Add(watcher);
        }

        private static void RemoveFileSystemWatcher(string path)
        {
            var watcher = watchers.Find(x => x.Path == path);
            if (watcher == null)
            {
                return;
            }
            watcher.Changed -= FileSystemWatcherChanged;
            watcher.Dispose();
            watchers.Remove(watcher);
        }

        private static void FileSystemWatcherChanged(object sender, System.IO.FileSystemEventArgs e)
        {
            // we don't care about directories.
            if (Path.EndsInDirectorySeparator(e.FullPath))
            {
                return;
            }

            // if the file has no relative path that means the file is not monitored.
            if (!TryGetRelativePath(e.FullPath, out var relativePath))
            {
                return;
            }

            lock (pathToAssets)
            {
                if (pathToAssets.TryGetValue(relativePath, out var asset))
                {
                    asset.Refresh();
                }
            }

            FileSystemEventArgs? args = null;

            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    {
                        args = new(relativePath, FileSystemChangeTypes.Created);
                        FileCreated?.Invoke(args);
                    }
                    break;

                case WatcherChangeTypes.Deleted:
                    {
                        args = new(relativePath, FileSystemChangeTypes.Deleted);
                        FileDeleted?.Invoke(args);
                    }
                    break;

                case WatcherChangeTypes.Changed:
                    {
                        args = new(relativePath, FileSystemChangeTypes.Changed);
                        FileChanged?.Invoke(args);
                    }
                    break;

                case WatcherChangeTypes.Renamed:
                    if (e is System.IO.RenamedEventArgs eventArgs)
                    {
                        if (!TryGetRelativePath(eventArgs.OldFullPath, out var relativePathOld))
                        {
                            return;
                        }

                        var renamed = new RenamedEventArgs(relativePathOld, relativePath);
                        args = renamed;
                        FileRenamed?.Invoke(renamed);
                    }
                    break;

                default:
                    return;
            }

            if (args == null)
            {
                return;
            }

            Changed?.Invoke(args);
        }

        /// <summary>
        /// Adds an additional source directory to the file system.
        /// </summary>
        /// <param name="source">The path to the additional source directory.</param>
        public static void AddSource(string? source)
        {
            if (source == null || !Directory.Exists(source))
            {
                return;
            }

            sources.Add(source);

            // Create file indices for the files in the additional source directory and its subdirectories
            string[] directories = Directory.GetDirectories(source, "*", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < directories.Length; i++)
            {
                string dir = directories[i];
                string[] files = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);
                for (int j = 0; j < files.Length; j++)
                {
                    string file = files[j];
                    var abs = Path.GetFullPath(file);
                    var rel = Path.GetRelativePath(source, abs);
                    var log = Path.Combine("assets\\", rel);

                    if (!fileIndices.TryAdd(log, abs))
                    {
                        fileIndices[log] = abs;
                    }
                    else
                    {
                        fileIndicesHashes.Add(log);
                    }
                }
            }

            for (int i = 0; i < assets.Count; i++)
            {
                assets[i].Refresh();
            }

            AddFileSystemWatcher(source);

            SourceAdded?.Invoke(source);
        }

        /// <summary>
        /// Removes an additional source directory from the file system.
        /// </summary>
        /// <param name="source">The path to the additional source directory to remove.</param>
        public static bool RemoveSource(string? source)
        {
            if (source == null)
            {
                return false;
            }

            RemoveFileSystemWatcher(source);

            if (sources.Remove(source))
            {
                Refresh();
                SourceRemoved?.Invoke(source);
                return true;
            }

            return false;
        }

        public static Asset GetAsset(string assetPath)
        {
            lock (pathToAssets)
            {
                if (pathToAssets.TryGetValue(assetPath, out var asset))
                {
                    asset.AddRef();
                    return asset;
                }
                else
                {
                    asset = new(assetPath);
                    asset.AddRef();
                    pathToAssets.TryAdd(assetPath, asset);
                    assets.Add(asset);
                    return asset;
                }
            }
        }

        internal static void DestroyAsset(Asset asset)
        {
            lock (pathToAssets)
            {
                pathToAssets.Remove(asset.FullPath, out _);
                assets.Remove(asset);
            }
        }

        /// <summary>
        /// Checks if a file or directory exists at the specified path.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns><see langword="true"/> if the file or directory exists, otherwise <see langword="false"/>.</returns>
        public static bool Exists(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            var realPath = Path.GetRelativePath("./", Path.GetFullPath(path));

            if (string.IsNullOrWhiteSpace(realPath))
            {
                return false;
            }

            realPath = realPath.Replace(@"\\", @"\");

            if (fileIndicesHashes.Contains(realPath))
            {
                return true;
            }
            else
            {
                var rel = Path.GetRelativePath("assets/", realPath);
                return bundleAssets.Find(x => x.Path == rel) != null;
            }
        }

        public static uint GetCrc32Hash(string path)
        {
            var stream = Open(path);
            Crc32 crc = new();
            crc.Append(stream);
            Span<byte> buffer = stackalloc byte[4];
            crc.GetCurrentHash(buffer);
            stream.Close();

            return BinaryPrimitives.ReadUInt32LittleEndian(buffer);
        }

        /// <summary>
        /// Gets the relative path of the specified path within the file system.
        /// </summary>
        /// <param name="path">The path to get the relative path for.</param>
        /// <returns>The relative path of the specified path.</returns>
        public static string GetRelativePath(string path)
        {
            for (int i = 0; i < sources.Count; i++)
            {
                var rel = Path.GetRelativePath(sources[i], path);
                if (rel == path)
                {
                    continue;
                }

                return rel;
            }
            return path;
        }

        public static bool TryGetRelativePath(string path, [NotNullWhen(true)] out string? relativePath)
        {
            for (int i = 0; i < sources.Count; i++)
            {
                var rel = Path.GetRelativePath(sources[i], path);
                if (rel == path)
                {
                    continue;
                }
                relativePath = rel;
                return true;
            }
            relativePath = null;
            return false;
        }

        /// <summary>
        /// Opens a virtual stream for reading the file at the specified path.
        /// </summary>
        /// <param name="path">The path of the file to open.</param>
        /// <returns>A <see cref="VirtualStream"/> for reading the file.</returns>
        public static VirtualStream Open(string path)
        {
            var realPath = Path.GetRelativePath("./", Path.GetFullPath(path));
            realPath = realPath.Replace(@"\\", @"\");

            if (fileIndices.TryGetValue(realPath, out string? value))
            {
                var fs = File.OpenRead(value);

                return new(fs, 0, fs.Length);
            }
            else if (File.Exists(path))
            {
                var fs = File.OpenRead(path);

                return new(fs, 0, fs.Length);
            }
            else if (!string.IsNullOrWhiteSpace(realPath))
            {
                var rel = Path.GetRelativePath("assets/", realPath);
                var asset = bundleAssets.Find(x => x.Path == rel);

                return asset == null ? throw new FileNotFoundException(realPath) : asset.GetStream();
            }

            throw new FileNotFoundException(realPath);
        }

        /// <summary>
        /// Tries to open a virtual stream for reading the file at the specified path.
        /// </summary>
        /// <param name="path">The path of the file to open.</param>
        /// <param name="stream">When this method returns, contains the <see cref="VirtualStream"/> for reading the file, if the file exists; otherwise, the default _value.</param>
        /// <returns><see langword="true"/> if the file was successfully opened; otherwise, <see langword="false"/>.</returns>
        public static bool TryOpen(string path, [NotNullWhen(true)] out VirtualStream? stream)
        {
            var realPath = Path.GetRelativePath("./", Path.GetFullPath(path));
            realPath = realPath.Replace(@"\\", @"\");

            if (fileIndices.TryGetValue(realPath, out string? value))
            {
                var fs = File.OpenRead(value);
                stream = new(fs, 0, fs.Length);
                return true;
            }
            else if (File.Exists(path))
            {
                var fs = File.OpenRead(path);
                stream = new(fs, 0, fs.Length);
                return true;
            }
            else if (!string.IsNullOrWhiteSpace(realPath))
            {
                var rel = Path.GetRelativePath("assets/", realPath);
                var asset = bundleAssets.Find(x => x.Path == rel);

                if (asset == null)
                {
                    stream = default;
                    return false;
                }

                stream = asset.GetStream();
                return true;
            }

            stream = default;
            return false;
        }

        /// <summary>
        /// Gets the files at the specified path.
        /// </summary>
        /// <param name="path">The path to get the files for.</param>
        /// <returns>An bundles of file paths.</returns>
        public static string[] GetFiles(string path)
        {
            var realPath = Path.GetRelativePath("./", Path.GetFullPath(path));
            realPath = realPath.Replace(@"\\", @"\");
            var files = bundleAssets.Where(x => x.Path.StartsWith(realPath)).Select(x => x.Path);
            return fileIndices.Where(x => x.Key.StartsWith(realPath)).Select(x => x.Key).Union(files).ToArray();
        }

        /// <summary>
        /// Reads all lines of the file at the specified path.
        /// </summary>
        /// <param name="path">The path of the file to read.</param>
        /// <returns>An bundles of lines read from the file.</returns>
        public static string[] ReadAllLines(string path)
        {
            var fs = Open(path);
            var reader = new StreamReader(fs);
            var result = reader.ReadToEnd().Split(Environment.NewLine);
            reader.Close();
            fs.Close();
            return result;
        }

        /// <summary>
        /// Reads all bytes of the file at the specified path.
        /// </summary>
        /// <param name="path">The path of the file to read.</param>
        /// <returns>An bundles of bytes read from the file.</returns>
        public static byte[] ReadAllBytes(string path)
        {
            var fs = Open(path);
            var buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);
            fs.Close();
            return buffer;
        }

        /// <summary>
        /// Reads the file at the specified path into a <see cref="FileBlob"/>.
        /// </summary>
        /// <param name="path">The path of the file to read.</param>
        /// <returns>A <see cref="FileBlob"/> containing the file data.</returns>
        public static unsafe FileBlob ReadBlob(string path)
        {
            var fs = Open(path);
            var blob = new FileBlob((nint)fs.Length);
            fs.Read(blob.AsSpan());
            fs.Close();
            return blob;
        }

        /// <summary>
        /// Tries to read all lines of the file at the specified path.
        /// </summary>
        /// <param name="path">The path of the file to read.</param>
        /// <param name="lines">When this method returns, contains an bundles of lines read from the file if the file exists; otherwise, the default _value.</param>
        /// <returns><see langword="true"/> if the file was successfully read; otherwise, <see langword="false"/>.</returns>
        public static bool TryReadAllLines(string path, [NotNullWhen(true)] out string[]? lines)
        {
            if (TryOpen(path, out var fs))
            {
                var reader = new StreamReader(fs);
                lines = reader.ReadToEnd().Split(Environment.NewLine);
                reader.Dispose();
                return true;
            }

            lines = null;
            return false;
        }

        /// <summary>
        /// Reads all text from the file at the specified path.
        /// </summary>
        /// <param name="path">The path of the file to read.</param>
        /// <returns>The content of the file as a string.</returns>
        public static string ReadAllText(string path)
        {
            var fs = Open(path);
            var reader = new StreamReader(fs);
            var result = reader.ReadToEnd();
            reader.Close();
            fs.Close();
            return result;
        }

        /// <summary>
        /// Tries to read all text from the file at the specified path.
        /// </summary>
        /// <param name="path">The path of the file to read.</param>
        /// <param name="text">When this method returns, contains the content of the file as a string if the file exists; otherwise, the default _value.</param>
        /// <returns><see langword="true"/> if the file was successfully read; otherwise, <see langword="false"/>.</returns>
        public static bool TryReadAllText(string path, [NotNullWhen(true)] out string? text)
        {
            if (TryOpen(path, out var fs))
            {
                var reader = new StreamReader(fs);
                text = reader.ReadToEnd();
                return true;
            }
            text = null;
            return false;
        }

        /// <summary>
        /// Opens a <see cref="StreamReader"/> for reading the file at the specified path.
        /// </summary>
        /// <param name="path">The path of the file to open.</param>
        /// <returns>A <see cref="StreamReader"/> for reading the file.</returns>
        public static StreamReader OpenRead(string path)
        {
            var fs = Open(path);
            var reader = new StreamReader(fs);
            return reader;
        }

        /// <summary>
        /// Opens a <see cref="Stream"/> for writing to the file at the specified path.
        /// </summary>
        /// <param name="path">The path of the file to open.</param>
        /// <returns>A <see cref="Stream"/> for writing to the file.</returns>
        public static Stream OpenWrite(string path)
        {
            var realPath = Path.GetRelativePath("./", Path.GetFullPath(path));
            realPath = realPath.Replace(@"\\", @"\");

            if (fileIndices.TryGetValue(realPath, out string? value))
            {
                var fs = File.OpenWrite(value);
                return fs;
            }
            else if (File.Exists(path))
            {
                var fs = File.OpenWrite(path);
                return fs;
            }
            else if (!string.IsNullOrWhiteSpace(realPath))
            {
                var rel = Path.GetRelativePath("assets/", realPath);
                var asset = bundleAssets.Find(x => x.Path == rel) ?? throw new FileNotFoundException(realPath);
                throw new NotSupportedException();
            }

            throw new FileNotFoundException(realPath);
        }

        /// <summary>
        /// Creates a <see cref="Stream"/> for writing a new file at the specified path.
        /// </summary>
        /// <param name="path">The path of the file to create.</param>
        /// <returns>A <see cref="Stream"/> for writing the new file.</returns>
        public static Stream Create(string path)
        {
            var realPath = Path.GetRelativePath("./", Path.GetFullPath(path));
            realPath = realPath.Replace(@"\\", @"\");

            if (fileIndices.TryGetValue(realPath, out string? value))
            {
                var fs = File.Create(value);
                return fs;
            }
            else if (File.Exists(path))
            {
                var fs = File.Create(path);
                return fs;
            }
            else if (!string.IsNullOrWhiteSpace(realPath))
            {
                var rel = Path.GetRelativePath("assets/", realPath);
                var asset = bundleAssets.Find(x => x.Path == rel) ?? throw new FileNotFoundException(realPath);
                throw new NotSupportedException();
            }

            throw new FileNotFoundException(realPath);
        }
    }
}