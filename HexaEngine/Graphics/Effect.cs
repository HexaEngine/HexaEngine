namespace HexaEngine.Graphics
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Mathematics;
    using HexaEngine.Objects;
    using System.Numerics;

    public class Effect : GraphicsPipeline, IEffect
    {
        public Vector4 AutoClearColor;
        public bool AutoClear;
        public bool AutoClearDepth;
        public bool AutoSetTarget = true;
        public PinType TargetType;
        public readonly List<(int, string, PinType)> ResourceSlots = new();

        public Effect(IGraphicsDevice device, GraphicsPipelineDesc desc) : base(device, desc)
        {
        }

        public IRenderTargetView? Target { get; set; }

        public IDepthStencilView? DepthStencilView { get; set; }

        public RenderTargetViewArray? Targets { get; set; }

        public IShaderResourceView[]? Inputs { get; set; }

        public IShaderResourceView[]? Outputs { get; set; }

        public IPrimitive? Mesh { get; set; }

        public virtual void Draw(IGraphicsContext context)
        {
        }

        protected virtual void DrawAuto(IGraphicsContext context, Viewport viewport)
        {
            Mesh?.DrawAuto(context, this, viewport);
        }

        public override void BeginDraw(IGraphicsContext context, Viewport viewport)
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

        public void BeginResize()
        {
            throw new NotImplementedException();
        }

        public void EndResize(int width, int height)
        {
            throw new NotImplementedException();
        }

        public async Task Initialize(IGraphicsDevice device, int width, int height)
        {
            throw new NotImplementedException();
        }
    }
}