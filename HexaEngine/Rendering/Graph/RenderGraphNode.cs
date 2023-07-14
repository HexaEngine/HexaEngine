namespace HexaEngine.Rendering.Graph
{
    public class RenderGraphNode
    {
        public RenderGraphNode(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public List<RenderGraphNode> Dependencies { get; } = new();

        public List<ResourceBinding> Bindings { get; } = new();

        public List<ResourceBinding> Writes { get; } = new();

        public int QueueIndex { get; internal set; }

        public override string ToString()
        {
            return $"{Name}:{QueueIndex}";
        }
    }
}