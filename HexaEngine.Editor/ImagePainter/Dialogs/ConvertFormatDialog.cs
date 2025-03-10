﻿namespace HexaEngine.Editor.ImagePainter.Dialogs
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Textures;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Editor.ImagePainter;

    public class ConvertFormatDialog : Modal
    {
        private readonly ImagePainterWindow imagePainter;
        private Format format;

        public ConvertFormatDialog(ImagePainterWindow imagePainter)
        {
            this.imagePainter = imagePainter;
        }

        public override string Name => "Convert Format";

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
            ComboEnumHelper<Format>.Combo("Format", ref format);
            if (ImGui.Button("Cancel"))
            {
                Close();
            }
            ImGui.SameLine();
            if (ImGui.Button("Convert"))
            {
                if (imagePainter.Source == null)
                {
                    return;
                }

                var image = imagePainter.Source.ToScratchImage();

                var converted = image.Convert(format, TexFilterFlags.Default);

                image.Dispose();

                imagePainter.Load(converted);

                converted.Dispose();

                Close();
            }
        }
    }
}