namespace HexaEngine.Editor
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.MeshEditor;
    using HexaEngine.Editor.ImagePainter;
    using HexaEngine.Editor.Widgets;
    using HexaEngine.Editor.MaterialEditor;
    using HexaEngine.Editor.PoseEditor;

    public static class WindowManager
    {
        private static IGraphicsDevice? device;
        private static readonly List<IEditorWindow> windows = new();

        static WindowManager()
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
            Register<PublishProjectWindow>();
            Register<SceneVariablesWindow>();
            Register<DebugWindow>();
            Register<ProfilerWindow>();
            Register<MeshEditorWindow>();
            Register<PoseEditorWindow>();
            Register<MaterialEditorWindow>();
            Register<PostProcessWindow>();
            Register<InputWindow>();
            Register<ImagePainterWindow>();
            Register<WeatherWidget>();
            Register<RenderGraphWidget>();
            Register<MemoryWidget>();
        }

        public static IReadOnlyList<IEditorWindow> Windows => windows;

        public static void Register<T>() where T : IEditorWindow, new()
        {
            IEditorWindow window = new T();
            window.Shown += Shown;
            window.Closed += Closed;
            windows.Add(window);
        }

        public static void Register(IEditorWindow window)
        {
            window.Shown += Shown;
            window.Closed += Closed;
            windows.Add(window);
        }

        public static void Unregister<T>() where T : IEditorWindow, new()
        {
            IEditorWindow? window = windows.FirstOrDefault(x => x is T);
            if (window != null)
            {
                window.Shown -= Shown;
                window.Closed -= Closed;
                if (window.Initialized)
                {
                    window.Dispose();
                }

                windows.Remove(window);
            }
        }

        public static void Unregister(IEditorWindow window)
        {
            window.Shown -= Shown;
            window.Closed -= Closed;
            if (window.Initialized)
            {
                window.Dispose();
            }

            windows.Remove(window);
        }

        private static void Closed(IEditorWindow window)
        {
            if (window.Initialized)
                window.Dispose();
        }

        private static void Shown(IEditorWindow window)
        {
            if (!window.Initialized && device != null)
                window.Init(device);
        }

        public static void Init(IGraphicsDevice device)
        {
            WindowManager.device = device;
            for (int i = 0; i < windows.Count; i++)
            {
                var window = windows[i];
                if (window.IsShown)
                    window.Init(device);
            }
        }

        public static void Draw(IGraphicsContext context)
        {
            for (int i = 0; i < windows.Count; i++)
            {
                windows[i].DrawWindow(context);
            }
        }

        public static unsafe void DrawMenu()
        {
            for (int i = 0; i < windows.Count; i++)
            {
                windows[i].DrawMenu();
            }
        }

        public static void Dispose()
        {
            for (int i = 0; i < windows.Count; i++)
            {
                var window = windows[i];
                window.Shown -= Shown;
                window.Closed -= Closed;
                if (window.Initialized)
                    window.Dispose();
            }
            windows.Clear();
        }
    }
}