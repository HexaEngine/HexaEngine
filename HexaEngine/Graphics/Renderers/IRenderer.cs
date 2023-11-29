namespace HexaEngine.Graphics.Renderers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Lights;
    using HexaEngine.Scenes;
    using System.Numerics;

    public interface IRenderer
    {
        void Dispose();
        void DrawDeferred(IGraphicsContext context);
        void DrawDepth(IGraphicsContext context);
        void DrawDepth(IGraphicsContext context, IBuffer camera);
        void DrawForward(IGraphicsContext context);
        void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type);
        void Update(IGraphicsContext context, Matrix4x4 transform);
        void VisibilityTest(IGraphicsContext context, Camera camera);
    }
}