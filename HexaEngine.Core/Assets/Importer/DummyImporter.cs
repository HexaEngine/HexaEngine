namespace HexaEngine.Core.Assets.Importer
{
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Debugging;

    public enum DummyImportMethod
    {
        SymLink,
        HardLink,
        Copy,
    }

    public class DummyImporter : IAssetImporter
    {
        private static DummyImportMethod importMethod = DummyImportMethod.SymLink;
        private static readonly ILogger Logger = LoggerFactory.GetLogger("DummyImporter");

        public static DummyImportMethod ImportMethod { get => importMethod; set => importMethod = value; }

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
                ".prefab" => true,
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
                ".prefab" => AssetType.Prefab,
                _ => AssetType.Unknown,
            };

            context.EmitArtifact(name, type, out string newPath);

            try
            {
                if (File.Exists(newPath))
                {
                    File.Delete(newPath);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return;
            }

            switch (importMethod)
            {
                case DummyImportMethod.SymLink:
                    try
                    {
                        File.CreateSymbolicLink(newPath, path);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                        Logger.Info("Trying using fallback method: Copy");
                        importMethod = DummyImportMethod.Copy;
                        goto case DummyImportMethod.Copy;
                    }
                    break;

                case DummyImportMethod.Copy:
                    try
                    {
                        File.Copy(newPath, path);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                    }
                    break;

                default:
                    throw new NotSupportedException();
            }
        }
    }
}