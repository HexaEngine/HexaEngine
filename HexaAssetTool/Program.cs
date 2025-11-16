namespace AssetsBundler
{
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
            public Mode Mode { get; set; }

            public string Path { get; set; } = string.Empty;

            public string Output { get; set; } = string.Empty;

            public bool DeleteOriginal { get; set; } = false;

            public int Compress { get; set; } = 0;

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
            if (args.Length == 0)
            {
                PrintHelp();
                return;
            }

            var options = ParseArguments(args);
            if (options == null)
            {
                return;
            }

            switch (options.Mode)
            {
                case Mode.Create:
                    if (Directory.Exists(options.Path))
                    {
                        CreateArchive(options.Path, options.DeleteOriginal);
                    }
                    else
                    {
                        Console.WriteLine($"Error: Directory not found: {options.Path}");
                    }
                    break;

                case Mode.MultiCreate:
                    if (Directory.Exists(options.Path))
                    {
                        var dirs = Directory.GetDirectories(options.Path);
                        foreach (var dir in dirs)
                        {
                            CreateArchive(dir, options.DeleteOriginal);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Error: Directory not found: {options.Path}");
                    }
                    break;

                case Mode.List:
                    if (File.Exists(options.Path))
                    {
                        using AssetArchive bundle = new(options.Path, AssetArchiveMode.OpenRead);
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
                    else
                    {
                        Console.WriteLine($"Error: File not found: {options.Path}");
                    }
                    break;

                case Mode.Extract:
                    if (File.Exists(options.Path))
                    {
                        AssetArchive bundle = new(options.Path, AssetArchiveMode.OpenRead);
                        if (Directory.Exists("assets/"))
                        {
                            Directory.Delete("assets/", true);
                        }

                        Directory.CreateDirectory("assets/");
                        bundle.Extract("assets/");
                    }
                    else
                    {
                        Console.WriteLine($"Error: File not found: {options.Path}");
                    }
                    break;

                default:
                    throw new InvalidOperationException($"'{options.Mode}' is not supported.");
            }
        }

        private static Options? ParseArguments(string[] args)
        {
            Options options = new();
            bool modeSet = false;
            bool pathSet = false;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                switch (arg.ToLowerInvariant())
                {
                    case "-m":
                    case "--mode":
                        if (i + 1 >= args.Length)
                        {
                            Console.WriteLine("Error: --mode requires a value (create, multicreate, list, extract)");
                            PrintHelp();
                            return null;
                        }
                        if (Enum.TryParse<Mode>(args[++i], true, out var mode))
                        {
                            options.Mode = mode;
                            modeSet = true;
                        }
                        else
                        {
                            Console.WriteLine($"Error: Invalid mode '{args[i]}'. Valid values: create, multicreate, list, extract");
                            PrintHelp();
                            return null;
                        }
                        break;

                    case "-p":
                    case "--path":
                        if (i + 1 >= args.Length)
                        {
                            Console.WriteLine("Error: --path requires a value");
                            PrintHelp();
                            return null;
                        }
                        options.Path = args[++i];
                        pathSet = true;
                        break;

                    case "-o":
                    case "--output":
                        if (i + 1 >= args.Length)
                        {
                            Console.WriteLine("Error: --output requires a value");
                            PrintHelp();
                            return null;
                        }
                        options.Output = args[++i];
                        break;

                    case "-d":
                    case "--delete":
                        options.DeleteOriginal = true;
                        break;

                    case "-c":
                    case "--compression":
                        if (i + 1 >= args.Length)
                        {
                            Console.WriteLine("Error: --compression requires a value");
                            PrintHelp();
                            return null;
                        }
                        if (int.TryParse(args[++i], out var compress))
                        {
                            options.Compress = compress;
                        }
                        else
                        {
                            Console.WriteLine($"Error: Invalid compression value '{args[i]}'");
                            PrintHelp();
                            return null;
                        }
                        break;

                    case "-l":
                    case "--compression-level":
                        if (i + 1 >= args.Length)
                        {
                            Console.WriteLine("Error: --compression-level requires a value");
                            PrintHelp();
                            return null;
                        }
                        if (int.TryParse(args[++i], out var level))
                        {
                            options.CompressionLevel = level;
                        }
                        else
                        {
                            Console.WriteLine($"Error: Invalid compression level '{args[i]}'");
                            PrintHelp();
                            return null;
                        }
                        break;

                    case "-h":
                    case "--help":
                        PrintHelp();
                        return null;

                    default:
                        Console.WriteLine($"Error: Unknown argument '{arg}'");
                        PrintHelp();
                        return null;
                }
            }

            if (!modeSet)
            {
                Console.WriteLine("Error: --mode is required");
                PrintHelp();
                return null;
            }

            if (!pathSet)
            {
                Console.WriteLine("Error: --path is required");
                PrintHelp();
                return null;
            }

            return options;
        }

        private static void PrintHelp()
        {
            Console.WriteLine("AssetsBundler - Asset Archive Management Tool");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  AssetsBundler --mode <mode> --path <path> [options]");
            Console.WriteLine();
            Console.WriteLine("Required arguments:");
            Console.WriteLine("  -m, --mode <mode>              Mode: create, multicreate, list, extract");
            Console.WriteLine("  -p, --path <path>              Path to directory or file");
            Console.WriteLine();
            Console.WriteLine("Optional arguments:");
            Console.WriteLine("  -o, --output <path>            Output directory");
            Console.WriteLine("  -d, --delete                   Delete original files after packing");
            Console.WriteLine("  -c, --compression <level>      Compression (0 = none, 1 = deflate)");
            Console.WriteLine("  -l, --compression-level <num>  Compression level");
            Console.WriteLine("  -h, --help                     Show this help message");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  AssetsBundler --mode create --path ./myassets");
            Console.WriteLine("  AssetsBundler --mode list --path ./archive.assets");
            Console.WriteLine("  AssetsBundler --mode extract --path ./archive.assets");
        }

        private static AssetArchive CreateArchive(string path, bool deleteSource)
        {
            string name = Path.GetFileName(path);
            using AssetArchive bundle = new(Path.Combine(Path.GetDirectoryName(path)!, $"{name}.assets"), AssetArchiveMode.Create);
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