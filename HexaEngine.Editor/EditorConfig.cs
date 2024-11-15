namespace HexaEngine.Editor
{
    using HexaEngine.Core.Utilities;
    using HexaEngine.Core.Windows;
    using Newtonsoft.Json;

    public enum ExternalTextEditorType
    {
        [EnumName("Visual Studio")]
        VisualStudio,

        [EnumName("VS Code")]
        VSCode,

        Custom,
    }

    public class ExternalTextEditor
    {
        public ExternalTextEditor(string name, string program, string commandLine)
        {
            Name = name;
            ProgramPath = program;
            CommandLine = commandLine;
        }

        public ExternalTextEditor()
        {
            Name = string.Empty;
            ProgramPath = string.Empty;
            CommandLine = string.Empty;
        }

        public string Name { get; set; }

        public string ProgramPath { get; set; }

        public string CommandLine { get; set; }
    }

    public class EditorConfig
    {
        private const string configFile = "editorConfig.json";
        private static readonly string configPath = Path.Combine(DetermineAppBasePath(), configFile);

        public static readonly string BasePath = DetermineAppBasePath();

        public bool SetupDone { get; set; }

        public string ProjectsFolder { get; set; } = null!;

        public int Width { get; set; } = 1280;

        public int Height { get; set; } = 720;

        public int X { get; set; } = (int)SdlWindow.WindowPosCentered;

        public int Y { get; set; } = (int)SdlWindow.WindowPosCentered;

        public float MouseSensitivity { get; set; } = 1;

        public WindowState State { get; set; } = WindowState.Normal;

        public ExternalTextEditorType ExternalTextEditorType { get; set; }

        public int SelectedExternalTextEditor { get; set; }

        public List<ExternalTextEditor> ExternalTextEditors { get; set; } = [];

        public static EditorConfig Default { get; } = Load();

        public string? SelectedLayout { get; set; }

        public void Save()
        {
            File.WriteAllText(configPath, JsonConvert.SerializeObject(this));
        }

        public static EditorConfig Load()
        {
            EditorConfig config;
            if (File.Exists(configPath))
            {
                config = JsonConvert.DeserializeObject<EditorConfig>(File.ReadAllText(configPath)) ?? new();
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