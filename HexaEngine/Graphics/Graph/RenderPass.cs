namespace HexaEngine.Graphics.Graph
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Profiling;
    using System.Reflection;

    public abstract class RenderPass
    {
        private readonly string name;
        private readonly RenderPassType type;

        protected RenderPass(string name, RenderPassType type = RenderPassType.Default)
        {
            this.name = name;
            this.type = type;
        }

        public string Name => name;

        public RenderPassType Type => type;

        public bool Enabled { get; set; }

        public virtual void BuildDependencies(GraphDependencyBuilder builder)
        {
        }

        public virtual void Init(GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
        }

        public virtual void Prepare(GraphResourceBuilder creator)
        {
        }

        public abstract void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler? profiler);

        public virtual void Release()
        {
        }

        public virtual void OnResize(GraphResourceBuilder creator)
        {
            Release();
        }
    }

    public abstract class RenderPass<T> : RenderPass
    {
        protected RenderPass(RenderPassType type = RenderPassType.Default) : base(GetPassName(), type)
        {
        }

        private static string GetPassName()
        {
            var type = typeof(T);
            var attr = type.GetCustomAttribute<RenderPassNameAttribute>();
            if (attr != null)
            {
                return attr.Name;
            }
            return type.Name;
        }
    }

    public abstract class ComputePass : RenderPass
    {
        public ComputePass(string name, RenderPassType type = RenderPassType.Default) : base(name, type)
        {
        }
    }

    public abstract class DrawPass : RenderPass
    {
        public DrawPass(string name, RenderPassType type = RenderPassType.Default) : base(name, type)
        {
        }

        public IRenderTargetView RenderTargetView { get; set; } = null!;

        public IDepthStencilView DepthStencilView { get; set; } = null!;
    }
}