using System.Diagnostics.CodeAnalysis;

namespace HexaEngine.Graphics.Graph
{
    public class GraphNodeRegistry
    {
        private readonly Dictionary<string, RenderGraphNode> nameToNode = [];
        private readonly Dictionary<Type, string> typeToName = [];
        private readonly Dictionary<RenderPass, RenderGraphNode> passToNode = [];

        public IEnumerable<RenderGraphNode> Nodes => nameToNode.Values;

        public RenderGraphNode GetNode(RenderPass pass)
        {
            return passToNode[pass];
        }

        public bool TryGetNode(RenderPass pass, [NotNullWhen(true)] out RenderGraphNode? node)
        {
            return passToNode.TryGetValue(pass, out node);
        }

        public bool TryGetNode(string name, [NotNullWhen(true)] out RenderGraphNode? node)
        {
            return nameToNode.TryGetValue(name, out node);
        }

        public string GetNodeName<T>() where T : RenderPass
        {
            return typeToName[typeof(T)];
        }

        public bool TryGetNodeName<T>([NotNullWhen(true)] out string? name) where T : RenderPass
        {
            return typeToName.TryGetValue(typeof(T), out name);
        }

        public RenderGraphNode GetNode(string name)
        {
            return nameToNode[name];
        }

        public void Register(RenderGraphNode node)
        {
            nameToNode.Add(node.Name, node);
            passToNode.Add(node.Pass, node);
            typeToName.Add(node.Type, node.Name);
        }

        public void Unregister(RenderGraphNode node)
        {
            nameToNode.Remove(node.Name);
            passToNode.Remove(node.Pass);
            typeToName.Remove(node.Type);
        }

        public void Clear()
        {
            nameToNode.Clear();
        }
    }
}