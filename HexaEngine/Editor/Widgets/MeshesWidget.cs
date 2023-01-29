namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using ImGuiNET;

    public class MeshesWidget : ImGuiWindow
    {
        private int current = -1;

        protected override string Name => "Meshes";

        public override void DrawContent(IGraphicsContext context)
        {
            var scene = SceneManager.Current;
            if (scene is null)
            {
                EndWindow();
                return;
            }

            var manager = scene.MeshManager;

            bool selected = ImGui.Combo("Mesh", ref current, manager.Meshes.Select(x => x.Name).ToArray(), manager.Count);

            ImGui.Separator();

            if (current != -1)
            {
            }
        }
    }
}