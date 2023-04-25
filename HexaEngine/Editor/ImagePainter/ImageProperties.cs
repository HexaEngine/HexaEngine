namespace HexaEngine.Editor.ImagePainter
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Textures;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor;
    using ImGuiNET;
    using System;

    public class ImageProperties : ImGuiWindow
    {
        private static readonly Format[] formats = Enum.GetValues<Format>();
        private static readonly string[] formatNames = Enum.GetNames<Format>();
        private readonly ImagePainterWindow painter;
        private ImageSource? image;

        public ImageProperties(ImagePainterWindow painter)
        {
            this.painter = painter;
        }

        public ImageSource? Image { get => image; set => image = value; }

        protected override string Name => "Image properties";

        public override void DrawContent(IGraphicsContext context)
        {
            if (image == null)
                return;
            var metadata = image.Metadata;
            ImGui.InputInt2("Size", ref metadata.Width, ImGuiInputTextFlags.ReadOnly);

            ImGuiEnumHelper<Format>.Text(metadata.Format);

            ImGui.InputInt("ArraySize", ref metadata.ArraySize, 0, 0, ImGuiInputTextFlags.ReadOnly);

            var arrayIndex = image.ArrayIndex;
            if (ImGui.SliderInt("ArrayIndex", ref arrayIndex, 0, metadata.ArraySize - 1))
            {
                image.ArrayIndex = arrayIndex;
            }
            ImGui.InputInt("MipLevels", ref metadata.MipLevels, 0, 0, ImGuiInputTextFlags.ReadOnly);

            var mipIndex = image.MipLevel;
            if (ImGui.SliderInt("MipLevel", ref mipIndex, 0, metadata.MipLevels - 1))
            {
                image.MipLevel = mipIndex;
            }

            if ((metadata.MiscFlags & TexMiscFlags.TextureCube) != 0)
                ImGui.Text("HasCubeFlag");
        }
    }
}