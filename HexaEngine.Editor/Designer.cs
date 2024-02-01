namespace HexaEngine.Editor
{
    using HexaEngine.Components;
    using HexaEngine.Components.Renderer;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Editors;
    using HexaEngine.Editor.Factories;
    using HexaEngine.Editor.Icons;
    using HexaEngine.Editor.Properties;
    using HexaEngine.Editor.Tools;
    using HexaEngine.Editor.Widgets;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public static class Designer
    {
        private static Task? task;

        public static void Init(IGraphicsDevice device)
        {
            MainMenuBar.Init(device);
            IconManager.Init(device);
            WindowManager.Init(device);
            PopupManager.Show<OpenProjectWindow>();

            ObjectEditorFactory.AddFactory<GameObjectReferenceEditorFactory>();
            ObjectEditorFactory.AddFactory(new DrawLayerPropertyEditorFactory());
            ObjectEditorFactory.AddFactory(new BoolPropertyEditorFactory());
            ObjectEditorFactory.AddFactory(new EnumPropertyEditorFactory());
            ObjectEditorFactory.AddFactory(new FloatPropertyEditorFactory());
            ObjectEditorFactory.AddFactory(new StringPropertyEditorFactory());
            ObjectEditorFactory.AddFactory(new TypePropertyFactory());
            ObjectEditorFactory.AddFactory(new Vector2PropertyEditorFactory());
            ObjectEditorFactory.AddFactory(new Vector3PropertyEditorFactory());
            ObjectEditorFactory.AddFactory(new Vector4PropertyEditorFactory());
            ObjectEditorFactory.AddFactory<QuaternionPropertyEditorFactory>();
            ObjectEditorFactory.AddFactory(new SubTypePropertyFactory());

            PropertyObjectEditorRegistry.RegisterEditor<GameObjectEditor>();

            ObjectEditorFactory.RegisterEditor(typeof(ScriptBehaviour), new ScriptBehaviourEditor());
            ObjectEditorFactory.RegisterEditor(typeof(TerrainRendererComponent), new TerrainEditor());
        }

        public static void Dispose()
        {
            WindowManager.Dispose();
            IconManager.Dispose();
            PopupManager.Dispose();
        }

        public static void Draw(IGraphicsContext context)
        {
            MainMenuBar.Draw();
            WindowManager.Draw(context);
            ImGuiConsole.Draw();
            MessageBoxes.Draw();
            PopupManager.Draw();
        }

        public static void OpenFile(string? path)
        {
            if ((task == null || task.IsCompleted) && path != null)
            {
                task = ToolManager.Open(path);
            }
        }

        public static void OpenFileWith(string? path)
        {
            if (path == null)
                return;
        }

        public static void OpenDirectory(string? path)
        {
            if (path == null)
                return;

            if (OperatingSystem.IsWindows())
            {
                Process.Start("explorer.exe", path);
            }
            else if (OperatingSystem.IsLinux())
            {
                Process.Start("xdg-open", path);
            }
            else if (OperatingSystem.IsMacOS())
            {
                Process.Start("open", path);
            }
        }
    }
}