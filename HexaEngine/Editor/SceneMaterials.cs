namespace HexaEngine.Editor
{
    using HexaEngine.Scenes;
    using ImGuiNET;
    using System.Linq;

    public static class SceneMaterials
    {
        private static int current = -1;

        private static bool isShown;
        public static bool IsShown { get => isShown; set => isShown = value; }

        internal static void Draw()
        {
            if (!ImGui.Begin("Materials", ref isShown, ImGuiWindowFlags.MenuBar))
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

            if (ImGui.Button("Create"))
            {
                scene.AddMaterial(new() { Name = "New Material" });
            }

            bool selected = ImGui.Combo("Material", ref current, scene.Materials.Select(x => x.Name).ToArray(), scene.Materials.Count);

            ImGui.Separator();

            if (current != -1)
            {
                var material = scene.Materials[current];

                var name = material.Name;
                if (ImGui.InputText("Name", ref name, 256))
                    material.Name = name;

                var color = material.Color;
                if (ImGui.ColorEdit3("Color", ref color, ImGuiColorEditFlags.Float))
                    material.Color = color;

                var opacity = material.Opacity;
                if (ImGui.InputFloat("Opacity", ref opacity))
                    material.Opacity = opacity;

                var roughness = material.Roughness;
                if (ImGui.InputFloat("Roughness", ref roughness))
                    material.Roughness = roughness;

                var Metalness = material.Metalness;
                if (ImGui.InputFloat("Metalness", ref Metalness))
                    material.Metalness = Metalness;

                var Ao = material.Ao;
                if (ImGui.InputFloat("Ao", ref Ao))
                    material.Ao = Ao;

                var Emissivness = material.Emissivness;
                if (ImGui.ColorEdit3("Emissivness", ref Emissivness, ImGuiColorEditFlags.Float))
                    material.Emissivness = Emissivness;
            }

            ImGui.End();
        }
    }
}