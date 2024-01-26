namespace HexaEngine.Graphics.Graph
{
    public class GraphDependencyBuilder
    {
        private readonly RenderGraphNode node;
        private readonly List<ResourceBinding> bindings = new();
        private readonly List<ResourceTarget> sources = new();

        private readonly List<string> after = new();
        private readonly List<string> before = new();

        public GraphDependencyBuilder(RenderGraphNode node)
        {
            this.node = node;
        }

        public void Build(RenderGraph graph)
        {
        }

        public void Clear()
        {
            bindings.Clear();
            sources.Clear();
            after.Clear();
            before.Clear();
        }

        public void AddWrite(ResourceTarget target)
        {
            sources.Add(target);
        }

        public void AddWrite(string target)
        {
            AddWrite(new ResourceTarget(target));
        }

        public void AddRead(ResourceBinding source)
        {
            bindings.Add(source);
        }

        public void AddRead(string source)
        {
            AddRead(new ResourceBinding(source));
        }

        public void RunBefore(string name)
        {
            before.Add(name);
        }

        public void RunAfter(string name)
        {
            after.Add(name);
        }

        private static RenderGraphNode? ResolveDependency(ResourceBinding binding, IReadOnlyList<RenderGraphNode> others, IReadOnlyList<ResourceBinding> globalResources)
        {
            var name = binding.Name;
            if (name.StartsWith('#'))
            {
                throw new InvalidOperationException($"Couldn't find render graph global resource: {name}");
            }
            else
            {
                for (int i = 0; i < others.Count; i++)
                {
                    var node = others[i];
                    var builder = node.Builder;
                    for (int j = 0; j < builder.sources.Count; j++)
                    {
                        var source = builder.sources[j];
                        if (source.Name == name)
                        {
                            return node;
                        }
                    }
                }
                for (int i = 0; i < globalResources.Count; i++)
                {
                    var global = globalResources[i];
                    if (global.Name == name)
                    {
                        return null;
                    }
                }

                throw new InvalidOperationException($"Couldn't find local or sub graph local resource: {name}");
            }
        }

        private static RenderGraphNode? ResolveNode(string name, IReadOnlyList<RenderGraphNode> others)
        {
            for (int i = 0; i < others.Count; i++)
            {
                var other = others[i];
                if (other.Name == name)
                {
                    return other;
                }
            }
            return null;
        }

        public bool HasDependencyOn(string name)
        {
            if (after.Contains(name))
            {
                return true;
            }

            for (int i = 0; i < bindings.Count; i++)
            {
                if (bindings[i].Name == name)
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

            for (int i = 0; i < sources.Count; i++)
            {
                if (other.Builder.HasDependencyOn(sources[i].Name))
                {
                    return true;
                }
            }

            return false;
        }

        public void Build(IReadOnlyList<RenderGraphNode> others, IReadOnlyList<ResourceBinding> globalResources)
        {
            for (int i = 0; i < bindings.Count; i++)
            {
                var dep = ResolveDependency(bindings[i], others, globalResources);
                if (dep is null)
                {
                    throw new Exception("Could not resolve dependencyaaaaaaaaaaaaaa");
                }
                else
                {
                    if (!node.Dependencies.Contains(dep))
                    {
                        node.Dependencies.Add(dep);
                        dep.Dependents.Add(node);
                    }
                }
            }

            for (int i = 0; i < after.Count; i++)
            {
                var runAfter = after[i];
                var afterNode = ResolveNode(runAfter, others);
                if (afterNode is null)
                {
                    if (runAfter == "!All")
                    {
                        for (int j = 0; j < others.Count; j++)
                        {
                            var other = others[j];
                            if (node != other)
                            {
                                if (!node.Dependencies.Contains(other))
                                    node.Dependencies.Add(other);
                            }
                        }
                    }
                    if (runAfter == "!AllNotReferenced")
                    {
                        for (int j = 0; j < others.Count; j++)
                        {
                            var other = others[j];
                            if (node != other && !IsReferencedBy(other))
                            {
                                if (!node.Dependencies.Contains(other))
                                    node.Dependencies.Add(other);
                            }
                        }
                    }
                    continue;
                }
                if (!node.Dependencies.Contains(afterNode))
                    node.Dependencies.Add(afterNode);
            }

            for (int i = 0; i < before.Count; i++)
            {
                var beforeNode = ResolveNode(before[i], others);
                if (beforeNode is null)
                {
                    continue;
                }
                if (!beforeNode.Dependencies.Contains(node))
                    beforeNode.Dependencies.Add(node);
            }
        }
    }
}