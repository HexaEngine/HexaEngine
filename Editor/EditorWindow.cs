namespace Editor
{
    using HexaEngine.Core;
    using HexaEngine.Core.Windows;
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.Windows;
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

    public class EditorWindow : Window
    {
        private bool firstTime = true;
        private readonly EditorConfig config = EditorConfig.Load();

        protected override void OnShown(ShownEventArgs args)
        {
            if (firstTime)
            {
                X = config.X;
                Y = config.Y;
                Width = config.Width;
                Height = config.Height;
                State = config.State;
                firstTime = false;
            }
            base.OnShown(args);
        }

        protected override void OnClose(CloseEventArgs args)
        {
            config.X = X;
            config.Y = Y;
            config.Width = Width;
            config.Height = Height;
            config.State = State;
            config.Save();
            base.OnClose(args);
        }
    }
}