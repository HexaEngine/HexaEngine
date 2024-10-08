namespace HexaEngine.Graphics
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Collections;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Lights;
    using HexaEngine.Scenes;

    public delegate void QueueIndexChangedEventHandler(IDrawable sender, uint oldIndex, uint newIndex);

    public delegate void DrawableInvalidatedEventHandler(IDrawable sender);

    public interface IDrawable : IComponent, IHasFlags<RendererFlags>
    {
        internal int LeafId { get; set; }

        uint QueueIndex { get; }

        bool BatchSupport { get; }

        event QueueIndexChangedEventHandler? QueueIndexChanged;

        event DrawableInvalidatedEventHandler? DrawableInvalidated;

        BoundingBox BoundingBox { get; }

        void Load(IGraphicsDevice device);

        void Unload();

        void Update(IGraphicsContext context);

        void VisibilityTest(CullingContext context);

        void DrawDepth(IGraphicsContext context);

        void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type);

        void Draw(IGraphicsContext context, RenderPath path);

        void Bake(IGraphicsContext context);

        void Draw(IGraphicsContext deferred, string pass);

        string DebugName { get; }
    }
}