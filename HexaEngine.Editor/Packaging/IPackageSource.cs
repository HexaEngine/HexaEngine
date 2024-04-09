namespace HexaEngine.Editor.Packaging
{
    using System.Collections.Generic;

    public interface IPackageSource
    {
        PackageMetadata? GetPackageMetadata(PackageIdentifier identifier);

        IEnumerable<string> GetPackages();

        bool DownloadPackage(PackageMetadata metadata, Stream stream);
    }
}