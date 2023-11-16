namespace Editor
{
    using HexaEngine.Core.Windows;
    using Newtonsoft.Json;

    public class EditorConfig
    {
        private const string editorPath = "editorConfig.json";

        public int Width { get; set; } = 1280;

        public int Height { get; set; } = 720;

        public int X { get; set; } = SdlWindow.WindowPosCentered;

        public int Y { get; set; } = SdlWindow.WindowPosCentered;

        public WindowState State { get; set; } = WindowState.Normal;

        public void Save()
        {
            File.WriteAllText(editorPath, JsonConvert.SerializeObject(this));
        }

        public static EditorConfig Load()
        {
            EditorConfig config;
            if (File.Exists(editorPath))
            {
                config = JsonConvert.DeserializeObject<EditorConfig>(File.ReadAllText(editorPath));
            }
            else
            {
                config = new();
            }
            config.Save();
            return config;
        }
    }
}