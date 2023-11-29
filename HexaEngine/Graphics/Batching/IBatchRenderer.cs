namespace HexaEngine.Graphics.Batching
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Lights;
    using HexaEngine.Scenes;

    public interface IBatchRenderer : IComparable<IBatchRenderer>, IEquatable<IBatchRenderer>
    {
        void Dispose();

        void Initialize(IGraphicsDevice device);

        void DrawDeferred<T>(IGraphicsContext context, Batch<T> batch) where T : IBatchRenderer;

        void DrawDepth<T>(IGraphicsContext context, Batch<T> batch) where T : IBatchRenderer;

        void DrawDepth<T>(IGraphicsContext context, Batch<T> batch, IBuffer camera) where T : IBatchRenderer;

        void DrawForward<T>(IGraphicsContext context, Batch<T> batch) where T : IBatchRenderer;

        void DrawShadowMap<T>(IGraphicsContext context, Batch<T> batch, IBuffer light, ShadowType type) where T : IBatchRenderer;

        void Update<T>(IGraphicsContext context, Batch<T> batch) where T : IBatchRenderer;

        void VisibilityTest<T>(IGraphicsContext context, Batch<T> batch, Camera camera) where T : IBatchRenderer;
    }
}