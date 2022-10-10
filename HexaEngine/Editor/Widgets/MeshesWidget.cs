namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Objects;
    using HexaEngine.Scenes;
    using ImGuiNET;

    public class MeshesWidget : Widget
    {
        private int current = -1;

        public override void Dispose()
        {
        }

        public override void Draw(IGraphicsContext context)
        {
            if (!IsShown) return;
            if (!ImGui.Begin("Meshes", ref IsShown, ImGuiWindowFlags.MenuBar))
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
                    if (ImGui.Combo("Material", ref mesh.MaterialIndex, scene.Materials.Select(x => x.Name).ToArray(), scene.Materials.Count))
                    {
                        mesh.MaterialName = scene.Materials[mesh.MaterialIndex].Name;
                        scene.CommandQueue.Enqueue(new SceneCommand(CommandType.Update, mesh));
                    }
                }
            }

            ImGui.End();
        }

        public override void DrawMenu()
        {
            if (ImGui.MenuItem("Meshes"))
            {
                IsShown = true;
            }
        }

        public override void Init(IGraphicsDevice device)
        {
        }
    }
}