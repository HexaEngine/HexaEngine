namespace HexaEngine.Editor.Plugins
{
    using System.IO.Compression;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class PluginMetadata
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Version { get; set; }

        public DateTime PublishDate { get; set; }

        public string Autors { get; set; }

        public string ProjectUrl { get; set; }

        public string RepositoryUrl { get; set; }

        public string RepositoryType { get; set; }

        public string Copyright { get; set; }

        public string LicenseUrl { get; set; }

        public string Tags { get; set; }
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(PluginPackageSpecs))]
    internal partial class PluginSpecsSourceGenerationContext : JsonSerializerContext
    {
    }

    public class PluginPackageSpecs
    {
        private const string CurrentSpecsVersion = "1.0.0";

        public string SpecsVersion { get; set; } = CurrentSpecsVersion;

        public PluginMetadata Metadata { get; set; }

        public List<string> Files { get; set; } = [];
    }

    public class PluginPackage
    {
        public static void ExtractPackage(string packagePath, string outputPath)
        {
            using var fs = File.OpenRead(packagePath);
            using ZipArchive archive = new(fs, ZipArchiveMode.Read);

            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                string name = entry.Name;
                var target = Path.Combine(outputPath, name);
                entry.ExtractToFile(target, true);
            }
        }

        public static void CreatePackage(PluginPackageSpecs specs, List<(string src, string packagePath)> files, string outputPath)
        {
            using var fs = File.Create(outputPath);
            using ZipArchive archive = new(fs, ZipArchiveMode.Create);

            foreach ((string src, string packagePath) in files)
            {
                specs.Files.Add(packagePath);
                archive.CreateEntryFromFile(src, packagePath);
            }

            var metaEntry = archive.CreateEntry("specs.json");
            using var mefs = metaEntry.Open();
            JsonSerializer.Serialize(mefs, specs, typeof(PluginPackageSpecs), PluginSpecsSourceGenerationContext.Default);
        }
    }
}