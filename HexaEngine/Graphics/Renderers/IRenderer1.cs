namespace HexaEngine.Graphics.Renderers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Lights;
    using System.Numerics;

    public interface IRenderer1 : IDisposable
    {
        void Bake(IGraphicsContext context, object instance);

        void DrawDeferred(IGraphicsContext context, object instance);

        void DrawDepth(IGraphicsContext context, object instance);

        void DrawDepth(IGraphicsContext context, object instance, IBuffer camera);

        void DrawForward(IGraphicsContext context, object instance);

        void DrawShadowMap(IGraphicsContext context, object instance, IBuffer light, ShadowType type);

        void Update(IGraphicsContext context, Matrix4x4 transform, object instance);

        void VisibilityTest(CullingContext context, object instance);

        void Initialize(IGraphicsDevice device, CullingContext cullingContext);

        bool CanRender(object instance);
    }

    public interface IRenderer1<T>
    {
        void Bake(IGraphicsContext context, T instance);

        void Dispose();

        void DrawDeferred(IGraphicsContext context, T instance);

        void DrawDepth(IGraphicsContext context, T instance);

        void DrawDepth(IGraphicsContext context, T instance, IBuffer camera);

        void DrawForward(IGraphicsContext context, T instance);

        void DrawShadowMap(IGraphicsContext context, T instance, IBuffer light, ShadowType type);

        void Update(IGraphicsContext context, Matrix4x4 transform, T instance);

        void VisibilityTest(CullingContext context, T instance);
    }
}