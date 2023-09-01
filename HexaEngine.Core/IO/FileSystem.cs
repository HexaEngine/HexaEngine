namespace HexaEngine.Core.IO
{
    using HexaEngine.Core.IO.Assets;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Provides utility methods for interacting with the file system.
    /// </summary>
    public class FileSystem
    {
        private static readonly List<Asset> assetBundles = new();
        private static readonly Dictionary<string, string> fileIndices = new();
        private static readonly List<string> sources = new();

        public static event Action? FileSystemInitialized;

        public static event Action? FileSystemReload;

        public static event Action<string>? SourceAdded;

        public static event Action<string>? SourceRemoved;

        /// <summary>
        /// Initializes the file system by loading asset bundles and creating file indices.
        /// </summary>
        public static void Initialize()
        {
            // Load asset bundles and populate the assetBundles list
            string[] bundles = Directory.GetFiles("assets\\", "*.assets", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < bundles.Length; i++)
            {
                string file = bundles[i];
                assetBundles.AddRange(new AssetArchive(file).Assets);
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
                }
            }

            FileSystemInitialized?.Invoke();
        }

        /// <summary>
        /// Refreshes the file system by recreating the file indices.
        /// </summary>
        public static void Refresh()
        {
            // Clear existing file indices
            fileIndices.Clear();

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
                    }
                }
            }

            FileSystemReload?.Invoke();
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
                }
            }

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

            if (sources.Remove(source))
            {
                Refresh();
                SourceRemoved?.Invoke(source);
                return true;
            }

            return false;
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

            if (fileIndices.ContainsKey(realPath))
            {
                return true;
            }
            else
            {
                var rel = Path.GetRelativePath("assets/", realPath);
                return assetBundles.Find(x => x.Path == rel) != null;
            }
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
                var asset = assetBundles.Find(x => x.Path == rel);

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
                var asset = assetBundles.Find(x => x.Path == rel);

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
            var files = assetBundles.Where(x => x.Path.StartsWith(realPath)).Select(x => x.Path);
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
                var asset = assetBundles.Find(x => x.Path == rel) ?? throw new FileNotFoundException(realPath);
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
                var asset = assetBundles.Find(x => x.Path == rel) ?? throw new FileNotFoundException(realPath);
                throw new NotSupportedException();
            }

            throw new FileNotFoundException(realPath);
        }
    }
}