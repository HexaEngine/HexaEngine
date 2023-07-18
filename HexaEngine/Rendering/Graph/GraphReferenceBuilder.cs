namespace HexaEngine.Rendering.Graph
{
    public class GraphReferenceBuilder
    {
        private RenderGraphNode node;
        private readonly List<ResourceBinding> readBindings = new();
        private readonly List<ResourceTarget> writeBindings = new();

        private readonly List<string> runAfter = new();
        private readonly List<string> runBefore = new();

        public GraphReferenceBuilder(RenderGraphNode node)
        {
            this.node = node;
        }

        public void Build(RenderGraph graph)
        {
        }

        public void Clear()
        {
            readBindings.Clear();
            writeBindings.Clear();
            runAfter.Clear();
            runBefore.Clear();
        }

        public void AddWrite(ResourceTarget target)
        {
            writeBindings.Add(target);
        }

        public void AddWrite(string target)
        {
            AddWrite(new ResourceTarget(target));
        }

        public void AddRead(ResourceBinding source)
        {
            readBindings.Add(source);
        }

        public void AddRead(string source)
        {
            AddRead(new ResourceBinding(source));
        }

        public void RunBefore(string name)
        {
            runBefore.Add(name);
        }

        public void RunAfter(string name)
        {
            runAfter.Add(name);
        }
    }
}