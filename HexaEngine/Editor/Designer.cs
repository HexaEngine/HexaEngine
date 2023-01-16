namespace HexaEngine.Editor
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.Scenes.Serialization;
    using HexaEngine.Scenes.Importer;
    using System.Threading.Tasks;

    public static class Designer
    {
        private static Task? task;
        private static readonly AssimpSceneLoader loader = new();

        public static History History { get; } = new();

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
            if (!Application.InEditorMode) return;
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
                    task = loader.ImportAsync(path).ContinueWith(ImGuiConsole.HandleError);
                }
                if (extension == ".gltf")
                {
                    task = loader.ImportAsync(path).ContinueWith(ImGuiConsole.HandleError);
                }
                if (extension == ".dae")
                {
                    task = loader.ImportAsync(path).ContinueWith(ImGuiConsole.HandleError);
                }
                if (extension == ".obj")
                {
                    task = loader.ImportAsync(path).ContinueWith(ImGuiConsole.HandleError);
                }
                if (extension == ".hexlvl")
                {
                    task = SceneManager.AsyncLoad(SceneSerializer.Deserialize(path)).ContinueWith(ImGuiConsole.HandleError);
                }
            }
        }
    }
}