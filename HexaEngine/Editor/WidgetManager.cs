namespace HexaEngine.Editor
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Widgets;

    public static class WidgetManager
    {
        private static IGraphicsDevice? device;
        private static readonly List<ImGuiWindow> widgets = new();

        static WidgetManager()
        {
            Register<PreferencesWidget>();
            Register<AssetExplorer>();
            Register<LayoutWidget>();
            Register<PropertiesWidget>();
            Register<MaterialsWidget>();
            Register<MeshesWidget>();
            // Register<PreviewWidget>();
            Register<PrefilterWidget>();
            Register<IrradianceWidget>();
            Register<DFGLUTWidget>();
        }

        public static bool Register<T>() where T : ImGuiWindow, new()
        {
            if (device == null)
            {
                ImGuiWindow widget = new T();
                widgets.Add(widget);
                return false;
            }
            else
            {
                ImGuiWindow widget = new T();
                widget.Init(device);
                widgets.Add(widget);
                return true;
            }
        }

        public static bool Register(ImGuiWindow widget)
        {
            if (device == null)
            {
                widgets.Add(widget);
                return false;
            }
            else
            {
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
                widgets[i].DrawWindow(context);
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