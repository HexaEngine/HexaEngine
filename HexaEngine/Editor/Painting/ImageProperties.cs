namespace HexaEngine.Editor.Painting
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Textures;
    using ImGuiNET;
    using System;

    public class ImageProperties : ImGuiWindow
    {
        private static readonly Format[] formats = Enum.GetValues<Format>();
        private static readonly string[] formatNames = Enum.GetNames<Format>();
        private readonly ImagePainter painter;
        private TexMetadata metadata;

        public ImageProperties(ImagePainter painter)
        {
            this.painter = painter;
        }

        public TexMetadata Metadata { get => metadata; set => metadata = value; }

        protected override string Name => "Image properties";

        public override void DrawContent(IGraphicsContext context)
        {
            ImGui.InputInt2("Size", ref metadata.Width);
            var formatIndex = Array.IndexOf(formats, metadata.Format);
            if (ImGui.Combo("Format", ref formatIndex, formatNames, formatNames.Length))
            {
                var format = formats[formatIndex];
                painter.Convert(format);
            }
            ImGui.InputInt("ArraySize", ref metadata.ArraySize, 0, 0, ImGuiInputTextFlags.ReadOnly);
            ImGui.InputInt("MipLevels", ref metadata.MipLevels, 0, 0, ImGuiInputTextFlags.ReadOnly);
        }
    }
}