namespace HexaEngine.IO
{
    using System.IO;
    using System.IO.Compression;
    using System.IO.Hashing;
    using System.Text;

    public class AssetBundle
    {
        private readonly Stream stream;

        public AssetBundle(string path)
        {
            var fs = Stream.Synchronized(File.OpenRead(path));
            stream = fs;
            var count = fs.ReadInt();
            var compression = fs.ReadInt();
            Assets = new Asset[count];
            for (int i = 0; i < count; i++)
            {
                var type = fs.ReadUInt64();
                var length = fs.ReadInt64();
                var decompressedLength = fs.ReadInt64();
                var apath = fs.ReadString();
                var pointer = fs.Position;
                if (compression == 0)
                    Assets[i] = new Asset((AssetType)type, pointer, length, apath, stream);
                else if (compression == 1)
                {
                    MemoryStream ms = new(new byte[decompressedLength]);
                    Decompress(new VirtualStream(stream, pointer, length, false), ms);
                    Assets[i] = new Asset((AssetType)type, 0, decompressedLength, apath, ms);
                }
                fs.Position = pointer + length;
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

        public static void CreateFrom(string path, bool compress = false, CompressionLevel level = CompressionLevel.Optimal)
        {
            var root = new DirectoryInfo(path);

            foreach (var dir in root.GetDirectories())
            {
                int i = 0;
                var fs = File.Create(dir.Name + ".assets");
                fs.Position = 8;
                foreach (var file in root.GetFiles("*.*", SearchOption.AllDirectories))
                {
                    if (file.Extension == ".assets")
                        continue;
                    var ts = file.OpenRead();

                    fs.WriteUInt64((ulong)AssetType.Binary);
                    var p = fs.Position;
                    fs.WriteInt64(ts.Length);
                    fs.WriteInt64(ts.Length);
                    fs.WriteString(Path.GetRelativePath(path, file.FullName));
                    var p1 = fs.Position;
                    if (compress)
                    {
                        var buffer = Compress(ts, level);
                        fs.Position = p;
                        fs.WriteInt64(buffer.Length);
                        fs.Position = p1;
                        fs.Write(buffer);
                    }
                    else
                    {
                        ts.CopyTo(fs);
                    }
                    i++;
                }
                fs.Position = 0;
                fs.WriteInt(i);
                fs.WriteInt(compress ? 1 : 0);
                fs.Flush();
                fs.Close();
            }
        }

        public static void GenerateFrom(string path, bool compress = false, CompressionLevel level = CompressionLevel.Optimal)
        {
            var root = new DirectoryInfo(path);
            foreach (var dir in root.GetDirectories())
            {
                int i = 0;
                var filename = dir.Name + ".assets";
                var fs = File.Create(root.FullName + dir.Name + ".assets");
                fs.Position = 8;
                foreach (var file in dir.GetFiles("*.*", SearchOption.AllDirectories))
                {
                    if (file.Extension == ".assets")
                        continue;
                    var ts = file.OpenRead();
                    var rel = Path.GetRelativePath(dir.FullName, file.FullName);
                    var type = GetAssetType(rel, out string relTrim);

                    Console.WriteLine($"Packing {filename} <-- [{type}] {relTrim}");

                    fs.WriteUInt64((ulong)type);
                    var p = fs.Position;
                    fs.WriteInt64(ts.Length);
                    fs.WriteInt64(ts.Length);
                    fs.WriteString(rel);
                    var p1 = fs.Position;
                    if (compress)
                    {
                        var buffer = Compress(ts, level);
                        fs.Position = p;
                        fs.WriteInt64(buffer.Length);
                        fs.Position = p1;
                        fs.Write(buffer);
                    }
                    else
                    {
                        ts.CopyTo(fs);
                    }
                    ts.Close();
                    i++;
                }
                dir.Delete(true);
                fs.Position = 0;
                fs.WriteInt(i);
                fs.WriteInt(compress ? 1 : 0);
                fs.Flush();
                fs.Close();
            }
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
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

        private static byte[] Compress(Stream input, CompressionLevel level)
        {
            using var compressStream = new MemoryStream();
            using var compressor = new DeflateStream(compressStream, level);
            input.CopyTo(compressor);
            compressor.Close();
            return compressStream.ToArray();
        }

        private static void Decompress(Stream input, Stream output)
        {
            using var decompressor = new DeflateStream(input, CompressionMode.Decompress);
            decompressor.CopyTo(output);
            decompressor.Close();
        }
    }
}