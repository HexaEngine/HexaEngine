﻿namespace HexaEngine.Core.Assets.Importer
{
    using System;
    using System.IO;

    public class TerrainImporter : IAssetImporter
    {
        public Type? SettingsType { get; }
        public string? SettingsKey { get; }

        public string? SettingsDisplayName { get; }

        public bool CanImport(ReadOnlySpan<char> fileExtension)
        {
            return fileExtension.SequenceEqual(".terrain");
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