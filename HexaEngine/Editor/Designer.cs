namespace HexaEngine.Editor
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.Scenes.Serialization;
    using System.Threading.Tasks;

    public static class Designer
    {
        private static Task? task;

        public static History History { get; } = new();

        internal static void Init(IGraphicsDevice device)
        {
            MainMenuBar.Init(device);
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

                if (extension == ".hexlvl")
                {
                    task = SceneManager.AsyncLoad(SceneSerializer.Deserialize(path)).ContinueWith(ImGuiConsole.HandleError);
                }
            }
        }
    }
}