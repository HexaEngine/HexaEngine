namespace HexaEngine.Core.Assets.Importer
{
    using HexaEngine.Core.Assets;

    public class DummyImporter : IAssetImporter
    {
        public bool CanImport(string fileExtension)
        {
            return fileExtension switch
            {
                ".model" => true,
                ".material" => true,
                ".matlib" => true,
                ".hexlvl" => true,
                ".cs" => true,
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
                ".material" => AssetType.Material,
                ".matlib" => AssetType.MaterialLibrary,
                ".hexlvl" => AssetType.Scene,
                ".cs" => AssetType.Script,
                _ => AssetType.Unknown,
            };

            context.EmitArtifact(name, type, out string newPath);
            File.Delete(newPath);
            File.CreateSymbolicLink(newPath, path);
        }
    }
}