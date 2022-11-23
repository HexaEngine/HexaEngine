namespace HexaEngine.IO
{
    using System.IO;

    public class AssetBundle
    {
        private readonly Stream stream;

        public AssetBundle(string path)
        {
            var fs = Stream.Synchronized(File.OpenRead(path));
            stream = fs;
            var count = fs.ReadInt();
            Assets = new Asset[count];
            for (int i = 0; i < count; i++)
            {
                var type = fs.ReadUInt64();
                var length = fs.ReadInt64();
                var apath = fs.ReadString();
                var pointer = fs.Position;
                fs.Position += length;
                Assets[i] = new Asset((AssetType)type, pointer, length, apath, stream);
            }
        }

        public Asset[] Assets { get; }

        public Stream GetStream() => stream;

        public void Extract(string path)
        {
            var root = new DirectoryInfo(path);
            foreach (Asset asset in Assets)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(root.FullName + asset.Path));
                var fs = File.Create(root.FullName + asset.Path);
                fs.Write(asset.GetData());
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
                    fs.WriteUInt64((ulong)AssetType.Binary);
                    fs.WriteInt64(ts.Length);
                    fs.WriteString(Path.GetRelativePath(path, file.FullName));
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
                    var rel = Path.GetRelativePath(dir.FullName, file.FullName);
                    var type = GetAssetType(rel, out string relTrim);
                    Console.WriteLine($"Packing {filename} <-- [{type}] {relTrim}");
                    fs.WriteUInt64((ulong)type);
                    fs.WriteInt64(ts.Length);
                    fs.WriteString(rel);
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

        public static AssetType GetAssetType(string path, out string trimmed)
        {
            if (path.Contains("textures"))
            {
                trimmed = path.Replace("textures/", string.Empty).Replace("textures\\", string.Empty);
                return AssetType.Texture;
            }

            if (path.Contains("meshes"))
            {
                trimmed = path.Replace("meshes/", string.Empty).Replace("meshes\\", string.Empty);
                return AssetType.Mesh;
            }

            if (path.Contains("shaders"))
            {
                trimmed = path.Replace("shaders/", string.Empty).Replace("shaders\\", string.Empty);
                return AssetType.Shader;
            }

            if (path.Contains("fonts"))
            {
                trimmed = path.Replace("fonts/", string.Empty).Replace("fonts\\", string.Empty);
                return AssetType.Font;
            }

            if (path.Contains("sounds"))
            {
                trimmed = path.Replace("sounds/", string.Empty).Replace("sounds\\", string.Empty);
                return AssetType.Sound;
            }

            if (path.Contains("scripts"))
            {
                trimmed = path.Replace("scripts/", string.Empty).Replace("scripts\\", string.Empty);
                return AssetType.Script;
            }

            trimmed = path;
            return AssetType.Binary;
        }
    }
}