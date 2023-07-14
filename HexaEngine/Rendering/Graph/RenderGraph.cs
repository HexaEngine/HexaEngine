namespace HexaEngine.Rendering.Graph
{
    using HexaEngine.Core.Graphics;
    using System;

    public class RenderGraphExecuter
    {
        private IGraphicsDevice device;
        private ResourceCreator resourceCreator;
        private PipelineCreator pipelineCreator;
        private RenderGraph renderGraph;
        private RenderPass[] renderPasses;

        public RenderGraphExecuter(IGraphicsDevice device, RenderGraph renderGraph, RenderPass[] renderPasses)
        {
            this.device = device;
            this.renderGraph = renderGraph;
            this.renderPasses = renderPasses;
            resourceCreator = new ResourceCreator(device);
            pipelineCreator = new PipelineCreator(device);
        }

        public ResourceCreator ResourceCreator => resourceCreator;

        public void Init()
        {
            for (int i = 0; i < renderGraph.SortedNodeIndices.Count; i++)
            {
                renderPasses[renderGraph.SortedNodeIndices[i]].Init(resourceCreator, pipelineCreator, device);
            }
        }

        public void Execute(IGraphicsContext context)
        {
            for (int i = 0; i < renderGraph.SortedNodeIndices.Count; i++)
            {
                renderPasses[renderGraph.SortedNodeIndices[i]].Execute(context, resourceCreator);
            }
        }

        public void Release()
        {
            resourceCreator.ReleaseResources();
            pipelineCreator.ReleaseResources();
        }
    }

    public class RenderGraph
    {
        private readonly List<RenderGraphNode> nodes = new();
        private readonly List<ResourceBinding> globalResources = new();
        private readonly Dictionary<ResourceBinding, RenderGraphNode> globalResourcesLastWrite = new();
        private readonly List<RenderGraphNode> sortedNodes = new();
        private readonly List<int> sortedNodeIndices = new();

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

        private void Visit(RenderGraphNode node, List<RenderGraphNode> sorted, HashSet<RenderGraphNode> visited)
        {
            bool isVisited = visited.Contains(node);
            if (!isVisited)
            {
                visited.Add(node);
                for (int i = 0; i < node.Dependencies.Count; i++)
                {
                    var dependency = node.Dependencies[i];
                    Visit(dependency, sorted, visited);
                }
                sorted.Add(node);
            }
        }

        private void TopologicalSort()
        {
            sortedNodes.Clear();
            HashSet<RenderGraphNode> visited = new();

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                Visit(node, sortedNodes, visited);
            }

            for (int i = 0; i < sortedNodes.Count; i++)
            {
                var node = sortedNodes[i];
                var index = nodes.IndexOf(node);
                node.QueueIndex = index;
                sortedNodeIndices.Add(index);
            }
        }
    }
}