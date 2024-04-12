namespace HexaEngine.Editor.Themes
{
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using System.Xml.Serialization;

    [XmlRoot("Theme")]
    public class ThemeDescription
    {
        [XmlArray("Colors")]
        [XmlArrayItem("Color")]
        public List<ColorEntry> Colors = new();

        public float Alpha { get; set; }

        public float DisabledAlpha { get; set; }

        public Vector2 WindowPadding { get; set; }

        public float WindowRounding { get; set; }

        public float WindowBorderSize { get; set; }

        public Vector2 WindowMinSize { get; set; }

        public Vector2 WindowTitleAlign { get; set; }

        public float ChildRounding { get; set; }

        public float ChildBorderSize { get; set; }

        public float PopupRounding { get; set; }

        public float PopupBorderSize { get; set; }

        public Vector2 FramePadding { get; set; }

        public float FrameRounding { get; set; }

        public float FrameBorderSize { get; set; }

        public Vector2 ItemSpacing { get; set; }

        public Vector2 ItemInnerSpacing { get; set; }

        public Vector2 CellPadding { get; set; }

        public Vector2 TouchExtraPadding { get; set; }

        public float IndentSpacing { get; set; }

        public float ColumnsMinSpacing { get; set; }

        public float ScrollbarSize { get; set; }

        public float ScrollbarRounding { get; set; }

        public float GrabMinSize { get; set; }

        public float GrabRounding { get; set; }

        public float LogSliderDeadzone { get; set; }

        public float TabRounding { get; set; }

        public float TabBorderSize { get; set; }

        public float TabMinWidthForCloseButton { get; set; }

        public Vector2 ButtonTextAlign { get; set; }

        public Vector2 SelectableTextAlign { get; set; }

        public float SeparatorTextBorderSize { get; set; }

        public Vector2 SeparatorTextAlign { get; set; }

        public Vector2 SeparatorTextPadding { get; set; }

        public Vector2 DisplayWindowPadding { get; set; }

        public Vector2 DisplaySafeAreaPadding { get; set; }

        public float DockingSeparatorSize { get; set; }

        public float MouseCursorScale { get; set; }

        public bool AntiAliasedLines { get; set; }

        public bool AntiAliasedLinesUseTex { get; set; }

        public bool AntiAliasedFill { get; set; }

        public float CurveTessellationTol { get; set; }

        public float CircleTessellationMaxError { get; set; }

        public float HoverStationaryDelay { get; set; }

        public float HoverDelayShort { get; set; }

        public float HoverDelayNormal { get; set; }

        public ColorRGBA this[ThemeColor index]
        {
            get => GetOrAdd(index).Value;
            set => GetOrAdd(index).Value = value;
        }

        public ColorEntry? GetColor(ThemeColor color)
        {
            for (int i = 0; i < Colors.Count; i++)
            {
                ColorEntry colorEntry = Colors[i];
                if (colorEntry.Name == color)
                {
                    return colorEntry;
                }
            }
            return null;
        }

        public bool TryGetColor(ThemeColor color, [NotNullWhen(true)] out ColorEntry? colorEntry)
        {
            colorEntry = GetColor(color);
            return colorEntry != null;
        }

        public ColorEntry GetOrAdd(ThemeColor color)
        {
            if (!TryGetColor(color, out var colorEntry))
            {
                colorEntry = new() { Name = color };
                Colors.Add(colorEntry);
            }
            return colorEntry;
        }
    }
}