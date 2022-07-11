namespace HexaEngine.Shaders.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Specialized;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Objects;
    using System.Collections.Generic;
    using System.Numerics;

    public abstract class Effect : Pipeline
    {
        public bool AutoClear;
        public bool AutoClearDepth;

        public Effect(IGraphicsDevice device, PipelineDesc desc) : base(device, desc)
        {
        }

        public IRenderTargetView Target { get; set; }

        public IDepthStencilView DepthStencilView { get; set; }

        public RenderTargetViewArray Targets { get; set; }

        public IPrimitive Mesh { get; set; }

        public abstract void Draw(IGraphicsContext context, IView view);

        protected virtual void DrawAuto(IGraphicsContext context, Viewport viewport, IView view)
        {
            Mesh.DrawAuto(context, this, viewport, view, default);
        }

        protected override void BeginDraw(IGraphicsContext context, Viewport viewport, IView view, Matrix4x4 transform)
        {
            if (AutoClear)
                context.ClearRenderTargetView(Target, Vector4.Zero);
            if (AutoClearDepth)
                context.ClearDepthStencilView(DepthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1, 0);

            context.SetRenderTargets(Target, DepthStencilView);

            base.BeginDraw(context, viewport, view, transform);
        }

        public override void Dispose()
        {
            Mesh.Dispose();
            base.Dispose();
        }
    }
}