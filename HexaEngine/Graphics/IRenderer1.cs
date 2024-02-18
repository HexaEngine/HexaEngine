namespace HexaEngine.Graphics
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public interface IRenderer1 : IDisposable
    {
        void Update(IGraphicsContext context);

        void DrawDeferred(IGraphicsContext context);

        void DrawDepth(IGraphicsContext context);

        void DrawForward(IGraphicsContext context);

        void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type);

        void VisibilityTest(CullingContext context);

        bool AddInstance(IRendererInstance instance);

        bool RemoveInstance(IRendererInstance instance);

        bool ContainsInstance(IRendererInstance instance);

        void Clear();
    }

    public interface IRenderer1<T> : IRenderer1 where T : IRendererInstance
    {
        void AddInstance(T instance);

        bool RemoveInstance(T instance);

        bool ContainsInstance(T instance);
    }

    public delegate void QueueIndexChangedEventHandler1(IRendererInstance sender, uint oldIndex, uint newIndex);

    public interface IRendererInstance
    {
        uint QueueIndex { get; }

        Matrix4x4 Transform { get; }

        BoundingBox BoundingBox { get; }

        event QueueIndexChangedEventHandler1? QueueIndexChanged;
    }
}