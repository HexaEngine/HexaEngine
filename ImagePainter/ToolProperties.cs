namespace ImagePainter
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor;

    public class ToolProperties : ImGuiWindow
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