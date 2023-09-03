namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Rendering.Renderers;

    public class PostProcessWindow : EditorWindow
    {
        protected override string Name => "Post Process";

        public override void DrawContent(IGraphicsContext context)
        {
            SceneRenderer? renderer = SceneRenderer.Current;
            if (renderer == null)
            {
                return;
            }
        }
    }
}