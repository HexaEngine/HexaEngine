namespace HexaEngine.Editor.ImagePainter.Exporters
{
    using HexaEngine.Core.Graphics;
    using ImGuiNET;

    public abstract class BaseExporter
    {
        private static readonly Format[] formats = Enum.GetValues<Format>();
        private static readonly string[] formatNames = Enum.GetNames<Format>();
        protected Format format;
        private IScratchImage? image;
        private string? path;

        public IScratchImage? Image
        {
            get => image;
            set
            {
                image = value;
                if (value != null)
                {
                    format = value.Metadata.Format;
                }
            }
        }

        public string? Path
        {
            get => path;
            set => path = value;
        }

        public unsafe bool DrawContent(IGraphicsDevice device)
        {
            var index = Array.IndexOf(formats, format);
            if (ImGui.Combo("Format", ref index, formatNames, formatNames.Length))
            {
                format = formats[index];
            }

            float footerHeightToReserve = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();
            float width = ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X;

            if (ImGui.BeginChild(1, new(width, -footerHeightToReserve), false, ImGuiWindowFlags.HorizontalScrollbar))
            {
                DrawExporterSettings();
            }

            ImGui.EndChild();

            if (ImGui.Button("Cancel"))
            {
                return true;
            }
            ImGui.SameLine();
            if (ImGui.Button("Export"))
            {
                Export(device);
                return true;
            }

            return false;
        }

        protected abstract void DrawExporterSettings();

        protected abstract void Export(IGraphicsDevice device);
    }
}