namespace Editor
{
    using Hexa.NET.DebugDraw;
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor;
    using HexaEngine.Graphics.Overlays;
    using HexaEngine.Scenes.Managers;
    using System.Numerics;

    public class EditorRenderer : OverlayRendererBase
    {
        private EditorWindow window;

        public EditorRenderer(EditorWindow window)
        {
            this.window = window;
        }

        public override int ZIndex { get; } = 0;

        protected override void Draw(IGraphicsContext context, Viewport viewport, Texture2D target, DepthStencil depthStencil)
        {
            if (!window.EditorInitialized) return;
            DrawEditor(context);
        }

        public static void DrawEditor(IGraphicsContext context)
        {
            DebugDraw.SetCamera(CameraManager.Current?.Transform.ViewProjection ?? Matrix4x4.Identity);
            Designer.Draw(context);
            SceneWindow.Draw();
        }

        protected override void Init()
        {
        }

        protected override void Release()
        {
        }
    }
}