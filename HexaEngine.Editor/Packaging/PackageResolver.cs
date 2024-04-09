namespace HexaEngine.Editor.Packaging
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    public delegate bool ResolveConflictHandler(ResolvedDependency a, ResolvedDependency b, ConflictType conflict);

    public delegate bool ResolvePackageFailed(PackageIdentifier identifier, [MaybeNullWhen(false)] out IPackageSource? packageSource, [MaybeNullWhen(false)] out PackageMetadata? metadata);

    public class PackageResolver
    {
        private readonly IList<IPackageSource> packageSources;
        private readonly Dictionary<PackageMetadata, PackageDependencyTree> resolveCache = [];
        private readonly Dictionary<PackageIdentifier, (IPackageSource source, PackageMetadata metadata)> resolvePackageCache = [];

        public PackageResolver(IList<IPackageSource> packageSources)
        {
            this.packageSources = packageSources;
        }

        public event ResolveConflictHandler? ResolveConflict;

        public event ResolvePackageFailed? ResolvePackageFailed;

        public void ClearResolveCache()
        {
            resolveCache.Clear();
            resolvePackageCache.Clear();
        }

        public PackageDependencyTree? Resolve(PackageMetadata metadata, IPackageSource source)
        {
            if (resolveCache.TryGetValue(metadata, out var dependencyTree))
            {
                return dependencyTree;
            }

            List<ResolvedDependency> dependencies = new();
            Stack<(PackageDependency? parent, PackageDependency dependency)> dependenciesStack = new();
            for (int i = 0; i < metadata.Dependencies.Count; i++)
            {
                var dependency = metadata.Dependencies[i];
                dependenciesStack.Push((null, dependency));
            }

            dependencyTree = new(new(new PackageDependency(metadata.Id, metadata.Version), source, metadata));
            while (dependenciesStack.TryPop(out var dependency))
            {
                if (!Resolve(dependency.dependency, out var packageSource, out var packageMetadata))
                {
                    var result = ResolvePackageFailed?.Invoke(dependency.dependency, out packageSource, out packageMetadata) ?? false;
                    if (!result)
                    {
                        return null;
                    }
                }

                if (packageMetadata == null)
                {
                    throw new InvalidOperationException();
                }

                ResolvedDependency resolvedDependency = new(dependency.dependency, packageSource, packageMetadata);
                dependencyTree.Insert(dependency.parent, resolvedDependency);
                dependencies.Add(resolvedDependency);

                for (int i = 0; i < packageMetadata.Dependencies.Count; i++)
                {
                    dependenciesStack.Push((dependency.dependency, packageMetadata.Dependencies[i]));
                }
            }

            if (!CheckConflicts(dependencies))
            {
                return null;
            }

            resolveCache.Add(metadata, dependencyTree);

            return dependencyTree;
        }

        public bool Resolve(PackageIdentifier identifier, [MaybeNullWhen(false)] out IPackageSource? packageSource, [MaybeNullWhen(false)] out PackageMetadata? metadata)
        {
            if (resolvePackageCache.TryGetValue(identifier, out var value))
            {
                packageSource = value.source;
                metadata = value.metadata;
                return true;
            }

            for (int i = 0; i < packageSources.Count; i++)
            {
                var source = packageSources[i];
                var packageMetadata = source.GetPackageMetadata(identifier);
                if (packageMetadata == null)
                {
                    continue;
                }

                resolvePackageCache.Add(identifier, (source, packageMetadata));
                packageSource = source;
                metadata = packageMetadata;
                return true;
            }

            packageSource = null;
            metadata = null;
            return false;
        }

        private bool CheckConflicts(List<ResolvedDependency> dependencies)
        {
            for (int i = 0; i < dependencies.Count; i++)
            {
                bool unresolvedConflict = CheckInner(dependencies[i], dependencies);

                if (unresolvedConflict)
                {
                    return true;
                }
            }

            return false;
        }

        private bool CheckInner(ResolvedDependency dependency, List<ResolvedDependency> dependencies)
        {
            for (int i = 0; i < dependencies.Count; i++)
            {
                var other = dependencies[i];
                if (dependency == other)
                {
                    continue;
                }

                if (dependency.Metadata.Id != other.Metadata.Id)
                {
                    continue;
                }

                if (dependency.Metadata.Version != other.Metadata.Version)
                {
                    if (!(ResolveConflict?.Invoke(dependency, other, ConflictType.Version) ?? false))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}