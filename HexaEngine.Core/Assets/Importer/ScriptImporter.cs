namespace HexaEngine.Core.Assets.Importer
{
    using HexaEngine.Core.Scripts;
    using System;
    using System.Linq;

    public class ScriptImporter : IAssetImporter
    {
        public Type? SettingsType { get; }

        public string? SettingsKey { get; }

        public string? SettingsDisplayName { get; }

        public bool CanImport(ReadOnlySpan<char> fileExtension)
        {
            return fileExtension.SequenceEqual(".cs");
        }

        public void Import(TargetPlatform targetPlatform, ImportContext context)
        {
            var path = context.SourcePath;
            string? name = CSharpScriptAnalyser.FindScript(path);
            if (name != null)
            {
                context.EmitArtifact(name, AssetType.Script, out string newPath);
                File.Delete(newPath);
                File.CreateSymbolicLink(newPath, path);
            }
        }
    }
}