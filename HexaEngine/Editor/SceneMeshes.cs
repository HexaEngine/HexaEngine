namespace HexaEngine.Editor
{
    using HexaEngine.Scenes;
    using ImGuiNET;

    public static class SceneMeshes
    {
        private static int current = -1;
        private static bool isShown;

        public static bool IsShown { get => isShown; set => isShown = value; }

        public static void Draw()
        {
            if (!ImGui.Begin("Meshes", ref isShown, ImGuiWindowFlags.MenuBar))
            {
                ImGui.End();
                return;
            }

            var scene = SceneManager.Current;
            if (scene is null)
            {
                ImGui.End();
                return;
            }

            bool selected = ImGui.Combo("Mesh", ref current, scene.Meshes.Select(x => x.Name).ToArray(), scene.Meshes.Count);

            ImGui.Separator();

            if (current != -1)
            {
                var mesh = scene.Meshes[current];
            }
        }
    }
}