namespace HexaEngine.Editor
{
    using Hexa.NET.ImGui;
    using HexaEngine.Graphics.Renderers;
    using System.Text;

    public struct LayoutConfig : IEquatable<LayoutConfig>
    {
        public string Path;
        public string Name;

        public LayoutConfig(string path, string name)
        {
            Path = path;
            Name = name;
        }

        public LayoutConfig(string path)
        {
            Path = path;
            Name = System.IO.Path.GetFileNameWithoutExtension(path);
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is LayoutConfig config && Equals(config);
        }

        public readonly bool Equals(LayoutConfig other)
        {
            return Path == other.Path;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Path);
        }

        public static bool operator ==(LayoutConfig left, LayoutConfig right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LayoutConfig left, LayoutConfig right)
        {
            return !(left == right);
        }
    }

    public static class LayoutManager
    {
        private static readonly string basePath = Path.Combine(EditorConfig.BasePath, "Layouts");
        private static readonly string defaultPath = Path.Combine(basePath, "default.ini");
        private static readonly List<LayoutConfig> layouts = [];
        private static bool changed = false;
        private static bool initialized = false;

        static LayoutManager()
        {
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }
            layouts.Add(new LayoutConfig(defaultPath, "Default")); // default is a special path.
            foreach (var file in Directory.GetFiles(basePath, "*.ini"))
            {
                if (file.EndsWith("default.ini"))
                {
                    continue;
                }
                layouts.Add(new LayoutConfig(file));
            }
        }

        public static IReadOnlyList<LayoutConfig> Layouts => layouts;

        public static string SelectedLayout
        {
            get => EditorConfig.Default.SelectedLayout ??= defaultPath;
            set
            {
                if (layouts.Contains(new LayoutConfig() { Path = value }))
                {
                    SetLayout(value);
                }
            }
        }

        public static string BasePath => basePath;

        internal static unsafe bool Init()
        {
            var io = ImGui.GetIO();
            io.WantSaveIniSettings = false;
            initialized = true;
            var layout = SelectedLayout!;
            if (File.Exists(layout))
            {
                ImGuiP.ClearIniSettings();
                ImGui.LoadIniSettingsFromDisk(layout);
                return true;
            }

            return false;
        }

        internal static void NewFrame()
        {
            if (!initialized)
            {
                return;
            }
            if (changed)
            {
                var layout = SelectedLayout!;
                ImGui.LoadIniSettingsFromDisk(layout);
                //SetIniString(layout);
                changed = false;
            }

            var io = ImGui.GetIO();
            if (io.WantSaveIniSettings)
            {
                ImGui.SaveIniSettingsToDisk(SelectedLayout!);
                io.WantSaveIniSettings = false;
            }
        }

        private static unsafe void SetLayout(string value)
        {
            EditorConfig.Default.SelectedLayout = value;
            changed = true;
        }

        private static unsafe void SetIniString(string value)
        {
            var byteCount = Encoding.UTF8.GetByteCount(value);
            var str = (byte*)ImGui.MemAlloc((nuint)(byteCount + 1));
            fixed (char* pValue = value)
            {
                Encoding.UTF8.GetBytes(pValue, value.Length, str, byteCount);
            }
            str[byteCount] = 0;

            var io = ImGui.GetIO();
            io.IniFilename = str;
        }

        public static void ResetLayout()
        {
            ImGuiManager.ResetLayout();
        }

        public static void CreateNewLayout(string name)
        {
            string path = Path.Combine(basePath, $"{name}.ini");
            layouts.Add(new(path, name));
            SetIniString(path);
        }
    }
}