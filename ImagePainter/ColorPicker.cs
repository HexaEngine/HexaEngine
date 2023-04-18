namespace ImagePainter
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor;
    using ImGuiNET;
    using System.Numerics;

    public class ColorPicker : ImGuiWindow
    {
        private readonly Vector4[] history = new Vector4[10];

        private ColorMode mode;
        private Vector4 colorPrimary = Vector4.One;
        private Vector4 colorSecondary = new(0, 0, 0, 1);
        private Vector4 internalColor = Vector4.One;
        private bool active;
        private bool modified;

        protected override string Name => "Color-picker";

        public Vector4 Color
        {
            get
            {
                if (mode == ColorMode.Primary)
                    return colorPrimary;
                if (mode == ColorMode.Secondary)
                    return colorSecondary;
                return default;
            }

            set
            {
                if (mode == ColorMode.Primary)
                    colorPrimary = value;
                if (mode == ColorMode.Secondary)
                    colorSecondary = value;
                for (int i = history.Length - 1; i != 0; i--)
                {
                    history[i] = history[i - 1];
                }
                history[0] = value;
                internalColor = value;
            }
        }

        public void SetColorFromHistory(int index)
        {
            if (mode == ColorMode.Primary)
                colorPrimary = history[index];
            if (mode == ColorMode.Secondary)
                colorSecondary = history[index];
            internalColor = history[index];
        }

        public void SetMode(ColorMode mode)
        {
            this.mode = mode;
            if (mode == ColorMode.Primary)
            {
                internalColor = colorPrimary;
            }
            if (mode == ColorMode.Secondary)
            {
                internalColor = colorSecondary;
            }
        }

        public override void DrawContent(IGraphicsContext context)
        {
            if (ImGui.ColorPicker4("Color##TexturePainter", ref internalColor, ImGuiColorEditFlags.DefaultOptions))
            {
                modified = true;
            }
            active = ImGui.IsItemActive();

            if (modified && !active)
            {
                modified = false;
                Color = internalColor;
            }

            if (ImGui.ColorButton("Primary", colorPrimary, ImGuiColorEditFlags.None, new Vector2(32, 32)))
            {
                SetMode(ColorMode.Primary);
            }
            ImGui.SameLine();
            if (ImGui.ColorButton("Secondary", colorSecondary, ImGuiColorEditFlags.None, new Vector2(32, 32)))
            {
                SetMode(ColorMode.Secondary);
            }
            ImGui.SameLine();
            ImGui.BeginChild("##HistoryBox");
            for (int i = 0; i < history.Length; i++)
            {
                if (ImGui.ColorButton($"##Color{i}", history[i]))
                {
                    SetColorFromHistory(i);
                }
                if (i != 4 || i == history.Length - 1)
                {
                    ImGui.SameLine();
                }
            }
            ImGui.EndChild();
        }
    }
}