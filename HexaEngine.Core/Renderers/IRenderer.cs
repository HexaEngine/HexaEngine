namespace HexaEngine.Core.Renderers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Instances;

    public interface IRenderer
    {
        public uint QueueIndex { get; }

        public void Initialize(IGraphicsDevice device, InstanceManager manager);

        public void Uninitialize();

        public void VisibilityTest(IGraphicsContext context);

        public void DrawDepth(IGraphicsContext context);

        public void Draw(IGraphicsContext context);

        public void DrawIndirect(IGraphicsContext context, IBuffer drawArgs, uint offset);
    }
}