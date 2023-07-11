namespace HexaEngine.Rendering.Graph
{
    using HexaEngine.Core.Graphics;
    using System;
    using System.Threading.Tasks;

    public class ResourceBinding
    {
        public ResourceBinding(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public class ResourceTarget : ResourceBinding
    {
        public ResourceTarget(string name) : base(name)
        {
        }
    }

    public class ResourceSource : ResourceBinding
    {
        public ResourceSource(string name) : base(name)
        {
        }
    }

    public class RenderPassOld
    {
        private readonly List<ResourceBinding> reads = new();
        private readonly List<ResourceTarget> writes = new();
        private readonly List<ResourceSource> sources = new();

        private readonly string name;

        public RenderPassOld(string name)
        {
            this.name = name;
        }

        public bool Enabled { get; set; }

        public List<RenderPassOld> Dependencies { get; } = new();

        public IReadOnlyList<ResourceBinding> ReadDependencies => reads;

        public IReadOnlyList<ResourceBinding> WriteDependencies => writes;

        public virtual void Init(IGraphicsDevice device)
        {
        }

        public virtual void Execute(IGraphicsContext context)
        {
        }

        private void AddBinding(ResourceBinding binding)
        {
            reads.Add(binding);
        }

        private void RemoveBinding(ResourceBinding binding)
        {
            reads.Remove(binding);
        }

        protected void AddWriteDependency(ResourceTarget target)
        {
            AddBinding(target);
            writes.Add(target);
        }

        protected void RemoveWriteDependency(ResourceTarget target)
        {
            writes.Remove(target);
            RemoveBinding(target);
        }

        protected void AddReadDependency(ResourceSource source)
        {
            AddBinding(source);
            sources.Add(source);
        }

        protected void RemoveReadDependency(ResourceSource source)
        {
            sources.Remove(source);
            RemoveBinding(source);
        }

        public bool HasAnyDependencies()
        {
            return reads.Count != 0;
        }

        internal void Clear()
        {
            throw new NotImplementedException();
        }
    }

    public class ClearRenderTargetPass : RenderPassOld
    {
        public ClearRenderTargetPass(string name) : base(name)
        {
        }
    }

    public class ClearMultiRenderTargetPass : RenderPassOld
    {
        public ClearMultiRenderTargetPass(string name) : base(name)
        {
        }
    }

    public class ClearDepthStencilPass : RenderPassOld
    {
        public ClearDepthStencilPass(string name) : base(name)
        {
        }
    }

    public class ComputePass : RenderPassOld
    {
        public ComputePass(string name) : base(name)
        {
        }
    }

    public class DepthStencilPass : RenderPassOld
    {
        public DepthStencilPass(string name) : base(name)
        {
        }

        public IDepthStencilView DepthStencilView { get; set; }
    }

    public class DrawPass : RenderPassOld
    {
        public DrawPass(string name) : base(name)
        {
        }

        public IRenderTargetView RenderTargetView { get; set; }

        public IDepthStencilView DepthStencilView { get; set; }
    }

    public class DeferredDrawPass : RenderPassOld
    {
        private IGraphicsContext deferredContext;
        private ICommandList commandList;
        private Task task;
        protected bool invalidate;

        public DeferredDrawPass(string name) : base(name)
        {
        }

        public IRenderTargetView RenderTargetView { get; set; }

        public IDepthStencilView DepthStencilView { get; set; }

        public override void Init(IGraphicsDevice device)
        {
            deferredContext = device.CreateDeferredContext();
            task = new(() => Update(deferredContext));
        }

        public override sealed void Execute(IGraphicsContext context)
        {
            if (invalidate)
            {
                commandList.Dispose();
                Bind(deferredContext);
                Record(deferredContext);
                commandList = deferredContext.FinishCommandList(false);
            }

            task.Wait();
            task.Start();
            context.ExecuteCommandList(commandList, false);
        }

        public void Bind(IGraphicsContext context)
        {
            context.SetRenderTarget(RenderTargetView, DepthStencilView);
        }

        public virtual void Update(IGraphicsContext context)
        {
        }

        public virtual void Record(IGraphicsContext context)
        {
        }
    }

    public class MultiTargetDrawPass : RenderPassOld
    {
        public MultiTargetDrawPass(string name) : base(name)
        {
        }

        public IRenderTargetView[] RenderTargetViews { get; set; }

        public IDepthStencilView DepthStencilView { get; set; }

        public void Bind(IGraphicsContext context)
        {
            //context.SetRenderTargets();
        }
    }

    public class DeferredMultiTargetDrawPass : RenderPassOld
    {
        private IGraphicsContext deferredContext;
        private ICommandList commandList;
        private Task task;
        protected bool invalidate;

        public DeferredMultiTargetDrawPass(string name) : base(name)
        {
        }

        public IRenderTargetView[] RenderTargetViews { get; set; }

        public IDepthStencilView DepthStencilView { get; set; }

        public override void Init(IGraphicsDevice device)
        {
            deferredContext = device.CreateDeferredContext();
            task = new(() => Update(deferredContext));
        }

        public override sealed void Execute(IGraphicsContext context)
        {
            if (invalidate)
            {
                commandList.Dispose();
                Record(deferredContext);
                commandList = deferredContext.FinishCommandList(false);
            }

            task.Wait();
            task.Start();
            context.ExecuteCommandList(commandList, false);
        }

        public void Bind(IGraphicsContext context)
        {
            //context.SetRenderTargets();
        }

        public virtual void Update(IGraphicsContext context)
        {
        }

        public virtual void Record(IGraphicsContext context)
        {
        }
    }
}