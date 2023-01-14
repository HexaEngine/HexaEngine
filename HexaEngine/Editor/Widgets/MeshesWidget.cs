namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Objects;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
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

            bool selected = ImGui.Combo("Mesh", ref current, MeshManager.Meshes.Select(x => x.Name).ToArray(), MeshManager.Count);

            ImGui.Separator();

            if (current != -1)
            {
                MeshData mesh = MeshManager.Meshes[current];
                {
                    string name = mesh.Name;
                    if (ImGui.InputText("DebugName", ref name, 256, ImGuiInputTextFlags.EnterReturnsTrue))
                    {
                        if (MeshManager.Meshes.All(x => x.Name != name))
                        {
                            MeshManager.Remove(mesh);
                            mesh.Name = name;
                            MeshManager.Add(mesh);
                        }
                    }
                }
            }
        }
    }
}