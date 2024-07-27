namespace HexaEngine.Editor.Packaging
{
    using System.Collections.Generic;
    using System.Linq;

    public class LocalPackageSource : IPackageSource
    {
        private readonly Dictionary<PackageIdentifier, PackageMetadata?> _metadataCache = [];
        private readonly List<string> _packages = [];
        private readonly string folder;

        public LocalPackageSource(string folder)
        {
            this.folder = folder;
            _packages.AddRange(Directory.GetFiles(folder, "*.hexpkg").Select(x => Path.GetFileNameWithoutExtension(x)));
        }

        public bool DownloadPackage(PackageMetadata metadata, Stream stream)
        {
            var fullPath = Path.Combine(folder, $"{metadata.Name}.hexpkg");
            if (!File.Exists(fullPath))
            {
                return false;
            }

            FileStream? fs = null;
            try
            {
                fs = File.OpenRead(fullPath);
                fs.CopyTo(stream);
                fs.Close();
            }
            catch (Exception ex)
            {
                PackageManager.Logger.Error($"Failed to read package ({fullPath})");
                PackageManager.Logger.Log(ex);
                return false;
            }
            finally
            {
                fs?.Dispose();
            }

            return true;
        }

        public PackageMetadata? GetPackageMetadata(PackageIdentifier identifier)
        {
            if (_metadataCache.TryGetValue(identifier, out var metadata))
            {
                return metadata;
            }

            var fullPath = Path.Combine(folder, $"{identifier.Id}.hexpkg");
            if (!File.Exists(fullPath))
            {
                _metadataCache.Add(identifier, null);
                return null;
            }

            metadata = Package.ReadMetadata(fullPath);
            _metadataCache[identifier] = metadata;
            return metadata;
        }

        public IEnumerable<string> GetPackages()
        {
            return _packages;
        }
    }
}