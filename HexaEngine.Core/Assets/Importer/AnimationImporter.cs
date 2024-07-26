namespace HexaEngine.Core.Assets.Importer
{
    using HexaEngine.Core.IO.Binary.Animations;
    using HexaEngine.Core.Logging;
    using System;

    public class AnimationImporter : IAssetImporter
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(AnimationImporter));

        public Type? SettingsType { get; }

        public string? SettingsKey { get; }

        public string? SettingsDisplayName { get; }

        public bool CanImport(ReadOnlySpan<char> fileExtension)
        {
            return fileExtension.SequenceEqual(".animation");
        }

        public void Import(TargetPlatform targetPlatform, ImportContext context)
        {
            FileStream? inputStream = null;
            FileStream? outputStream = null;
            try
            {
                inputStream = File.OpenRead(context.SourcePath);
                AnimationFile animationFile = AnimationFile.Read(inputStream);

                context.EmitArtifact(animationFile.Name, AssetType.Animation, out outputStream);

                inputStream.Position = 0;
                inputStream.CopyTo(outputStream);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Logger.Error($"Failed to load animation, {context.SourcePath}");
            }
            finally
            {
                inputStream?.Dispose();
                outputStream?.Dispose();
            }
        }
    }
}