namespace HexaEngine.Editor.ImagePainter.Dialogs
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Textures;
    using HexaEngine.Editor.Dialogs;
    using ImGuiNET;
    using System;
    using System.Text;

    public class GenerateMipMapsDialog : Modal
    {
        private readonly ImagePainterWindow imagePainter;
        private readonly IGraphicsDevice device;

        public GenerateMipMapsDialog(ImagePainterWindow imagePainter, IGraphicsDevice device)
        {
            this.imagePainter = imagePainter;
            this.device = device;
        }

        public override string Name => "Generate MipMaps";

        protected override ImGuiWindowFlags Flags => ImGuiWindowFlags.None;

        public override void Reset()
        {
            throw new NotImplementedException();
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
                    return;

                var image = imagePainter.Source.ToScratchImage(device);

                var converted = image.GenerateMipMaps(TexFilterFlags.Default);

                image.Dispose();

                imagePainter.Load(converted);

                converted.Dispose();

                Close();
            }
        }
    }
}