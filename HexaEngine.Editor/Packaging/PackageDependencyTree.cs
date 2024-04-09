namespace HexaEngine.Editor.Packaging
{
    using System;
    using System.Collections;

    public class PackageDependencyTree : IEnumerable<PackageDependencyTreeNode>
    {
        private readonly PackageDependencyTreeNode root;

        public PackageDependencyTree(ResolvedDependency root)
        {
            this.root = new PackageDependencyTreeNode(null, root);
        }

        public void Insert(PackageDependency? parent, ResolvedDependency resolved)
        {
            if (parent == null)
            {
                var parentNode = root;

                // Create a new node for the resolved dependency
                var newNode = new PackageDependencyTreeNode(parentNode, resolved);

                // Add the new node as a child of the parent node
                parentNode.Children.Add(newNode);
            }
            else
            {
                // Find the node corresponding to the parent package in the tree
                var parentNode = FindNode(root, parent) ?? throw new ArgumentException("Parent package not found in the tree.");

                // Create a new node for the resolved dependency
                var newNode = new PackageDependencyTreeNode(parentNode, resolved);

                // Add the new node as a child of the parent node
                parentNode.Children.Add(newNode);
            }
        }

        private static PackageDependencyTreeNode? FindNode(PackageDependencyTreeNode node, PackageDependency dependency)
        {
            if (node.Resolved.Dependency.Equals(dependency))
            {
                return node;
            }

            foreach (var child in node.Children)
            {
                var foundNode = FindNode(child, dependency);
                if (foundNode != null)
                {
                    return foundNode;
                }
            }

            return null;
        }

        public List<ResolvedDependency> ToList(bool removeDuplicates)
        {
            var list = new List<ResolvedDependency>();
            AddToList(root, list);

            if (removeDuplicates)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    for (int j = i + 1; j < list.Count; j++)
                    {
                        var other = list[j];
                        if (item.Dependency.Id == other.Dependency.Id)
                        {
                            list.RemoveAt(j);
                            j--;
                        }
                    }
                }
            }

            return list;
        }

        private static void AddToList(PackageDependencyTreeNode node, List<ResolvedDependency> list)
        {
            foreach (var child in node.Children)
            {
                AddToList(child, list);
            }

            list.Add(node.Resolved);
        }

        private struct Enumerator : IEnumerator<PackageDependencyTreeNode>
        {
            private readonly PackageDependencyTreeNode root;
            private PackageDependencyTreeNode? current;
            private int index;

            public Enumerator(PackageDependencyTreeNode root)
            {
                this.root = root;
                current = null;
                index = -1;
            }

            public readonly PackageDependencyTreeNode Current => current;

            readonly object IEnumerator.Current => current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (index == -1)
                {
                    current = root;
                    index = 0;
                    return true;
                }

                if (current == null)
                {
                    return false;
                }

                if (index < current.Children.Count)
                {
                    current = current.Children[index];
                    index = 0;
                    return true;
                }

                while (current != null)
                {
                    if (current.Parent == null)
                    {
                        current = null;
                        return false;
                    }

                    var parent = current.Parent;
                    index = parent.Children.IndexOf(current) + 1;
                    if (index < parent.Children.Count)
                    {
                        current = parent.Children[index];
                        index = 0;
                        return true;
                    }

                    current = parent;
                }

                return false;
            }

            public void Reset()
            {
                current = null;
                index = -1;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return new Enumerator(root);
        }

        IEnumerator<PackageDependencyTreeNode> IEnumerable<PackageDependencyTreeNode>.GetEnumerator()
        {
            return new Enumerator(root);
        }
    }
}