namespace HexaEngine.Editor
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Input;
    using HexaEngine.Editor.Projects;
    using HexaEngine.Scenes;
    using ImGuiNET;
    using ImGuizmoNET;
    using ImNodesNET;
    using System.Threading.Tasks;

    public static class Designer
    {
        private static Task? task;
        private static readonly AssimpSceneLoader loader = new();
        private static bool inDesignMode = true;
        private static bool isShown = true;

        public static bool InDesignMode
        {
            get => inDesignMode;
            set
            {
                inDesignMode = value;
            }
        }

        public static bool IsShown
        {
            get => isShown;
            set => isShown = value;
        }

        public static History History { get; } = new();

        public static HexaProject? CurrentProject { get; set; }

        static Designer()
        {
            Keyboard.Released += (s, e) =>
            {
                if (Keyboard.IsDown(KeyCode.LCtrl))
                {
                    if (e.KeyCode == KeyCode.Z)
                    {
                        History.TryUndo();
                    }
                    if (e.KeyCode == KeyCode.Y)
                    {
                        History.TryRedo();
                    }
                }
            };
        }

        internal static void Draw()
        {
            if (!isShown) return;
            MainMenuBar.Draw();
            Inspector.Draw();
        }

        public static void OpenFile(string? path)
        {
            if ((task == null || task.IsCompleted) && path != null)
            {
                var extension = Path.GetExtension(path);
                if (extension == ".glb")
                {
                    task = loader.OpenAsync(path).ContinueWith(ImGuiConsole.HandleError);
                }
                if (extension == ".gltf")
                {
                    task = loader.OpenAsync(path).ContinueWith(ImGuiConsole.HandleError);
                }
                if (extension == ".dae")
                {
                    task = loader.OpenAsync(path).ContinueWith(ImGuiConsole.HandleError);
                }
                if (extension == ".obj")
                {
                    task = loader.OpenAsync(path).ContinueWith(ImGuiConsole.HandleError);
                }
            }
        }
    }
}