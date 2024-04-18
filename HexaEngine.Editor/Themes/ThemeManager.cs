namespace HexaEngine.Editor.Themes
{
    using HexaEngine.Core;
    using HexaEngine.Core.Configuration;

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
                Config.SaveGlobal();
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
}