using HexaEngine.Core.Assets.Importer;

namespace HexaEngine.Core.Assets
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public static class AssetImporterRegistry
    {
        private static readonly List<IAssetImporter> importers = [];

        static AssetImporterRegistry()
        {
            RegisterImporter<ScriptImporter>();
            RegisterImporter<TextureImporter>();
            //RegisterImporter<GLTFImporter>();
            RegisterImporter<ModelImporter>();
            RegisterImporter<AnimationImporter>();
            RegisterImporter<MaterialImporter>();
            RegisterImporter<DummyImporter>();
            RegisterImporter<TerrainImporter>();
        }

        public static void RegisterImporter(IAssetImporter importer)
        {
            importers.Add(importer);
        }

        public static void RegisterImporter<T>() where T : IAssetImporter, new()
        {
            RegisterImporter(new T());
        }

        public static T GetImporter<T>() where T : IAssetImporter
        {
            for (var i = 0; i < importers.Count; i++)
            {
                var importer = importers[i];
                if (importer is T t)
                {
                    return t;
                }
            }

            throw new KeyNotFoundException($"Importer ({nameof(T)}) not found");
        }

        public static IAssetImporter GetImporterForFile(ReadOnlySpan<char> fileExtension)
        {
            for (var i = 0; i < importers.Count; i++)
            {
                var importer = importers[i];
                if (importer.CanImport(fileExtension))
                {
                    return importer;
                }
            }

            throw new KeyNotFoundException($"No importer for {fileExtension} found");
        }

        public static bool TryGetImporterForFile(ReadOnlySpan<char> fileExtension, [NotNullWhen(true)] out IAssetImporter? importer)
        {
            for (var i = 0; i < importers.Count; i++)
            {
                var iImporter = importers[i];
                if (iImporter.CanImport(fileExtension))
                {
                    importer = iImporter;
                    return true;
                }
            }

            importer = null;
            return false;
        }
    }
}