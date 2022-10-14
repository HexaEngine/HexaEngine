namespace HexaEngine.Graphics
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Nodes;
    using HexaEngine.Mathematics;
    using HexaEngine.Objects;
    using System.Numerics;

    public abstract class Effect : Pipeline, IEffect
    {
        public Vector4 AutoClearColor;
        public bool AutoClear;
        public bool AutoClearDepth;
        public bool AutoSetTarget = true;
        public PinType TargetType;
        public readonly List<(int, string, PinType)> ResourceSlots = new();

        public Effect(IGraphicsDevice device, PipelineDesc desc) : base(device, desc)
        {
        }

        public IRenderTargetView? Target { get; set; }

        public IDepthStencilView? DepthStencilView { get; set; }

        public RenderTargetViewArray? Targets { get; set; }

        public IPrimitive? Mesh { get; set; }

        public abstract void Draw(IGraphicsContext context);

        protected virtual void DrawAuto(IGraphicsContext context, Viewport viewport)
        {
            Mesh?.DrawAuto(context, this, viewport);
        }

        protected override void BeginDraw(IGraphicsContext context, Viewport viewport)
        {
            if (AutoClear && Target != null)
                context.ClearRenderTargetView(Target, AutoClearColor);
            if (AutoClearDepth && DepthStencilView != null)
                context.ClearDepthStencilView(DepthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);

            if (AutoSetTarget)
                context.SetRenderTarget(Target, DepthStencilView);

            base.BeginDraw(context, viewport);
        }

        public override void Dispose()
        {
            Mesh?.Dispose();
            base.Dispose();
        }

        public abstract void DrawSettings();
    }
}