namespace HexaEngine.Editor.Packaging
{
    public class PackageDependencyTreeNode
    {
        public PackageDependencyTreeNode? Parent;
        public List<PackageDependencyTreeNode> Children = [];
        public ResolvedDependency Resolved;

        public PackageDependencyTreeNode(PackageDependencyTreeNode? parent, ResolvedDependency resolved)
        {
            Parent = parent;
            Resolved = resolved;
        }
    }
}