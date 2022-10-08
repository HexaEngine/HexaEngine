namespace HexaEngine.Core.IO
{
    using HexaEngine.Core.Debugging;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;

    public class FileSystem
    {
        private static readonly List<Asset> assetBundles = new();

        static FileSystem()
        {
            foreach (string file in Directory.GetFiles("assets/", "*.assets", SearchOption.TopDirectoryOnly))
            {
                assetBundles.AddRange(new AssetBundle(file).Assets);
            }
        }

        public static bool Exists(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            if (File.Exists(path))
            {
                return true;
            }
            else
            {
                var rel = Path.GetRelativePath("assets/", path);
                return assetBundles.Find(x => x.Path == rel) != null; ;
            }
        }

        public static VirtualStream Open(string path)
        {
            if (File.Exists(path))
            {
                var fs = File.OpenRead(path);

                return new(fs, 0, fs.Length, true);
            }
            else if (!string.IsNullOrWhiteSpace(path))
            {
                var rel = Path.GetRelativePath("assets/", path);
                var asset = assetBundles.Find(x => x.Path == rel);

                if (asset == null)
                {
                    ImGuiConsole.Log(LogSeverity.Warning, $"Warning asset {path} is missing!");
                    throw new FileNotFoundException(path);
                }

                return asset.GetStream();
            }

            throw new FileNotFoundException(path);
        }

        public static bool TryOpen(string path, [NotNullWhen(true)] out VirtualStream? stream)
        {
            if (File.Exists(path))
            {
                var fs = File.OpenRead(path);
                stream = new(fs, 0, fs.Length, true);
                return true;
            }
            else if (!string.IsNullOrWhiteSpace(path))
            {
                var rel = Path.GetRelativePath("assets/", path);
                var asset = assetBundles.Find(x => x.Path == rel);

                if (asset == null)
                {
                    ImGuiConsole.Log(LogSeverity.Warning, $"Warning asset {path} is missing!");
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
            return assetBundles.Where(x => x.Path.StartsWith(path)).Select(x => x.Path).ToArray();
        }

        public static string[] ReadAllLines(string path)
        {
            var fs = Open(path);
            var reader = new StreamReader(fs);
            return reader.ReadToEnd().Split(Environment.NewLine);
        }

        public static byte[] ReadAllBytes(string path)
        {
            var fs = Open(path);
            var buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);
            fs.Close();
            return buffer;
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
    }
}