namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Scenes;

    [EditorWindowCategory("Debug")]
    public class RendererWidget : EditorWindow
    {
        private readonly ISceneRenderer renderer;

        public RendererWidget(ISceneRenderer renderer)
        {
            this.renderer = renderer;
        }

        protected override string Name => "Renderer";

        public override void DrawContent(IGraphicsContext context)
        {
            renderer.DrawSettings();
        }
    }
}