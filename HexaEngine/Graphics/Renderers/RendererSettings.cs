namespace HexaEngine.Graphics.Renderers
{
    public class RendererSettings
    {
        public float RenderResolution { get; set; }

        public int RendererWidth { get; set; }

        public int RendererHeight { get; set; }

        public bool VSync { get; set; }

        public bool LimitFPS { get; set; }

        public int FPSTarget { get; set; }
    }
}