namespace HexaEngine.Editor.ImagePainter.Dialogs
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Textures;
    using HexaEngine.Editor.Dialogs;

    public class GenerateMipMapsDialog : Modal
    {
        private readonly ImagePainterWindow imagePainter;

        public GenerateMipMapsDialog(ImagePainterWindow imagePainter)
        {
            this.imagePainter = imagePainter;
        }

        public override string Name => "Generate MipMaps";

        protected override ImGuiWindowFlags Flags => ImGuiWindowFlags.None;

        public override void Reset()
        {
        }

        protected override void DrawContent()
        {
            if (ImGui.Button("Cancel"))
            {
                Close();
            }
            ImGui.SameLine();
            if (ImGui.Button("Generate"))
            {
                if (imagePainter.Source == null)
                {
                    return;
                }

                var image = imagePainter.Source.ToScratchImage();

                var converted = image.GenerateMipMaps(TexFilterFlags.Default);

                image.Dispose();

                imagePainter.Load(converted);

                converted.Dispose();

                Close();
            }
        }
    }
}