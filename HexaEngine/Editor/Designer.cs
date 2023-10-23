namespace HexaEngine.Editor
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.ImagePainter;
    using HexaEngine.Editor.MaterialEditor;
    using HexaEngine.Editor.MeshEditor;
    using HexaEngine.Editor.Widgets;
    using HexaEngine.Scenes;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public static class Designer
    {
        private static Task? task;
        private static OpenProjectWindow projectWindow = new();

        internal static void Init(IGraphicsDevice device)
        {
            MainMenuBar.Init(device);
            projectWindow.Show();
        }

        internal static void Draw()
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