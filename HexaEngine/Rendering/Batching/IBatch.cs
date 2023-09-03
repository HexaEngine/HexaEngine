namespace HexaEngine.Rendering.Batching
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Lights;
    using HexaEngine.Scenes;
    using System.Collections.Generic;

    public interface IBatch<T> : IBatch where T : IBatchRenderer
    {
        new BatchRendererPair<T> this[int index] { get; }

        new IReadOnlyList<BatchRendererPair<T>> Objects { get; }

        void AddObject(GameObject parent, T t);

        bool Contains(GameObject parent, T t);

        int IndexOf(GameObject parent, T t);

        void RemoveObject(GameObject parent, T t);
    }

    public interface IBatch
    {
        BatchRendererPair this[int index] { get; }

        IEnumerable<BatchRendererPair> Objects { get; }

        int Count { get; }

        void AddObject(GameObject parent, IBatchRenderer t);

        bool CanBatch(IBatchRenderer renderer);

        void Clear();

        bool Contains(GameObject parent, IBatchRenderer t);

        void DrawDeferred(IGraphicsContext context);

        void DrawDepth(IGraphicsContext context);

        void DrawDepth(IGraphicsContext context, IBuffer camera);

        void DrawForward(IGraphicsContext context);

        void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type);

        int IndexOf(GameObject parent, IBatchRenderer t);

        void RemoveAt(int index);

        void RemoveObject(GameObject parent, IBatchRenderer t);

        void Sort();

        void Update(IGraphicsContext context);

        void VisibilityTest(IGraphicsContext context, Camera camera);
    }
}