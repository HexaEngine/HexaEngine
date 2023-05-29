namespace HexaEngine.Editor.ImagePainter
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor;

    public class ToolProperties : EditorWindow
    {
        private readonly Toolbox toolbox;

        protected override string Name => "Tool properties";

        public ToolProperties(Toolbox toolbox)
        {
            this.toolbox = toolbox;
        }

        public override void DrawContent(IGraphicsContext context)
        {
            toolbox.Current?.DrawSettings();
        }
    }
}