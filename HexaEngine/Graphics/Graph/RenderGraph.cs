namespace HexaEngine.Graphics.Graph
{
    using HexaEngine.Core.Collections;

    public class RenderGraph
    {
        private readonly List<RenderGraphNode> nodes = [];
        private readonly List<RenderGraphNode> sortedNodes = [];
        private readonly GraphNodeRegistry registry = new();
        private readonly TopologicalSorter<RenderGraphNode> sorter = new();

        public RenderGraph(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public IReadOnlyList<RenderGraphNode> Nodes => nodes;

        public IReadOnlyList<RenderGraphNode> SortedNodes => sortedNodes;

        public RenderGraphNode Add<T>() where T : RenderPass, new()
        {
            T pass = new();
            return Add(pass);
        }

        public RenderGraphNode Add(RenderPass pass)
        {
            RenderGraphNode node = new(pass, registry);
            registry.Register(node);
            nodes.Add(node);
            return node;
        }

        public bool Remove(RenderPass pass)
        {
            if (registry.TryGetNode(pass, out var node))
            {
                registry.Unregister(node);
                nodes.Remove(node);
                return true;
            }
            return false;
        }

        public RenderGraphNode? GetNodeByName(string name)
        {
            registry.TryGetNode(name, out var node);
            return node;
        }

        public void Build()
        {
            foreach (var node in nodes)
            {
                node.BuildDependencies();
            }

            foreach (var node in nodes)
            {
                node.ResolveDependencies();
            }

            sortedNodes.Clear();
            sorter.TopologicalSort(nodes, sortedNodes);
        }
    }
}