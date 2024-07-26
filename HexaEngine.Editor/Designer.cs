//#define BypassLauncher

namespace HexaEngine.Editor
{
    using HexaEngine.Components.Renderer;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.ColorGradingEditor;
    using HexaEngine.Editor.Editors;
    using HexaEngine.Editor.Factories;
    using HexaEngine.Editor.Icons;
    using HexaEngine.Editor.Projects;
    using HexaEngine.Editor.Properties;
    using HexaEngine.Editor.TerrainEditor;
    using HexaEngine.Editor.Tools;
    using HexaEngine.Editor.Widgets;
    using HexaEngine.PostFx.BuildIn;
    using HexaEngine.Profiling;
    using HexaEngine.Scripts;
    using HexaEngine.Volumes;
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
#if !BypassLauncher
            PopupManager.Show<LauncherWindow>();
#endif
            if (!EditorConfig.Default.SetupDone)
            {
                PopupManager.Show<SetupWindow>();
            }

            ObjectEditorFactory.AddFactory<GameObjectReferenceEditorFactory>();
            ObjectEditorFactory.AddFactory<MaterialMappingPropertyEditorFactory>();
            ObjectEditorFactory.AddFactory<AssetRefPropertyEditorFactory>();
            ObjectEditorFactory.AddFactory(new DrawLayerPropertyEditorFactory());
            ObjectEditorFactory.AddFactory(new BoolPropertyEditorFactory());
            ObjectEditorFactory.AddFactory(new EnumPropertyEditorFactory());
            ObjectEditorFactory.AddFactory(new FloatPropertyEditorFactory());
            ObjectEditorFactory.AddFactory(new IntPropertyEditorFactory());
            ObjectEditorFactory.AddFactory(new UIntPropertyEditorFactory());
            ObjectEditorFactory.AddFactory(new StringPropertyEditorFactory());
            ObjectEditorFactory.AddFactory(new TypePropertyFactory());
            ObjectEditorFactory.AddFactory(new Vector2PropertyEditorFactory());
            ObjectEditorFactory.AddFactory(new Vector3PropertyEditorFactory());
            ObjectEditorFactory.AddFactory(new Vector4PropertyEditorFactory());
            ObjectEditorFactory.AddFactory<QuaternionPropertyEditorFactory>();
            ObjectEditorFactory.AddFactory(new SubTypePropertyFactory());

            PropertyObjectEditorRegistry.RegisterEditor<GameObjectEditor>();
            PropertyObjectEditorRegistry.RegisterEditor<AssetFileEditor>();

            ObjectEditorFactory.RegisterEditor(typeof(ScriptComponent), new ScriptBehaviourEditor());
            ObjectEditorFactory.RegisterEditor(typeof(TerrainRendererComponent), new TerrainObjectEditor());
            ObjectEditorFactory.RegisterEditor(typeof(Volume), new VolumeObjectEditor());
            PostProcessingEditorFactory.RegisterEditor<ColorGrading, ColorGradingObjectEditor>();
        }

        public static void Dispose()
        {
            ProjectManager.Unload();
            WindowManager.Dispose();
            IconManager.Dispose();
            PopupManager.Dispose();
        }

        [Profile]
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
            {
                return;
            }
        }

        public static void OpenLink(string? path)
        {
            OpenDirectory(path);
        }

        public static void OpenDirectory(string? path)
        {
            if (path == null)
            {
                return;
            }

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