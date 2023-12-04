namespace HexaEngine.Graphics.Batching
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Lights;
    using HexaEngine.Scenes;

    public interface IBatchRenderer : IDisposable
    {
        void DrawDeferred(IGraphicsContext context, IBatch batch);

        void DrawDepth(IGraphicsContext context, IBatch batch);

        void DrawDepth(IGraphicsContext context, IBatch batch, IBuffer camera);

        void DrawForward(IGraphicsContext context, IBatch batch);

        void DrawShadowMap(IGraphicsContext context, IBatch batch, IBuffer light, ShadowType type);

        void BeginUpdate(IGraphicsContext context);

        void Update(IGraphicsContext context, IBatch batch);

        void EndUpdate(IGraphicsContext context);

        void VisibilityTest(IGraphicsContext context, IBatch batch, Camera camera);
    }

    public interface IBatchRenderer<T> : IBatchRenderer where T : IBatchInstance
    {
        void DrawDeferred(IGraphicsContext context, IBatch<T> batch);

        void DrawDepth(IGraphicsContext context, IBatch<T> batch);

        void DrawDepth(IGraphicsContext context, IBatch<T> batch, IBuffer camera);

        void DrawForward(IGraphicsContext context, IBatch<T> batch);

        void DrawShadowMap(IGraphicsContext context, IBatch<T> batch, IBuffer light, ShadowType type);

        void Update(IGraphicsContext context, IBatch<T> batch);

        void VisibilityTest(IGraphicsContext context, IBatch<T> batch, Camera camera);
    }
}