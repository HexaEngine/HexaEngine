namespace HexaEngine.Core.Assets.Importer
{
    using HexaEngine.Core.Assets;

    public class DummyImporter : IAssetImporter
    {
        public Type? SettingsType { get; }
        public string? SettingsKey { get; }
        public string? SettingsDisplayName { get; }

        public bool CanImport(ReadOnlySpan<char> fileExtension)
        {
            return fileExtension switch
            {
                ".model" => true,
                ".matlib" => true,
                ".hexlvl" => true,
                _ => false,
            };
        }

        public void Import(TargetPlatform targetPlatform, ImportContext context)
        {
            var path = context.SourcePath;

            var name = Path.GetFileNameWithoutExtension(path);
            var extension = Path.GetExtension(path);

            AssetType type = extension switch
            {
                ".model" => AssetType.Model,
                ".matlib" => AssetType.MaterialLibrary,
                ".hexlvl" => AssetType.Scene,
                _ => AssetType.Unknown,
            };

            context.EmitArtifact(name, type, out string newPath);
            File.Delete(newPath);
            File.CreateSymbolicLink(newPath, path);
        }
    }
}