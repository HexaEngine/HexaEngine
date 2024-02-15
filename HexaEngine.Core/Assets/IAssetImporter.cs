namespace HexaEngine.Core.Assets
{
    public interface IAssetImporter
    {
        bool CanImport(string fileExtension);

        void Import(TargetPlatform targetPlatform, ImportContext context);

        Task ImportAsync(TargetPlatform targetPlatform, ImportContext context)
        {
            return Task.Run(() => Import(targetPlatform, context));
        }
    }
}