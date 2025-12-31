//#define BypassLauncher

namespace HexaEngine.Editor
{
    using Hexa.NET.ImGui;
    using HexaEngine.Components.Renderer;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.ColorGradingEditor;
    using HexaEngine.Editor.Editors;
    using HexaEngine.Editor.Factories;
    using HexaEngine.Editor.Icons;
    using HexaEngine.Editor.ImagePainter;
    using HexaEngine.Editor.MaterialEditor;
    using HexaEngine.Editor.Overlays;
    using HexaEngine.Editor.PoseEditor;
    using HexaEngine.Editor.Projects;
    using HexaEngine.Editor.Properties;
    using HexaEngine.Editor.TerrainEditor;
    using HexaEngine.Editor.TextEditor;
    using HexaEngine.Editor.Tools;
    using HexaEngine.Editor.Widgets;
    using HexaEngine.Editor.Widgets.AssetBrowser;
    using HexaEngine.Graphics.Overlays;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.PostFx.BuildIn;
    using HexaEngine.Profiling;
    using HexaEngine.Scripts;
    using HexaEngine.Volumes;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public static class Designer
    {
        private static Task? task;

        public unsafe static void Init(IGraphicsDevice device)
        {
            var io = ImGui.GetIO();
            io.IniFilename = null;
            MainMenuBar.Init(device);
            IconManager.Init();
            WindowManager.Init(device);
#if !BypassLauncher
            PopupManager.Show<LauncherWindow>();
#endif
            if (!EditorConfig.Default.SetupDone)
            {
                PopupManager.Show<SetupWindow>();
            }

            ImGuiConsole.IsDisplayed = true;

            WindowManager.Reset();

            WindowManager.Register<OutputWidget>();

            WindowManager.Register<PreferencesWidget>();
            WindowManager.Register<PipelineWidget>();
            WindowManager.Register<AssetBrowserWidget>();
            WindowManager.Register<GitWidget>();
            WindowManager.Register<HierarchyWidget>();
            WindowManager.Register<PropertiesWidget>();

            WindowManager.Register<MixerWidget>();
            WindowManager.Register<PublishProjectWindow>();
            WindowManager.Register<SceneVariablesWindow>();
            WindowManager.Register<DebugWindow>();
            WindowManager.Register<ProfilerWindow>();
            WindowManager.Register<DeepProfilerWindow>();
            WindowManager.Register<PoseEditorWindow>();
            WindowManager.Register<MaterialEditorWindow>();
            WindowManager.Register<PostProcessWindow>();
            WindowManager.Register<InputWindow>();
            WindowManager.Register<TextEditorWindow>();
            WindowManager.Register<ImagePainterWindow>();
            WindowManager.Register<WeatherWidget>();
            WindowManager.Register<RenderGraphWidget>();
            WindowManager.Register<RendererWidget>();
            WindowManager.Register<MemoryWidget>();
            WindowManager.Register<NativeMemoryWidget>();
            WindowManager.Register<InputManagerWindow>();
            WindowManager.Register<PackageManagerWidget>();
            WindowManager.Register<ErrorListWidget>();
            WindowManager.Register<CullingWidget>();
            WindowManager.Register<BundlerWidget>();

            WindowManager.Register<GraphicsDebugger>();

            WindowManager.Register<BakeWindow>();

            ObjectEditorFactory.Reset();
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

            PropertyObjectEditorRegistry.Reset();
            PropertyObjectEditorRegistry.RegisterEditor<GameObjectEditor>();
            PropertyObjectEditorRegistry.RegisterEditor<AssetFileEditor>();

            ObjectEditorFactory.RegisterEditor<ScriptComponent>(new ScriptBehaviourEditor());
            ObjectEditorFactory.RegisterEditor<TerrainRendererComponent>(new TerrainObjectEditor());
            ObjectEditorFactory.RegisterEditor<Volume>(new VolumeObjectEditor());

            PostProcessingEditorFactory.Reset();
            PostProcessingEditorFactory.RegisterEditor<ColorGrading, ColorGradingObjectEditor>();

            OverlayManager.Current.Add(new EditorSelectionOverlay());
            OverlayManager.Current.Add(new DebugDrawOverlay());
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
            //MainMenuBar.Draw();
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