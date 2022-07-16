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

                var color = material.Albedo;
                if (ImGui.ColorEdit3("Color", ref color, ImGuiColorEditFlags.Float))
                    material.Albedo = color;

                var opacity = material.Opacity;
                if (ImGui.SliderFloat("Opacity", ref opacity, 0, 1))
                    material.Opacity = opacity;

                var roughness = material.Roughness;
                if (ImGui.SliderFloat("Roughness", ref roughness, 0, 1))
                    material.Roughness = roughness;

                var Metalness = material.Metalness;
                if (ImGui.SliderFloat("Metalness", ref Metalness, 0, 1))
                    material.Metalness = Metalness;

                var Ao = material.Ao;
                if (ImGui.SliderFloat("Ao", ref Ao, 0, 1))
                    material.Ao = Ao;

                var Emissivness = material.Emissivness;
                if (ImGui.ColorEdit3("Emissivness", ref Emissivness, ImGuiColorEditFlags.Float))
                    material.Emissivness = Emissivness;

                var texAlbedo = material.AlbedoTextureMap;
                if (ImGui.InputText("Albedo Tex", ref texAlbedo, 256))
                    material.AlbedoTextureMap = texAlbedo;

                var texNormal = material.NormalTextureMap;
                if (ImGui.InputText("Normal Tex", ref texNormal, 256))
                    material.NormalTextureMap = texNormal;

                var texdisplacement = material.DisplacementTextureMap;
                if (ImGui.InputText("Displacement Tex", ref texdisplacement, 256))
                    material.DisplacementTextureMap = texdisplacement;

                var texRoughness = material.RoughnessTextureMap;
                if (ImGui.InputText("Roughness Tex", ref texRoughness, 256))
                    material.RoughnessTextureMap = texRoughness;

                var texMetalness = material.MetalnessTextureMap;
                if (ImGui.InputText("Metalness Tex", ref texMetalness, 256))
                    material.MetalnessTextureMap = texMetalness;

                var texEmissive = material.EmissiveTextureMap;
                if (ImGui.InputText("Emissive Tex", ref texEmissive, 256))
                    material.EmissiveTextureMap = texEmissive;

                var texAo = material.AoTextureMap;
                if (ImGui.InputText("Ao Tex", ref texAo, 256))
                    material.AoTextureMap = texAo;
            }

            ImGui.End();
        }
    }
}