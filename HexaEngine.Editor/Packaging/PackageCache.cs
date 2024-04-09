namespace HexaEngine.Editor.Packaging
{
    using System;

    public class PackageCache
    {
        private readonly string path;

        public PackageCache(string path)
        {
            this.path = path;
        }

        public void Clear()
        {
        }

        internal bool TryGet(PackageIdentifier identifier, out string location, out PackageFileTree fileTree)
        {
            throw new NotImplementedException();
        }

        public string Set(PackageIdentifier identifier, PackageMetadata metadata, IPackageSource packageSource)
        {
            string location = Path.Combine(path, identifier.Id, identifier.Version.ToString());

            string packageFile = Path.Combine(location, $"{identifier.Id}.hexpkg");

            using (FileStream fs = File.Create(packageFile))
            {
                packageSource.DownloadPackage(metadata, fs);
            }

            Package.Extract(packageFile, location);

            return location;
        }
    }
}