namespace HexaEngine.Editor
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Materials;
    using HexaEngine.Editor.Widgets;

    public static class WidgetManager
    {
        private static IGraphicsDevice? device;
        private static readonly List<IImGuiWindow> widgets = new();

        static WidgetManager()
        {
            Register<PreferencesWidget>();
            Register<ProjectExplorer>();
            Register<PipelineWidget>();
            Register<AssetExplorer>();
            Register<LayoutWidget>();
            Register<PropertiesWidget>();
            Register<MaterialsWidget>();
            Register<MeshesWidget>();
            Register<MixerWidget>();
            Register<OpenProjectWindow>();
            Register<PublishProjectWindow>();
            Register<SceneVariablesWindow>();
            Register<DebugWindow>();
            Register<ProfilerWindow>();
            Register<MaterialEditor>();
        }

        public static bool Register<T>() where T : IImGuiWindow, new()
        {
            if (device == null)
            {
                IImGuiWindow widget = new T();
                widgets.Add(widget);
                return false;
            }
            else
            {
                IImGuiWindow widget = new T();
                widget.Init(device);
                widgets.Add(widget);
                return true;
            }
        }

        public static void Unregister<T>() where T : IImGuiWindow, new()
        {
            IImGuiWindow? window = widgets.FirstOrDefault(x => x is T);
            if (window != null)
            {
                if (device != null)
                {
                    window.Dispose();
                }

                widgets.Remove(window);
            }
        }

        public static bool Register(IImGuiWindow widget)
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

        public static unsafe void DrawMenu()
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