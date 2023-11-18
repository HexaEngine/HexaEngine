namespace HexaEngine.Editor
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.ImagePainter;
    using HexaEngine.Editor.MaterialEditor;
    using HexaEngine.Editor.MeshEditor;
    using HexaEngine.Editor.PoseEditor;
    using HexaEngine.Editor.TextEditor;
    using HexaEngine.Editor.Widgets;
    using Hexa.NET.ImGui;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    public class EditorWindowCategory(string name) : IEquatable<EditorWindowCategory>
    {
        public string Name = name;
        public List<IEditorWindow> Windows = new();
        public ImGuiName UIName = new(name);

        public int Count => Windows.Count;

        public IEditorWindow this[int index]
        {
            get => Windows[index];
            set => Windows[index] = value;
        }

        public void Add(IEditorWindow window)
        {
            Windows.Add(window);
        }

        public void Remove(IEditorWindow window)
        {
            Windows.Remove(window);
        }

        public bool Contains(IEditorWindow window)
        {
            return Windows.Contains(window);
        }

        public void Clear()
        {
            Windows.Clear();
        }

        public void DrawMenu()
        {
            if (Name == string.Empty)
            {
                for (int i = 0; i < Windows.Count; i++)
                {
                    Windows[i].DrawMenu();
                }
            }
            else if (ImGui.BeginMenu(UIName.UniqueName))
            {
                for (int i = 0; i < Windows.Count; i++)
                {
                    Windows[i].DrawMenu();
                }
                ImGui.EndMenu();
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is EditorWindowCategory category && Equals(category);
        }

        public bool Equals(EditorWindowCategory? other)
        {
            return Name == other?.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }

        public static bool operator ==(EditorWindowCategory left, EditorWindowCategory right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EditorWindowCategory left, EditorWindowCategory right)
        {
            return !(left == right);
        }
    }

    public static class WindowManager
    {
        private static IGraphicsDevice? device;
        private static readonly List<IEditorWindow> windows = new();
        private static readonly List<EditorWindowCategory> categories = new();
        private static readonly EditorWindowCategory Default = new(string.Empty);

        static WindowManager()
        {
            categories.Add(Default);

            Register<PreferencesWidget>();
            Register<ProjectExplorer>();
            Register<PipelineWidget>();
            Register<AssetExplorer2>();
            Register<GitWidget>();
            Register<LayoutWidget>();
            Register<PropertiesWidget>();

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
            Register<TextEditorWindow>();
            Register<ImagePainterWindow>();
            Register<WeatherWidget>();
            Register<RenderGraphWidget>();
            Register<RendererWidget>();
            Register<MemoryWidget>();
        }

        public static IReadOnlyList<IEditorWindow> Windows => windows;

        public static IReadOnlyList<EditorWindowCategory> Categories => categories;

        public static EditorWindowCategory? GetCategory(string categoryName)
        {
            for (int i = 0; i < categories.Count; i++)
            {
                var category = categories[i];
                if (category.Name == categoryName)
                {
                    return category;
                }
            }
            return null;
        }

        public static bool TryGetCategory(string categoryName, [NotNullWhen(true)] out EditorWindowCategory? category)
        {
            category = GetCategory(categoryName);
            return category is not null;
        }

        public static EditorWindowCategory GetOrAddCategory(string categoryName)
        {
            return GetCategory(categoryName) ?? AddCategory(categoryName);
        }

        public static EditorWindowCategory AddCategory(string categoryName)
        {
            EditorWindowCategory category = new(categoryName);
            categories.Add(category);
            return category;
        }

        public static bool HasCategory(string categoryName)
        {
            for (int i = 0; i < categories.Count; i++)
            {
                if (categories[i].Name == categoryName)
                {
                    return true;
                }
            }
            return false;
        }

        public static T? GetWindow<T>() where T : class, IEditorWindow
        {
            for (int i = 0; i < windows.Count; i++)
            {
                var window = windows[i];
                if (window is T t)
                {
                    return t;
                }
            }
            return null;
        }

        public static IEditorWindow? GetWindow(string name)
        {
            for (int i = 0; i < windows.Count; i++)
            {
                var window = windows[i];
                if (window.Name == name)
                {
                    return window;
                }
            }
            return null;
        }

        public static bool TryGetWindow<T>([NotNullWhen(true)] out T? editorWindow) where T : class, IEditorWindow
        {
            editorWindow = GetWindow<T>();
            return editorWindow != null;
        }

        public static bool TryGetWindow(string name, [NotNullWhen(true)] out IEditorWindow? editorWindow)
        {
            editorWindow = GetWindow(name);
            return editorWindow != null;
        }

        public static bool HasWindow<T>() where T : class, IEditorWindow
        {
            for (int i = 0; i < windows.Count; i++)
            {
                var window = windows[i];
                if (window is T)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool HasWindow(string name)
        {
            for (int i = 0; i < windows.Count; i++)
            {
                var window = windows[i];
                if (window.Name == name)
                {
                    return true;
                }
            }
            return false;
        }

        public static void Register<T>() where T : IEditorWindow, new()
        {
            IEditorWindow window = new T();
            window.Shown += Shown;
            window.Closed += Closed;
            windows.Add(window);
            var type = typeof(T);
            var categoryAttr = type.GetCustomAttribute<EditorWindowCategoryAttribute>();
            if (categoryAttr == null)
            {
                Default.Add(window);
                return;
            }

            var category = GetOrAddCategory(categoryAttr.Category);
            category.Add(window);
        }

        public static void Register(IEditorWindow window)
        {
            window.Shown += Shown;
            window.Closed += Closed;
            windows.Add(window);
            var type = window.GetType();
            var categoryAttr = type.GetCustomAttribute<EditorWindowCategoryAttribute>();
            if (categoryAttr == null)
            {
                Default.Add(window);
                return;
            }

            var category = GetOrAddCategory(categoryAttr.Category);
            category.Add(window);
        }

        public static void Unregister<T>() where T : IEditorWindow, new()
        {
            IEditorWindow? window = windows.FirstOrDefault(x => x is T);
            if (window != null)
            {
                for (int i = 0; i < categories.Count; i++)
                {
                    var category = categories[i];
                    category.Remove(window);
                    if (category.Count == 0)
                    {
                        categories.RemoveAt(i);
                        i--;
                    }
                }

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
            for (int i = 0; i < categories.Count; i++)
            {
                var category = categories[i];
                category.Remove(window);
                if (category.Count == 0)
                {
                    categories.RemoveAt(i);
                    i--;
                }
            }

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
            for (int i = 0; i < categories.Count; i++)
            {
                categories[i].DrawMenu();
            }
        }

        public static void Dispose()
        {
            for (int i = 0; i < categories.Count; i++)
            {
                categories[i].Clear();
            }
            categories.Clear();

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