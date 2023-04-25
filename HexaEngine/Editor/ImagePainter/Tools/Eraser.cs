namespace HexaEngine.Editor.ImagePainter.Tools
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.ImagePainter;
    using ImGuiNET;
    using System.Numerics;

    public class Eraser : Tool
    {
        private Vector2 brushSize = Vector2.One;

        public override string Icon => "\xED60##EraserTool";

        public override string Name => "Eraser";

        public override void Init(IGraphicsDevice device)
        {
        }

        public override void DrawSettings()
        {
            ImGui.InputFloat2("Size", ref brushSize);
        }

        public override void Draw(Vector2 position, Vector2 ratio, IGraphicsContext context)
        {
        }

        public override void DrawPreview(Vector2 position, Vector2 ratio, IGraphicsContext context)
        {
        }

        public override void Dispose()
        {
        }
    }
}