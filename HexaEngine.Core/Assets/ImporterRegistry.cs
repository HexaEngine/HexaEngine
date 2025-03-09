namespace HexaEngine.Core.Assets
{
    using HexaEngine.Core.Assets.Importer;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public static class AssetImporterRegistry
    {
        private static readonly List<IAssetImporter> importers = [];
        private static readonly List<IAssetThumbnailProvider> thumbnailProviders = [];

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
            RegisterThumbnailProvider<TextureThumbnailProvider>();
        }

        public static void RegisterThumbnailProvider(IAssetThumbnailProvider provider)
        {
            thumbnailProviders.Add(provider);
        }

        public static void RegisterThumbnailProvider<T>() where T : IAssetThumbnailProvider, new()
        {
            RegisterThumbnailProvider(new T());
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

        public static T GetThumbnailProvider<T>() where T : IAssetThumbnailProvider
        {
            for (var i = 0; i < thumbnailProviders.Count; i++)
            {
                var provider = thumbnailProviders[i];
                if (provider is T t)
                {
                    return t;
                }
            }

            throw new KeyNotFoundException($"Importer ({nameof(T)}) not found");
        }

        public static IAssetThumbnailProvider GetThumbnailProviderForFile(ReadOnlySpan<char> fileExtension, SourceAssetMetadata metadata)
        {
            for (var i = 0; i < thumbnailProviders.Count; i++)
            {
                var provider = thumbnailProviders[i];
                if (provider.CanCreate(fileExtension, metadata))
                {
                    return provider;
                }
            }

            throw new KeyNotFoundException($"No importer for {fileExtension} found");
        }

        public static bool TryGetThumbnailProviderForFile(ReadOnlySpan<char> fileExtension, SourceAssetMetadata metadata, [NotNullWhen(true)] out IAssetThumbnailProvider? provider)
        {
            for (var i = 0; i < thumbnailProviders.Count; i++)
            {
                var iprovider = thumbnailProviders[i];
                if (iprovider.CanCreate(fileExtension, metadata))
                {
                    provider = iprovider;
                    return true;
                }
            }

            provider = null;
            return false;
        }
    }
}