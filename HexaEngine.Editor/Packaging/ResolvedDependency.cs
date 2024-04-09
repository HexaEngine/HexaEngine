namespace HexaEngine.Editor.Packaging
{
    public class ResolvedDependency
    {
        public PackageDependency Dependency;
        public IPackageSource PackageSource;
        public PackageMetadata Metadata;

        public ResolvedDependency(PackageDependency dependency, IPackageSource packageSource, PackageMetadata metadata)
        {
            Dependency = dependency;
            PackageSource = packageSource;
            Metadata = metadata;
        }
    }
}