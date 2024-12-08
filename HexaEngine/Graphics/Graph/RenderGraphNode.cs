namespace HexaEngine.Graphics.Graph
{
    using HexaEngine.Core.Collections;

    public class RenderGraphNode : INode<RenderGraphNode>
    {
        public RenderGraphNode(string name)
        {
            Name = name;
            Builder = new(this);
            Container = new();
        }

        public string Name { get; }

        public List<RenderGraphNode> Dependencies { get; } = new();

        public List<RenderGraphNode> Dependents { get; } = new();

        public List<ResourceBinding> Bindings { get; } = new();

        public List<ResourceBinding> Writes { get; } = new();

        public GraphDependencyBuilder Builder { get; }

        public GraphResourceContainer Container { get; }

        public void Reset(bool clearContainer = false)
        {
            Dependencies.Clear();
            Dependents.Clear();
            Bindings.Clear();
            Builder.Clear();
            if (clearContainer)
            {
                Container.Clear();
            }
        }

        public int QueueIndex { get; internal set; }

        IEnumerable<RenderGraphNode> INode<RenderGraphNode>.Dependencies => Dependencies;

        public override string ToString()
        {
            return $"{Name}:{QueueIndex}";
        }
    }
}