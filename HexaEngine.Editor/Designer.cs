namespace HexaEngine.Editor
{
    using HexaEngine.Components;
    using HexaEngine.Components.Renderer;
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.ImagePainter;
    using HexaEngine.Editor.MaterialEditor;
    using HexaEngine.Editor.MeshEditor;
    using HexaEngine.Editor.Properties;
    using HexaEngine.Editor.Properties.Editors;
    using HexaEngine.Editor.Properties.Factories;
    using HexaEngine.Editor.Widgets;
    using HexaEngine.Scenes;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public static class Designer
    {
        private static Task? task;
        private static OpenProjectWindow projectWindow = new();

        public static void Init(IGraphicsDevice device)
        {
            MainMenuBar.Init(device);
            projectWindow.Show();

            ObjectEditorFactory.AddFactory(new BoolPropertyEditorFactory());
            ObjectEditorFactory.AddFactory(new EnumPropertyEditorFactory());
            ObjectEditorFactory.AddFactory(new FloatPropertyEditorFactory());
            ObjectEditorFactory.AddFactory(new StringPropertyEditorFactory());
            ObjectEditorFactory.AddFactory(new TypePropertyFactory());
            ObjectEditorFactory.AddFactory(new Vector2PropertyEditorFactory());
            ObjectEditorFactory.AddFactory(new Vector3PropertyEditorFactory());
            ObjectEditorFactory.AddFactory(new Vector4PropertyEditorFactory());
            ObjectEditorFactory.AddFactory(new SubTypePropertyFactory());

            ObjectEditorFactory.RegisterEditor(typeof(ScriptBehaviour), new ScriptBehaviourEditor());
            ObjectEditorFactory.RegisterEditor(typeof(TerrainRendererComponent), new TerrainEditor());
        }

        public static void Draw()
        {
            if (!Application.InEditorMode)
            {
                return;
            }
            projectWindow.Draw();
            MainMenuBar.Draw();
        }

        public static void OpenFile(string? path)
        {
            if ((task == null || task.IsCompleted) && path != null)
            {
                var extension = Path.GetExtension(path);

                if (extension == ".hexlvl")
                {
                    task = SceneManager.AsyncLoad(path).ContinueWith(Logger.HandleError);
                }

                if ((extension == ".dds" ||
                     extension == ".png" ||
                     extension == ".jpg" ||
                     extension == ".ico" ||
                     extension == ".bmp" ||
                     extension == ".tga" ||
                     extension == ".hdr"
                     )
                    && WindowManager.TryGetWindow<ImagePainterWindow>(out var imagePainterWindow))
                {
                    imagePainterWindow.Open(path);
                    imagePainterWindow.Focus();
                }

                if (extension == ".model"
                    && WindowManager.TryGetWindow<MeshEditorWindow>(out var meshEditorWindow))
                {
                    meshEditorWindow.Open(path);
                    meshEditorWindow.Focus();
                }

                if (extension == ".matlib"
                   && WindowManager.TryGetWindow<MaterialEditorWindow>(out var materialEditorWindow))
                {
                    materialEditorWindow.Open(path);
                    materialEditorWindow.Focus();
                }
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