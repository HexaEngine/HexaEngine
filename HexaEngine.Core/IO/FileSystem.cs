namespace HexaEngine.Core.IO
{
    using HexaEngine.Core.IO.Binary.Archives;
    using System;
    using System.Buffers.Binary;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.IO.Hashing;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;

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

        private static readonly ManualResetEventSlim initLock = new(false);

        /// <summary>
        /// Event raised when the file system has been initialized.
        /// </summary>
        public static event Action? Initialized;

        /// <summary>
        /// Event raised when the file system has been refreshed.
        /// </summary>
        public static event Action? Refreshed;

        /// <summary>
        /// Event raised when any file or directory in the monitored paths has changed.
        /// </summary>
        public static event Action<FileSystemEventArgs>? Changed;

        /// <summary>
        /// Event raised when a file is created in the monitored paths.
        /// </summary>
        public static event Action<FileSystemEventArgs>? FileCreated;

        /// <summary>
        /// Event raised when the content of a file is changed in the monitored paths.
        /// </summary>
        public static event Action<FileSystemEventArgs>? FileChanged;

        /// <summary>
        /// Event raised when a file is deleted from the monitored paths.
        /// </summary>
        public static event Action<FileSystemEventArgs>? FileDeleted;

        /// <summary>
        /// Event raised when a file is renamed in the monitored paths.
        /// </summary>
        public static event Action<RenamedEventArgs>? FileRenamed;

        /// <summary>
        /// Event raised when a new source is added.
        /// </summary>
        public static event Action<string>? SourceAdded;

        /// <summary>
        /// Event raised when a source is removed.
        /// </summary>
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

            initLock.Set();
        }

        /// <summary>
        /// Refreshes the file system by recreating the file indices.
        /// </summary>
        /// <remarks>Normally you don't need to manually refresh, the file system automatically does that for you.</remarks>
        public static void Refresh()
        {
            initLock.Reset();
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
            initLock.Set();
        }

        private static void AddFileSystemWatcher(string path)
        {
            FileSystemWatcher watcher = new(path);
            watcher.EnableRaisingEvents = true;
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.Security;
            watcher.IncludeSubdirectories = true;
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

            initLock.Reset();

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
                            initLock.Set();
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
            initLock.Set();
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

            initLock.Reset();
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

            initLock.Set();

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

        /// <summary>
        /// Gets the asset associated with the specified asset path.
        /// If the asset is not already loaded, it creates a new asset instance, adds it to the internal asset registry,
        /// and increments the reference count.
        /// </summary>
        /// <param name="assetPath">The path of the asset to retrieve.</param>
        /// <returns>The asset associated with the specified path.</returns>
        public static Asset GetAsset(string assetPath)
        {
            initLock.Wait();
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
            initLock.Wait();
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

        /// <summary>
        /// Calculates the CRC32 hash of the file located at the specified path.
        /// </summary>
        /// <param name="path">The path of the file for which to calculate the CRC32 hash.</param>
        /// <returns>The CRC32 hash of the file.</returns>
        public static uint GetCrc32Hash(string path)
        {
            initLock.Wait();
            var stream = OpenRead(path);
            Crc32 crc = new();
            crc.Append(stream);
            Span<byte> buffer = stackalloc byte[4];
            crc.GetCurrentHash(buffer);
            stream.Close();

            return BinaryPrimitives.ReadUInt32LittleEndian(buffer);
        }

        /// <summary>
        /// Calculates the CRC32 hash of the file located at the specified path.
        /// </summary>
        /// <param name="path">The path of the file for which to calculate the CRC32 hash.</param>
        /// <returns>The CRC32 hash of the file.</returns>
        public static uint GetCrc32HashExtern(string path)
        {
            if (!File.Exists(path))
            {
                return 0;
            }
            var stream = File.OpenRead(path);
            Crc32 crc = new();
            crc.Append(stream);
            Span<byte> buffer = stackalloc byte[4];
            crc.GetCurrentHash(buffer);
            stream.Close();

            return BinaryPrimitives.ReadUInt32LittleEndian(buffer);
        }

        /// <summary>
        /// Calculates the CRC32 hash of the text.
        /// </summary>
        /// <param name="text">The text for which to calculate the CRC32 hash.</param>
        /// <returns>The CRC32 hash of the text.</returns>
        public static uint GetCrc32HashFromText(string text)
        {
            Crc32 crc = new();
            crc.Append(MemoryMarshal.AsBytes(text.AsSpan()));
            Span<byte> buffer = stackalloc byte[4];
            crc.GetCurrentHash(buffer);

            return BinaryPrimitives.ReadUInt32LittleEndian(buffer);
        }

        /// <summary>
        /// Gets the relative path of the specified path within the file system.
        /// </summary>
        /// <param name="path">The path to get the relative path for.</param>
        /// <returns>The relative path of the specified path.</returns>
        public static string GetRelativePath(string path)
        {
            initLock.Wait();
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

        /// <summary>
        /// Tries to get the relative path of the specified path relative to the registered sources.
        /// </summary>
        /// <param name="path">The path for which to calculate the relative path.</param>
        /// <param name="relativePath">When this method returns, contains the relative path, if the operation succeeded; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if the relative path was successfully calculated; otherwise, <c>false</c>.</returns>
        public static bool TryGetRelativePath(string path, [NotNullWhen(true)] out string? relativePath)
        {
            initLock.Wait();
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
        public static VirtualStream OpenRead(string path)
        {
            initLock.Wait();
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
        public static bool TryOpenRead(string path, [NotNullWhen(true)] out VirtualStream? stream)
        {
            initLock.Wait();
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

        public static Regex CreateRegexPattern(string pattern)
        {
            // prepare the pattern to the form appropriate for Regex class
            StringBuilder sb = new(pattern);
            // remove superflous occurences of  "?*" and "*?"
            sb.Replace("?*", "*");
            sb.Replace("*?", "*");

            // remove superflous occurences of asterisk '*'
            sb.Replace("**", "*");

            // if only asterisk '*' is left, the mask is ".*"
            if (sb.Equals("*"))
            {
                pattern = ".*";
            }
            else
            {
                // replace '.' with "\."
                sb.Replace(".", "\\.");
                // replaces all occurrences of '*' with ".*"
                sb.Replace("*", ".*[^\\.]");
                // replaces all occurrences of '?' with '.*'
                sb.Replace("?", ".");
                // add "\b" to the beginning and end of the pattern
                sb.Insert(0, "\\b");
                sb.Append("\\b");
                sb.Append('$');
                pattern = sb.ToString();
            }
            return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        /// <summary>
        /// Gets the files at the specified path.
        /// </summary>
        /// <param name="path">The path to get the files for.</param>
        /// <returns>An array of file paths.</returns>
        public static string[] GetFiles(string path)
        {
            initLock.Wait();
            var realPath = Path.GetRelativePath("./", Path.GetFullPath(path));
            realPath = realPath.Replace(@"\\", @"\");
            var files = bundleAssets.Where(x => x.Path.StartsWith(realPath)).Select(x => x.Path);
            return fileIndices.Where(x => x.Key.StartsWith(realPath)).Select(x => x.Key).Union(files).ToArray();
        }

        /// <summary>
        /// Gets the files at the specified path.
        /// </summary>
        /// <param name="path">The path to get the files for.</param>
        /// <param name="pattern"></param>
        /// <returns>An array of file paths.</returns>
        public static string[] GetFiles(string path, string pattern)
        {
            Regex regex = CreateRegexPattern(pattern);
            initLock.Wait();
            var realPath = Path.GetRelativePath("./", Path.GetFullPath(path));
            realPath = realPath.Replace(@"\\", @"\");
            var files = bundleAssets.Where(x => x.Path.StartsWith(realPath) && regex.IsMatch(x.Path)).Select(x => x.Path);
            return fileIndices.Where(x => x.Key.StartsWith(realPath) && regex.IsMatch(x.Key)).Select(x => x.Key).Union(files).ToArray();
        }

        /// <summary>
        /// Gets the full path of <paramref name="path"/>
        /// </summary>
        /// <param name="path">The path to a source.</param>
        /// <returns>The full path</returns>
        public static string GetFullPath(string path)
        {
            initLock.Wait();
            var realPath = Path.GetRelativePath("./", Path.GetFullPath(path));
            realPath = realPath.Replace(@"\\", @"\");

            if (fileIndices.TryGetValue(realPath, out string? value))
            {
                return value;
            }
            else if (File.Exists(path))
            {
                return path;
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
        /// Reads all lines of the file at the specified path.
        /// </summary>
        /// <param name="path">The path of the file to read.</param>
        /// <returns>An bundles of lines read from the file.</returns>
        public static string[] ReadAllLines(string path)
        {
            var fs = OpenRead(path);
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
            var fs = OpenRead(path);
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
            var fs = OpenRead(path);
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
            if (TryOpenRead(path, out var fs))
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
            var fs = OpenRead(path);
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
            if (TryOpenRead(path, out var fs))
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
        public static StreamReader OpenText(string path)
        {
            var fs = OpenRead(path);
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
            initLock.Wait();
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
            initLock.Wait();
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