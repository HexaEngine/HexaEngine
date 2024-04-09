namespace HexaEngine.Core.Assets
{
    public interface IAssetImporter
    {
        Type? SettingsType { get; }

        string? SettingsKey { get; }

        string? SettingsDisplayName { get; }

        bool CanImport(ReadOnlySpan<char> fileExtension);

        void Import(TargetPlatform targetPlatform, ImportContext context);

        Task ImportAsync(TargetPlatform targetPlatform, ImportContext context)
        {
            return Task.Run(() => Import(targetPlatform, context));
        }

        public void GenerateThumbnail(ThumbnailCache cache, ImportContext context)
        {
        }
    }
}