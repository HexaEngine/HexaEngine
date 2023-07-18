namespace HexaEngine.Rendering.Graph
{
    using HexaEngine.Collections;
    using System;

    public class RenderGraph
    {
        private readonly List<RenderGraphNode> nodes = new();
        private readonly List<ResourceBinding> globalResources = new();
        private readonly Dictionary<ResourceBinding, RenderGraphNode> globalResourcesLastWrite = new();
        private readonly List<RenderGraphNode> sortedNodes = new();
        private readonly List<int> sortedNodeIndices = new();

        private readonly TopologicalSorter<RenderGraphNode> sorter = new();

        public RenderGraph(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public IReadOnlyList<RenderGraphNode> Nodes => nodes;

        public IReadOnlyList<RenderGraphNode> SortedNodes => sortedNodes;

        public IReadOnlyList<int> SortedNodeIndices => sortedNodeIndices;

        public int AddRenderPass(RenderPassMetadata metadata)
        {
            int index = nodes.Count;
            RenderGraphNode node = new(metadata.Name);
            nodes.Add(node);
            return index;
        }

        public bool RemoveRenderPass(RenderPassMetadata metadata)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (node.Name == metadata.Name)
                {
                    nodes.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public void Build()
        {
            ResolveDependencies();
            TopologicalSort();
        }

        public void ResolveGlobalResources()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                node.Dependencies.Clear();
                for (int j = 0; j < node.Bindings.Count; j++)
                {
                    var binding = node.Bindings[j];
                    var name = binding.Name;
                    if (!name.StartsWith("#"))
                    {
                        continue;
                    }

                    if (!globalResources.Contains(binding))
                    {
                        globalResources.Add(binding);
                    }
                }
                for (int j = 0; j < node.Writes.Count; j++)
                {
                    var binding = node.Writes[j];
                    var name = binding.Name;
                    if (!name.StartsWith("#"))
                    {
                        continue;
                    }

                    if (!globalResources.Contains(binding))
                    {
                        globalResources.Add(binding);
                    }
                }
            }
        }

        private RenderGraphNode? ResolveDependency(ResourceBinding binding)
        {
            var name = binding.Name;
            if (name.StartsWith("#"))
            {
                for (int i = 0; i < globalResources.Count; i++)
                {
                    var resource = globalResources[i];
                    if (resource.Name == name)
                    {
                        if (globalResourcesLastWrite.TryGetValue(resource, out RenderGraphNode? node))
                        {
                            return node;
                        }
                        return null;
                    }
                }
                if (true)
                {
                    return null;
                }

                throw new Exception($"Cannot find global resource {name}");
            }
            else
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    var node = nodes[i];
                    for (int j = 0; j < node.Writes.Count; j++)
                    {
                        var write = node.Writes[j];
                        if (write.Name == name)
                        {
                            return node;
                        }
                    }
                }

                throw new Exception($"Cannot find resource {name}");
            }
        }

        private void ResolveDependencies()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                node.Dependencies.Clear();
                for (int j = 0; j < node.Bindings.Count; j++)
                {
                    var binding = node.Bindings[j];
                    var dependency = ResolveDependency(binding);
                    if (dependency != null)
                    {
                        node.Dependencies.Add(dependency);
                    }
                }
                for (int j = 0; j < node.Writes.Count; j++)
                {
                    var binding = node.Writes[j];
                    var name = binding.Name;
                    if (name.StartsWith("#"))
                    {
                        if (globalResourcesLastWrite.ContainsKey(binding))
                        {
                            globalResourcesLastWrite[binding] = node;
                        }
                        else
                        {
                            globalResourcesLastWrite.Add(binding, node);
                        }
                    }
                }
            }
        }

        private void TopologicalSort()
        {
            sortedNodes.Clear();

            var sorted = sorter.TopologicalSort(nodes);

            for (int i = 0; i < sorted.Count; i++)
            {
                var node = sorted[i];
                int index = nodes.IndexOf(node);
                sorted[i].QueueIndex = index;
                sortedNodes.Add(sorted[i]);
                sortedNodeIndices.Add(index);
            }

            sortedNodes.AddRange(sorted);
        }
    }
}