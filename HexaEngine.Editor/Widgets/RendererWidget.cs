namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Windows;

    [EditorWindowCategory("Debug")]
    public class RendererWidget : EditorWindow
    {
        protected override string Name => "Renderer";

        public override void DrawContent(IGraphicsContext context)
        {
            var window = Application.MainWindow;
            if (window is not Window win)
            {
                return;
            }

            win.Renderer.DrawSettings();
        }
    }
}