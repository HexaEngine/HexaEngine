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
            Import(targetPlatform, context);
            return Task.CompletedTask;
        }

        public void GenerateThumbnail(ThumbnailCache cache, ImportContext context)
        {
        }

        /// <summary>
        /// Checks the content of a file, used for updating file formats.
        /// </summary>
        /// <param name="artifact">The path to the asset.</param>
        /// <returns><c>true</c> if content needs to be reimported/updated, otherwise <c>false</c>.</returns>
        bool RefreshContent(Artifact artifact)
        {
            return false;
        }
    }
}