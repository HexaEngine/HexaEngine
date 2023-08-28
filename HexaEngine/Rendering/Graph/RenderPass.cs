namespace HexaEngine.Rendering.Graph
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;

    public class RenderPass
    {
        private readonly List<ResourceBinding> readAndWriteBindings = new();
        private readonly List<ResourceBinding> readBindings = new();
        private readonly List<ResourceTarget> writeBindings = new();

        private readonly string name;
        private readonly RenderPassType type;
        private readonly RenderPassMetadata metadata;
        private int index = 0;
        private RenderGraphNode node;

        public RenderPass(string name, RenderPassType type = RenderPassType.Default)
        {
            this.name = name;
            this.type = type;
            metadata = new(name, type);
        }

        public string Name => name;

        public RenderPassType Type => type;

        public RenderPassMetadata Metadata => metadata;

        public bool Enabled { get; set; }

        public int Index => index;

        public IReadOnlyList<ResourceBinding> ReadDependencies => readBindings;

        public IReadOnlyList<ResourceBinding> WriteDependencies => writeBindings;

        public void Build(RenderGraph graph)
        {
            index = graph.AddRenderPass(metadata);
            node = graph.Nodes[index];
            for (int i = 0; i < readBindings.Count; i++)
            {
                node.Bindings.Add(readBindings[i]);
            }
            for (int i = 0; i < writeBindings.Count; i++)
            {
                node.Writes.Add(writeBindings[i]);
            }
        }

        public virtual void Init(GraphResourceBuilder creator, GraphPipelineBuilder pipelineCreator, IGraphicsDevice device, ICPUProfiler? profiler)
        {
        }

        public virtual void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
        }

        public virtual void Release()
        {
        }

        private void AddBinding(ResourceBinding binding)
        {
            readAndWriteBindings.Add(binding);
        }

        private void RemoveBinding(ResourceBinding binding)
        {
            readAndWriteBindings.Remove(binding);
        }

        public void AddWriteDependency(ResourceTarget target)
        {
            AddBinding(target);
            writeBindings.Add(target);
        }

        public void RemoveWriteDependency(ResourceTarget target)
        {
            writeBindings.Remove(target);
            RemoveBinding(target);
        }

        public void AddReadDependency(ResourceBinding source)
        {
            AddBinding(source);
            readBindings.Add(source);
        }

        public void RemoveReadDependency(ResourceBinding source)
        {
            readBindings.Remove(source);
            RemoveBinding(source);
        }

        public bool HasAnyDependencies()
        {
            return readBindings.Count != 0;
        }
    }

    public class RenderSubPass
    {
        private readonly string name;
        private readonly RenderPass parent;

        private readonly List<ResourceBinding> readAndWriteBindings = new();
        private readonly List<ResourceBinding> readBindings = new();
        private readonly List<ResourceTarget> writeBindings = new();

        public RenderSubPass(string name, RenderPass parent)
        {
            this.name = name;
            this.parent = parent;
        }

        public string Name => name;

        public RenderPass Parent => parent;

        public virtual void Init(IGraphicsDevice device)
        {
        }

        public virtual void Execute(IGraphicsContext context)
        {
        }

        private void AddBinding(ResourceBinding binding)
        {
            readAndWriteBindings.Add(binding);
        }

        private void RemoveBinding(ResourceBinding binding)
        {
            readAndWriteBindings.Remove(binding);
        }

        public void AddWriteDependency(ResourceTarget target)
        {
            AddBinding(target);
            writeBindings.Add(target);
            parent.AddWriteDependency(target);
        }

        public void RemoveWriteDependency(ResourceTarget target)
        {
            parent.RemoveWriteDependency(target);
            writeBindings.Remove(target);
            RemoveBinding(target);
        }

        public void AddReadDependency(ResourceBinding source)
        {
            AddBinding(source);
            readBindings.Add(source);
            parent.AddReadDependency(source);
        }

        public void RemoveReadDependency(ResourceBinding source)
        {
            parent.RemoveReadDependency(source);
            readBindings.Remove(source);
            RemoveBinding(source);
        }

        public bool HasAnyDependencies()
        {
            return readBindings.Count != 0;
        }
    }

    public class ComputePass : RenderPass
    {
        public ComputePass(string name, RenderPassType type = RenderPassType.Default) : base(name, type)
        {
        }
    }

    public class DrawPass : RenderPass
    {
        public DrawPass(string name, RenderPassType type = RenderPassType.Default) : base(name, type)
        {
        }

        public IRenderTargetView RenderTargetView { get; set; }

        public IDepthStencilView DepthStencilView { get; set; }
    }
}