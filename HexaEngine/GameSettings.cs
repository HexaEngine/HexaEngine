namespace HexaEngine
{
    public class GameSettings
    {
        public string Title { get; set; } = string.Empty;

        public int Width { get; set; } = 1280;

        public int Height { get; set; } = 720;

        public int RenderWidth { get; set; } = 1280;

        public int RenderHeight { get; set; } = 720;

        public bool VSync { get; set; } = true;

        public bool Fullscreen { get; set; } = false;

        public bool BorderlessFullscreen { get; set; } = false;

        public bool FPSLimit { get; set; } = false;

        public int FPSTarget { get; set; } = 60;

        public bool CursorCapture { get; set; } = true;

        public bool CursorVisible { get; set; } = false;
    }
}