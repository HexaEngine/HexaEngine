namespace HexaEngine.Editor
{
    using HexaEngine.Core;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using System.Xml.Serialization;

    public enum Theme
    {
        Light,
        Dark,
        Custom,
    }

    public static class ThemeManager
    {
        private static readonly ConfigKey config = Config.Global.GetOrCreateKey("Editor");
        private static Theme theme;
        private static string themeName;
        private static readonly ThemeDescription description = new();

        static ThemeManager()
        {
            theme = config.GetOrAddValue(nameof(Theme), Theme.Dark);
            themeName = theme.ToString();
        }

        public static Theme Theme
        {
            get => theme;
            set
            {
                theme = value;
                themeName = value.ToString();
                config.SetValue(nameof(Theme), theme);
                Config.Global.Save();
                ThemeChanged?.Invoke(theme);
            }
        }

        public static string ThemeName => themeName;

        public static ThemeDescription Description => description;

        public static bool IsDark => theme == Theme.Dark;

        public static bool IsLight => theme == Theme.Light;

        public static bool IsCustom => theme == Theme.Custom;

        public static event Action<Theme>? ThemeChanged;

        public static bool IsTheme(Theme theme)
        {
            return ThemeManager.theme == theme;
        }
    }

    public enum ThemeColor
    {
        Text,
        TextDisabled,
        WindowBg,
        ChildBg,
        PopupBg,
        Border,
        BorderShadow,
        FrameBg,
        FrameBgHovered,
        FrameBgActive,
        TitleBg,
        TitleBgActive,
        TitleBgCollapsed,
        MenuBarBg,
        ScrollbarBg,
        ScrollbarGrab,
        ScrollbarGrabHovered,
        ScrollbarGrabActive,
        CheckMark,
        SliderGrab,
        SliderGrabActive,
        Button,
        ButtonHovered,
        ButtonActive,
        Header,
        HeaderHovered,
        HeaderActive,
        Separator,
        SeparatorHovered,
        SeparatorActive,
        ResizeGrip,
        ResizeGripHovered,
        ResizeGripActive,
        Tab,
        TabHovered,
        TabActive,
        TabUnfocused,
        TabUnfocusedActive,
        DockingPreview,
        DockingEmptyBg,
        PlotLines,
        PlotLinesHovered,
        PlotHistogram,
        PlotHistogramHovered,
        TableHeaderBg,
        TableBorderStrong,
        TableBorderLight,
        TableRowBg,
        TableRowBgAlt,
        TextSelectedBg,
        DragDropTarget,
        NavHighlight,
        NavWindowingHighlight,
        NavWindowingDimBg,
        ModalWindowDimBg,
        Count
    }

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
                    return colorEntry;
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

    [XmlRoot("Color")]
    public class ColorEntry
    {
        [XmlAttribute("Name")]
        public ThemeColor Name { get; set; }

        [XmlElement("Value")]
        public ColorRGBA Value { get; set; }
    }

    public struct Padding
    {
    }

    [XmlRoot("Color")]
    public struct ColorRGBA
    {
        [XmlAttribute("r")]
        public float R;

        [XmlAttribute("g")]
        public float G;

        [XmlAttribute("b")]
        public float B;

        [XmlAttribute("a")]
        public float A;

        public ColorRGBA(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public ColorRGBA(Vector4 color)
        {
            R = color.X;
            G = color.Y;
            B = color.Z;
            A = color.W;
        }

        public static implicit operator Vector4(ColorRGBA c)
        {
            return new(c.R, c.G, c.B, c.A);
        }
    }
}