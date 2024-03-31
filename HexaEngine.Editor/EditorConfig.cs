namespace HexaEngine.Editor
{
    using HexaEngine.Core.Windows;
    using Newtonsoft.Json;

    public class EditorConfig
    {
        private const string configFile = "editorConfig.json";
        private static readonly string configPath = Path.Combine(DetermineBasePath(), configFile);

        public static readonly string BasePath = DetermineBasePath();

        public bool SetupDone { get; set; }

        public string ProjectsFolder { get; set; }

        public int Width { get; set; } = 1280;

        public int Height { get; set; } = 720;

        public int X { get; set; } = SdlWindow.WindowPosCentered;

        public int Y { get; set; } = SdlWindow.WindowPosCentered;

        public WindowState State { get; set; } = WindowState.Normal;

        public static EditorConfig Default { get; } = Load();

        public void Save()
        {
            File.WriteAllText(configPath, JsonConvert.SerializeObject(this));
        }

        public static EditorConfig Load()
        {
            EditorConfig config;
            if (File.Exists(configPath))
            {
                config = JsonConvert.DeserializeObject<EditorConfig>(File.ReadAllText(configPath));
            }
            else
            {
                config = new();
            }
            config.Save();
            return config;
        }

        private static string DetermineBasePath()
        {
            string projectsPath;
            if (OperatingSystem.IsWindows())
            {
                projectsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HexaEngine");
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                projectsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "HexaEngine");
            }
            else
            {
                throw new PlatformNotSupportedException("HexaEngine currently supports only Windows, Linux, and macOS platforms.");
            }

            Directory.CreateDirectory(projectsPath);
            return projectsPath;
        }
    }
}