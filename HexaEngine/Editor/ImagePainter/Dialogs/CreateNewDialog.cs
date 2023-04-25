namespace HexaEngine.Editor.ImagePainter.Dialogs
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Textures;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Editor.ImagePainter;
    using ImGuiNET;
    using System;

    public class CreateNewDialog : Modal
    {
        private static readonly TextureDimension[] dimensions = { TextureDimension.Texture2D, TextureDimension.Texture3D, TextureDimension.TextureCube };
        private static readonly string[] dimensionNames = { "Texture2D", "Texture3D", "TextureCube" };
        private static readonly Format[] formats = Enum.GetValues<Format>();
        private static readonly string[] formatNames = Enum.GetNames<Format>();
        private readonly ImagePainterWindow imagePainter;
        private readonly IGraphicsDevice device;

        private TextureDimension dimension = TextureDimension.Texture2D;
        private Format format = Format.R8G8B8A8UNorm;
        private int width;
        private int height;
        private int depth;
        private int arraySize = 1;
        private int cubeCount;

        private IScratchImage? result;

        public CreateNewDialog(ImagePainterWindow imagePainter, IGraphicsDevice device)
        {
            this.imagePainter = imagePainter;
            this.device = device;
        }

        public override string Name => "Create new";

        protected override ImGuiWindowFlags Flags => ImGuiWindowFlags.AlwaysAutoResize;

        public override void Reset()
        {
        }

        protected override void DrawContent()
        {
            var dim = Array.IndexOf(dimensions, dimension);
            if (ImGui.Combo("Dimension", ref dim, dimensionNames, dimensionNames.Length))
            {
                dimension = dimensions[dim];
            }

            ImGuiEnumHelper<Format>.Combo("Format", ref format);

            switch (dimension)
            {
                case TextureDimension.Texture2D:
                    ImGui.InputInt("Width", ref width);
                    ImGui.InputInt("Height", ref height);
                    ImGui.InputInt("ArraySize", ref arraySize);
                    break;

                case TextureDimension.Texture3D:
                    ImGui.InputInt("Width", ref width);
                    ImGui.InputInt("Height", ref height);
                    ImGui.InputInt("Depth", ref depth);
                    break;

                case TextureDimension.TextureCube:
                    ImGui.InputInt("Width", ref width);
                    ImGui.InputInt("Height", ref height);
                    ImGui.InputInt("CubeCount", ref cubeCount);
                    break;
            }

            if (ImGui.Button("Cancel"))
            {
                Close();
            }
            ImGui.SameLine();
            if (ImGui.Button("Create"))
            {
                switch (dimension)
                {
                    case TextureDimension.Texture2D:
                        result = device.TextureLoader.Initialize2D(format, width, height, arraySize, 1, CPFlags.None);
                        imagePainter.Load(result);
                        break;

                    case TextureDimension.Texture3D:
                        result = device.TextureLoader.Initialize3D(format, width, height, depth, 1, CPFlags.None);
                        imagePainter.Load(result);
                        break;

                    case TextureDimension.TextureCube:
                        result = device.TextureLoader.InitializeCube(format, width, height, cubeCount, 1, CPFlags.None);
                        imagePainter.Load(result);
                        break;
                }
                result?.Dispose();

                Close();
            }
        }
    }
}