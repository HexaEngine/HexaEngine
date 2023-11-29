namespace HexaEngine.Graphics.Graph
{
    using HexaEngine.Collections;

    public class RenderGraphNode : INode
    {
        public RenderGraphNode(string name)
        {
            Name = name;
            Builder = new(this);
        }

        public string Name { get; }

        public List<RenderGraphNode> Dependencies { get; } = new();

        public List<RenderGraphNode> Dependents { get; } = new();

        public List<ResourceBinding> Bindings { get; } = new();

        public List<ResourceBinding> Writes { get; } = new();

        public GraphDependencyBuilder Builder { get; }

        public int QueueIndex { get; internal set; }

        List<INode> INode.Dependencies => Dependencies.Cast<INode>().ToList();

        public override string ToString()
        {
            return $"{Name}:{QueueIndex}";
        }
    }
}