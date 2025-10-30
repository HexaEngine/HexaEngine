namespace HexaEngine.Graphics.Graph
{
    using HexaEngine.Core.Collections;

    public class RenderGraphNode : INode<RenderGraphNode>
    {
        private readonly RenderPass pass;
        private readonly GraphDependencyBuilder builder;

        public RenderGraphNode(RenderPass pass, GraphNodeRegistry registry)
        {
            this.pass = pass;
            builder = new(this, registry);
            Container = new();
            Type = pass.GetType();
        }

        public string Name => pass.Name;

        public RenderPass Pass => pass;

        public Type Type { get; }

        public HashSet<RenderGraphNode> Dependencies { get; } = [];

        public HashSet<RenderGraphNode> Dependents { get; } = [];

        public GraphDependencyBuilder Builder => builder;

        public GraphResourceContainer Container { get; }

        public void Reset(bool clearContainer = false)
        {
            Dependencies.Clear();
            Dependents.Clear();
            Builder.Clear();
            if (clearContainer)
            {
                Container.Clear();
            }
        }

        public void BuildDependencies()
        {
            Reset(true);
            pass.BuildDependencies(Builder);
        }

        public void ResolveDependencies()
        {
            Builder.Build();
        }

        public void AddDependency(RenderGraphNode node)
        {
            Dependencies.Add(node);
            node.Dependents.Add(this);
        }

        public void AddDependent(RenderGraphNode node)
        {
            Dependents.Add(node);
            node.Dependencies.Add(this);
        }

        IEnumerable<RenderGraphNode> INode<RenderGraphNode>.Dependencies => Dependencies;

        public override string ToString()
        {
            return Name;
        }
    }
}