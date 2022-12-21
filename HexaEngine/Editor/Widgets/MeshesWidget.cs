namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Objects;
    using HexaEngine.Scenes;
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

            bool selected = ImGui.Combo("Mesh", ref current, scene.Meshes.Select(x => x.Name).ToArray(), scene.Meshes.Count);

            ImGui.Separator();

            if (current != -1)
            {
                Mesh mesh = scene.Meshes[current];
                {
                    string name = mesh.Name;
                    if (ImGui.InputText("Name", ref name, 256, ImGuiInputTextFlags.EnterReturnsTrue))
                    {
                        if (scene.Meshes.All(x => x.Name != name))
                        {
                            mesh.Name = name;
                        }
                    }
                }
            }
        }
    }
}