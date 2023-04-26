namespace HexaEngine.Editor.ImagePainter.Dialogs
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Editor.ImagePainter;
    using ImGuiNET;

    public class OverwriteFormatDialog : Modal
    {
        private readonly ImagePainterWindow imagePainter;
        private readonly IGraphicsDevice device;
        private Format format;

        public OverwriteFormatDialog(ImagePainterWindow imagePainter, IGraphicsDevice device)
        {
            this.imagePainter = imagePainter;
            this.device = device;
        }

        public override string Name => "Overwrite Format";

        protected override ImGuiWindowFlags Flags => ImGuiWindowFlags.AlwaysAutoResize;

        public override void Reset()
        {
        }

        public override void Show()
        {
            if (imagePainter.Source == null)
            {
                return;
            }

            format = imagePainter.Source.Metadata.Format;
            base.Show();
        }

        protected override void DrawContent()
        {
            ImGuiEnumHelper<Format>.Combo("Format", ref format);

            if (ImGui.Button("Cancel"))
            {
                Close();
            }
            ImGui.SameLine();
            if (ImGui.Button("Overwrite"))
            {
                if (imagePainter.Source == null)
                {
                    return;
                }

                IScratchImage image = imagePainter.Source.ToScratchImage(device);

                if (image.OverwriteFormat(format))
                {
                    imagePainter.Load(image);
                }

                image.Dispose();
            }
        }
    }
}