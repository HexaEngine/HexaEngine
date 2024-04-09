namespace HexaEngine.Editor
{
    using HexaEngine.Core.Windows;
    using Newtonsoft.Json;

    public class EditorConfig
    {
        private const string configFile = "editorConfig.json";
        private static readonly string configPath = Path.Combine(DetermineAppBasePath(), configFile);

        public static readonly string BasePath = DetermineAppBasePath();

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

        private static string DetermineAppBasePath()
        {
            string projectsPath;
            if (OperatingSystem.IsWindows())
            {
                projectsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HexaEngine");
            }
            else if (OperatingSystem.IsLinux())
            {
                projectsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HexaEngine");
            }
            else if (OperatingSystem.IsMacOS())
            {
                projectsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HexaEngine");
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