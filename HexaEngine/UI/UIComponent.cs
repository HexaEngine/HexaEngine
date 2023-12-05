namespace HexaEngine.UI
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;

    public abstract class UIComponent : IRendererComponent
    {
        public uint QueueIndex { get; } = (uint)RenderQueueIndex.Overlay;

        public BoundingBox BoundingBox { get; }

        public abstract string DebugName { get; }

        public GameObject GameObject { get; set; }

        public RendererFlags Flags { get; } = RendererFlags.Forward | RendererFlags.Draw | RendererFlags.Update | RendererFlags.NoDepthTest;
        public bool BatchSupport { get; }

        public event QueueIndexChangedEventHandler? QueueIndexChanged;

        public abstract void Awake();

        public abstract void Destroy();

        public abstract void Draw(IGraphicsContext context, RenderPath path);

        public abstract void Load(IGraphicsDevice device);

        public abstract void Unload();

        public abstract void Update(IGraphicsContext context);

        public void DrawDepth(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            throw new NotImplementedException();
        }

        public void Bake(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public void VisibilityTest(CullingContext context)
        {
            throw new NotImplementedException();
        }
    }
}