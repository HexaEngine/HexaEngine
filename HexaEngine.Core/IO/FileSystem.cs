namespace HexaEngine.Core.IO
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.IO.Assets;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;

    public class FileSystem
    {
        private static readonly List<Asset> assetBundles = new();
        private static readonly Dictionary<string, string> fileIndices = new();
        private static readonly List<string> sources = new();

        public static void Initialize()
        {
            foreach (string file in Directory.GetFiles("assets\\", "*.assets", SearchOption.TopDirectoryOnly))
            {
                assetBundles.AddRange(new AssetArchive(file).Assets);
            }
            foreach (string dir in Directory.GetDirectories("assets\\", "*", SearchOption.TopDirectoryOnly))
            {
                foreach (string file in Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories))
                {
                    var abs = Path.GetFullPath(file);
                    var rel = Path.GetRelativePath(dir, abs);
                    var log = Path.Combine("assets\\", rel);

                    if (fileIndices.ContainsKey(log))
                    {
                        fileIndices[log] = abs;
                    }
                    else
                    {
                        fileIndices.Add(log, abs);
                    }
                }
            }
        }

        public static void Refresh()
        {
            fileIndices.Clear();
            foreach (string dir in Directory.GetDirectories("assets\\", "*", SearchOption.TopDirectoryOnly))
            {
                foreach (string file in Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories))
                {
                    var abs = Path.GetFullPath(file);
                    var rel = Path.GetRelativePath(dir, abs);
                    var log = Path.Combine("assets\\", rel);

                    if (fileIndices.ContainsKey(log))
                    {
                        fileIndices[log] = abs;
                    }
                    else
                    {
                        fileIndices.Add(log, abs);
                    }
                }
            }

            foreach (var source in sources)
            {
                foreach (string dir in Directory.GetDirectories(source, "*", SearchOption.TopDirectoryOnly))
                {
                    foreach (string file in Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories))
                    {
                        var abs = Path.GetFullPath(file);
                        var rel = Path.GetRelativePath(source, abs);
                        var log = Path.Combine("assets\\", rel);

                        if (fileIndices.ContainsKey(log))
                        {
                            fileIndices[log] = abs;
                        }
                        else
                        {
                            fileIndices.Add(log, abs);
                        }
                    }
                }
            }
        }

        public static void AddSource(string source)
        {
            sources.Add(source);
            foreach (string dir in Directory.GetDirectories(source, "*", SearchOption.TopDirectoryOnly))
            {
                foreach (string file in Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories))
                {
                    var abs = Path.GetFullPath(file);
                    var rel = Path.GetRelativePath(source, abs);
                    var log = Path.Combine("assets\\", rel);

                    if (fileIndices.ContainsKey(log))
                    {
                        fileIndices[log] = abs;
                    }
                    else
                    {
                        fileIndices.Add(log, abs);
                    }
                }
            }
        }

        public static void RemoveSource(string source)
        {
            sources.Remove(source);
            Refresh();
        }

        public static bool Exists(string? path)
        {
#pragma warning disable CS8604 // Possible null reference argument for parameter 'path' in 'string Path.GetFullPath(string path)'.
            var realPath = Path.GetRelativePath("./", Path.GetFullPath(path));
#pragma warning restore CS8604 // Possible null reference argument for parameter 'path' in 'string Path.GetFullPath(string path)'.
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
                return assetBundles.Find(x => x.Path == rel) != null; ;
            }
        }

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

                if (asset == null)
                {
                    throw new FileNotFoundException(realPath);
                }

                return asset.GetStream();
            }

            throw new FileNotFoundException(realPath);
        }

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

        public static string[] GetFiles(string path)
        {
            var realPath = Path.GetRelativePath("./", Path.GetFullPath(path));
            realPath = realPath.Replace(@"\\", @"\");
            var files = assetBundles.Where(x => x.Path.StartsWith(realPath)).Select(x => x.Path);
            return fileIndices.Where(x => x.Key.StartsWith(realPath)).Select(x => x.Key).Union(files).ToArray();
        }

        public static string[] ReadAllLines(string path)
        {
            var fs = Open(path);
            var reader = new StreamReader(fs);
            var result = reader.ReadToEnd().Split(Environment.NewLine);
            reader.Close();
            fs.Close();
            return result;
        }

        public static byte[] ReadAllBytes(string path)
        {
            var fs = Open(path);
            var buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);
            fs.Close();
            return buffer;
        }

        public static unsafe FileBlob ReadBlob(string path)
        {
            var fs = Open(path);
            var blob = new FileBlob((nint)fs.Length);
            fs.Read(blob.AsSpan());
            fs.Close();
            return blob;
        }

        public static bool ReadAllLines(string path, [NotNullWhen(true)] out string[]? lines)
        {
            if (TryOpen(path, out var fs))
            {
                var reader = new StreamReader(fs);
                lines = reader.ReadToEnd().Split(Environment.NewLine);
                return true;
            }

            lines = null;
            return false;
        }

        public static string ReadAllText(string path)
        {
            var fs = Open(path);
            var reader = new StreamReader(fs);
            var result = reader.ReadToEnd();
            reader.Close();
            fs.Close();
            return result;
        }

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

        public static StreamReader OpenRead(string path)
        {
            var fs = Open(path);
            var reader = new StreamReader(fs);
            return reader;
        }

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