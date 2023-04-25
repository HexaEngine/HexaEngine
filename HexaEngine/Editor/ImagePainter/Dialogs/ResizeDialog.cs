namespace HexaEngine.Editor.ImagePainter.Dialogs
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Textures;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Editor.ImagePainter;
    using ImGuiNET;

    public class ResizeDialog : Modal
    {
        private readonly ImagePainterWindow imagePainter;
        private readonly IGraphicsDevice device;
        private int width;
        private int height;

        public ResizeDialog(ImagePainterWindow imagePainter, IGraphicsDevice device)
        {
            this.imagePainter = imagePainter;
            this.device = device;
        }

        public override string Name => "Resize";

        protected override ImGuiWindowFlags Flags => ImGuiWindowFlags.AlwaysAutoResize;

        public override void Reset()
        {
        }

        public override void Show()
        {
            if (imagePainter.Source == null)
                return;
            width = imagePainter.Source.Metadata.Width;
            height = imagePainter.Source.Metadata.Height;
            base.Show();
        }

        protected override void DrawContent()
        {
            ImGui.InputInt("Width", ref width);
            ImGui.InputInt("Height", ref height);
            if (ImGui.Button("Cancel"))
            {
                Close();
            }
            ImGui.SameLine();
            if (ImGui.Button("Resize"))
            {
                if (imagePainter.Source == null)
                    return;

                var image = imagePainter.Source.ToScratchImage(device);

                var converted = image.Resize(width, height, TexFilterFlags.Default);

                image.Dispose();

                imagePainter.Load(converted);

                converted.Dispose();

                Close();
            }
        }
    }
}