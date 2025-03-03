namespace HexaEngine.Core.Assets.Importer
{
    using Hexa.NET.Logging;
    using HexaEngine.Core.IO.Binary.Materials;
    using System;

    public class MaterialImporter : IAssetImporter
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(MaterialImporter));

        public Type? SettingsType { get; }

        public string? SettingsKey { get; }

        public string? SettingsDisplayName { get; }

        public bool CanImport(ReadOnlySpan<char> fileExtension)
        {
            return fileExtension.SequenceEqual(".material");
        }

        public void Import(TargetPlatform targetPlatform, ImportContext context)
        {
            FileStream? inputStream = null;
            FileStream? outputStream = null;
            try
            {
                inputStream = File.OpenRead(context.SourcePath);
                MaterialFile materialFile = MaterialFile.ReadFrom(inputStream);

                context.EmitArtifact(materialFile.Name, AssetType.Material, out outputStream);

                inputStream.Position = 0;
                inputStream.CopyTo(outputStream);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Logger.Error($"Failed to load material, {context.SourcePath}");
            }
            finally
            {
                inputStream?.Dispose();
                outputStream?.Dispose();
            }
        }
    }
}