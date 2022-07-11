namespace AssetsBundler
{
    using System;
    using System.IO;

    public class AssetBundle
    {
        public AssetBundle(string path)
        {
            var fs = File.OpenRead(path);
            var count = fs.ReadInt();
            Assets = new Asset[count];
            for (int i = 0; i < count; i++)
            {
                var apath = fs.ReadString();
                var length = fs.ReadInt64();
                var data = fs.Read(length);
                Assets[i] = new Asset() { Path = apath, Data = data };
            }
        }

        public Asset[] Assets { get; }

        public void Extract(string path)
        {
            var root = new DirectoryInfo(path);
            foreach (Asset asset in Assets)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(root.FullName + asset.Path));
                var fs = File.Create(root.FullName + asset.Path);
                fs.Write(asset.Data);
                fs.Flush();
                fs.Close();
                fs.Dispose();
            }
        }

        public static void CreateFrom(string path)
        {
            var root = new DirectoryInfo(path);

            foreach (var dir in root.GetDirectories())
            {
                int i = 0;
                var fs = File.Create(dir.Name + ".assets");
                fs.Position = 4; foreach (var file in root.GetFiles("*.*", SearchOption.AllDirectories))
                {
                    if (file.Extension == ".assets")
                        continue;
                    var ts = file.OpenRead();
                    fs.WriteString(Path.GetRelativePath(path, file.FullName));
                    fs.WriteInt64(ts.Length);
                    ts.CopyTo(fs);
                    i++;
                }
                fs.Position = 0;
                fs.WriteInt(i);
                fs.Flush();
                fs.Close();
            }
        }

        public static void GenerateFrom(string path)
        {
            var root = new DirectoryInfo(path);

            foreach (var dir in root.GetDirectories())
            {
                int i = 0;
                var filename = dir.Name + ".assets";
                var fs = File.Create(root.FullName + dir.Name + ".assets");
                fs.Position = 4;
                foreach (var file in dir.GetFiles("*.*", SearchOption.AllDirectories))
                {
                    if (file.Extension == ".assets")
                        continue;
                    var ts = file.OpenRead();
                    var rel = Path.GetRelativePath(path, file.FullName);
                    Console.WriteLine($"Packing {filename} <-- {rel}");
                    fs.WriteString(rel);
                    fs.WriteInt64(ts.Length);
                    ts.CopyTo(fs);
                    ts.Close();
                    i++;
                }
                dir.Delete(true);
                fs.Position = 0;
                fs.WriteInt(i);
                fs.Flush();
                fs.Close();
            }
        }
    }

    public enum AssetBundleCacheMode
    {
        OnDemand,
        OnLoad,
    }
}