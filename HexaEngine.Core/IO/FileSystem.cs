namespace HexaEngine.Core.IO
{
    using HexaEngine.Core.Debugging;
    using System;
    using System.Collections.Generic;
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

        public static bool Exists(string path)
        {
            if (path == null) return false;
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
                    ImGuiConsole.Log(ConsoleMessageType.Warning, $"Warning asset {path} is missing!");

                return asset?.GetStream();
            }
            return null;
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

        public static string ReadAllText(string path)
        {
            var fs = Open(path);
            var reader = new StreamReader(fs);
            return reader.ReadToEnd();
        }
    }
}