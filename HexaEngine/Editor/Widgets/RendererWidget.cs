namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;

    public class RendererWidget : ImGuiWindow
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