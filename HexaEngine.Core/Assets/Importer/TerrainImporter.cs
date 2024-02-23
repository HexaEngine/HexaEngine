namespace HexaEngine.Core.Assets.Importer
{
    using System.IO;

    public class TerrainImporter : IAssetImporter
    {
        public bool CanImport(string fileExtension)
        {
            return fileExtension == ".terrain";
        }

        public void Import(TargetPlatform targetPlatform, ImportContext context)
        {
            var path = context.SourcePath;

            var name = Path.GetFileNameWithoutExtension(path);

            context.EmitArtifact(name, AssetType.Terrain, out string newPath);
            File.Delete(newPath);
            File.CreateSymbolicLink(newPath, path);
        }
    }
}