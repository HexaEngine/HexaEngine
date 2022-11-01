namespace HexaEngine.Editor
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Widgets;

    public static class WidgetManager
    {
        private static IGraphicsDevice? device;
        private static readonly List<Widget> widgets = new();

        static WidgetManager()
        {
            Register<AssetExplorer>();
            Register<LayoutWidget>();
            Register<PropertiesWidget>();
            Register<MaterialsWidget>();
            Register<MeshesWidget>();
            Register<PreviewWidget>();
            Register<PrefilterWidget>();
            Register<IrradianceWidget>();
        }

        public static bool Register<T>() where T : Widget, new()
        {
            if (device == null)
            {
                Widget widget = new T();
                widgets.Add(widget);
                return false;
            }
            else
            {
                Widget widget = new T();
                widget.Init(device);
                widgets.Add(widget);
                return true;
            }
        }

        public static void Init(IGraphicsDevice device)
        {
            WidgetManager.device = device;
            for (int i = 0; i < widgets.Count; i++)
            {
                var widget = widgets[i];
                widget.Init(device);
            }
        }

        public static void Draw(IGraphicsContext context)
        {
            for (int i = 0; i < widgets.Count; i++)
            {
                widgets[i].Draw(context);
            }
        }

        public static void DrawMenu()
        {
            for (int i = 0; i < widgets.Count; i++)
            {
                widgets[i].DrawMenu();
            }
        }

        public static void Dispose()
        {
            for (int i = 0; i < widgets.Count; i++)
            {
                widgets[i].Dispose();
            }
            widgets.Clear();
        }
    }
}