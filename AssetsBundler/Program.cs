namespace AssetsBundler
{
    using CommandLine;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.IO.Binary.Archives;
    using System;
    using System.IO;
    using System.IO.Hashing;
    using System.Text;

    internal class Program
    {
        private class Options
        {
            [Option('m', "mode", Required = true, HelpText = "Mode: create or list")]
            public Mode Mode { get; set; }

            [Option('p', "path", Required = true, HelpText = "path to dir")]
            public string Path { get; set; }

            [Option('o', "output", Required = false, HelpText = "out dir")]
            public string Output { get; set; }

            [Option('d', "delete", Required = false, HelpText = "delete original files after packing")]
            public bool DeleteOriginal { get; set; } = false;

            [Option('c', "Compression", Required = false, HelpText = "Compression 0 = none, 1 = deflate")]
            public int Compress { get; set; } = 0;

            [Option('l', "Compression level", Required = false, HelpText = "Compression level")]
            public int CompressionLevel { get; set; } = 0;
        }

        private enum Mode
        {
            Create,
            MultiCreate,
            List,
            Extract
        }

        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                switch (o.Mode)
                {
                    case Mode.Create:
                        if (Directory.Exists(o.Path))
                        {
                            CreateArchive(o.Path, o.DeleteOriginal);
                        }
                        break;

                    case Mode.MultiCreate:
                        if (Directory.Exists(o.Path))
                        {
                            var dirs = Directory.GetDirectories(o.Path);
                            foreach (var dir in dirs)
                            {
                                CreateArchive(dir, o.DeleteOriginal);
                            }
                        }
                        break;

                    case Mode.List:
                        if (File.Exists(o.Path))
                        {
                            using AssetArchive bundle = new(o.Path, AssetArchiveMode.OpenRead);
                            Crc32 crc = new();

                            Console.WriteLine($"Archive Namespace Entries: {bundle.Namespaces.Count}, BaseOffset: {bundle.BaseOffset}, Flags: {bundle.Flags}, CRC32: {bundle.CRC32:X8}");
                            foreach (var ns in bundle.Namespaces)
                            {
                                foreach (var pair in ns.Value.Assets)
                                {
                                    var asset = pair.Value;
                                    crc.Append(asset.GetData());
                                    Console.WriteLine($"[{asset.Type}] CRC32:{crc.GetCurrentHashAsUInt32():X8}, [{asset.Start}..{asset.Length}] {asset.Name}##{asset.Guid}, {asset.PathInArchive}");
                                }
                            }
                        }
                        break;

                    case Mode.Extract:
                        if (File.Exists(o.Path))
                        {
                            AssetArchive bundle = new(o.Path, AssetArchiveMode.OpenRead);
                            if (Directory.Exists("assets/"))
                            {
                                Directory.Delete("assets/", true);
                            }

                            Directory.CreateDirectory("assets/");
                            bundle.Extract("assets/");
                        }
                        break;

                    default:
                        throw new InvalidOperationException($"'{o.Mode}' is not supported.");
                }
            });
        }

        private static AssetArchive CreateArchive(string path, bool deleteSource)
        {
            string name = Path.GetFileName(path);
            using AssetArchive bundle = new(Path.Combine(Path.GetDirectoryName(path), $"{name}.assets"), AssetArchiveMode.Create);
            var ns = bundle.AddNamespace(name);
            foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(path, file);
                using var fs = File.OpenRead(file);
                bundle.AddEntry(ns, fs, relativePath, Path.GetFileName(file), AssetType.Unknown, Guid.Empty, Guid.Empty);
                fs.Dispose();
            }
            bundle.Dispose();
            if (deleteSource)
            {
                Directory.Delete(path, true);
            }

            return bundle;
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new(ba.Length * 2);
            foreach (byte b in ba)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }
    }
}