namespace HexaEngine.Editor.Packaging
{
    using HexaEngine.Core.Logging;
    using System.Collections.Generic;

    public class PackageManager
    {
        internal static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(PackageManager));
        private readonly List<IPackageSource> packageSources;
        private readonly PackageResolver resolver;
        private readonly PackageCache cache;

        public PackageManager(IEnumerable<IPackageSource> packageSources, PackageCache cache)
        {
            this.packageSources = new(packageSources);
            resolver = new(this.packageSources);
            this.cache = cache;
        }

        public void AddPackageSource(IPackageSource source)
        {
            packageSources.Add(source);
            resolver.ClearResolveCache();
        }

        public void RemovePackageSource(IPackageSource source)
        {
            packageSources.Remove(source);
            resolver.ClearResolveCache();
        }

        public void Refresh(bool clearPackageCache = false)
        {
            resolver.ClearResolveCache();
            if (clearPackageCache)
            {
                cache.Clear();
            }
        }

        public PackageFileTree GetPackageFileTree(PackageIdentifier identifier, out string locationOnDisk)
        {
            if (cache.TryGet(identifier, out string location, out PackageFileTree fileTree))
            {
                locationOnDisk = location;
                return fileTree;
            }
            throw new NotImplementedException();
        }

        public void DownloadPackage(PackageIdentifier identifier)
        {
            if (!resolver.Resolve(identifier, out var packageSource, out var metadata))
            {
                throw new InvalidOperationException($"Couldn't find package '{identifier}'.");
            }

            string location = cache.Set(identifier, metadata, packageSource);
        }
    }
}