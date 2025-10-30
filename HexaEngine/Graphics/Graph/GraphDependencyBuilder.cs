
namespace HexaEngine.Graphics.Graph
{
    public enum GraphDependencyType
    {
        Default,
        Required,
        AfterAll,
        AllNotReferenced
    }

    public struct GraphDependency : IEquatable<GraphDependency>
    {
        public string Name;
        public GraphDependencyType Type;

        public GraphDependency(string name, GraphDependencyType type)
        {
            Name = name;
            Type = type;
        }

        public readonly override bool Equals(object? obj)
        {
            return obj is GraphDependency dependency && Equals(dependency);
        }

        public readonly bool Equals(GraphDependency other)
        {
            return Name == other.Name;
        }

        public readonly override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static bool operator ==(GraphDependency left, GraphDependency right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GraphDependency left, GraphDependency right)
        {
            return !(left == right);
        }
    }

    public class GraphDependencyBuilder
    {
        private readonly RenderGraphNode node;
        private readonly GraphNodeRegistry registry;

        private readonly HashSet<GraphDependency> after = [];
        private readonly HashSet<GraphDependency> before = [];

        public GraphDependencyBuilder(RenderGraphNode node, GraphNodeRegistry registry)
        {
            this.node = node;
            this.registry = registry;
        }

        public RenderGraphNode Node { get => node; }

        public void Clear()
        {
            after.Clear();
            before.Clear();
        }

        public GraphDependencyBuilder AddDependency(string name, GraphDependencyType type)
        {
            after.Add(new GraphDependency(name, GraphDependencyType.Default));
            return this;
        }

        public GraphDependencyBuilder AddDependent(string name, GraphDependencyType type)
        {
            before.Add(new GraphDependency(name, GraphDependencyType.Default));
            return this;
        }

        public GraphDependencyBuilder RunBefore(string name)
        {
            return AddDependent(name, GraphDependencyType.Default);
           
        }

        public GraphDependencyBuilder RunAfter(string name)
        {
            return AddDependency(name, GraphDependencyType.Default);
        }

        public GraphDependencyBuilder RunBefore<T>() where T : RenderPass
        {
            if (registry.TryGetNodeName<T>(out var name))
            {
                AddDependent(name, GraphDependencyType.Default);
            }
            return this;
        }

        public GraphDependencyBuilder RunAfter<T>() where T : RenderPass
        {
            if (registry.TryGetNodeName<T>(out var name))
            {
                AddDependency(name, GraphDependencyType.Default);
            }
            return this;
        }
        
        private RenderGraphNode? ResolveNode(string name, bool optional)
        {
            if (!registry.TryGetNode(name, out var node))
            {
                if (optional) return null;
                throw new KeyNotFoundException($"Failed to resolve node '{name}'.");
            }
            if (!node.Pass.Enabled && !optional)
            {
                if (node.Pass.Type == RenderPassType.Optional)
                {
                    node.Pass.Enabled = true;
                    return node;
                }
                throw new InvalidOperationException($"The pass '{node.Name}' has an active relationship with '{node.Name}', but '{node.Name}' was not enabled.");
            }
            return node;
        }

        public bool HasDependencyOn(string name)
        {
            if (after.Contains(new GraphDependency(name, GraphDependencyType.Default)))
            {
                return true;
            }

            return false;
        }

        public bool IsReferencedBy(RenderGraphNode other)
        {
            if (other.Builder.HasDependencyOn(node.Name))
            {
                return true;
            }

            return false;
        }

        public void Build()
        {
            foreach (var runAfter in after)
            {
                switch (runAfter.Type)
                {
                    case GraphDependencyType.Default:
                        var afterNode = ResolveNode(runAfter.Name, true);
                        if (afterNode != null)
                        {
                            node.AddDependency(afterNode);
                        }
                        break;

                    case GraphDependencyType.Required:
                        node.AddDependency(ResolveNode(runAfter.Name, false)!);
                        break;

                    case GraphDependencyType.AfterAll:
                        foreach (var other in registry.Nodes)
                        {
                            if (node != other)
                            {
                                node.AddDependency(other);
                            }
                        }
                        break;

                    case GraphDependencyType.AllNotReferenced:
                        foreach (var other in registry.Nodes)
                        {
                            if (node != other && !IsReferencedBy(other))
                            {
                                node.AddDependency(other);
                            }
                        }
                        break;
                }
            }

            foreach (var before in before)
            {
                switch (before.Type)
                {
                    case GraphDependencyType.Default:
                        var beforeNode = ResolveNode(before.Name, true);
                        beforeNode?.AddDependency(node);
                        break;

                    case GraphDependencyType.Required:
                        node.AddDependent(ResolveNode(before.Name, false)!);
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
        }
    }
}