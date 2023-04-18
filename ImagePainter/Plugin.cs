namespace ImagePainter
{
    using HexaEngine.Editor;
    using HexaEngine.Editor.Plugins;

    public class Plugin : IPlugin
    {
        public string Name => "Image Painter Plugin";

        public string Version => "1.0.0.0";

        public string Description => "Allows to edit images";

        public void OnEnable()
        {
            WidgetManager.Register<ImagePainterWindow>();
        }

        public void OnDisable()
        {
            WidgetManager.Unregister<ImagePainterWindow>();
        }

        public void OnInitialize()
        {
        }

        public void OnUninitialize()
        {
        }
    }
}