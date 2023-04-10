namespace HexaEngine.Editor.Painting
{
    using HexaEngine.Core.Graphics;
    using System;

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