namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using ImGuiNET;
    using System.Linq;

    public unsafe class MaterialsWidget : ImGuiWindow
    {
        private int current = -1;

        public MaterialsWidget()
        {
            IsShown = true;
        }

        protected override string Name => "Materials";

        public override void DrawContent(IGraphicsContext context)
        {
            var scene = SceneManager.Current;
            if (scene is null)
            {
                current = -1;
                EndWindow();

                return;
            }

            if (ImGui.Button("Create"))
            {
                MaterialManager.Add(new() { Name = "New Material" });
            }

            if (MaterialManager.Count == 0)
            {
                current = -1;
            }
            /*
            lock (MaterialManager.Names)
            {
                bool selected = ImGui.Combo("Material", ref current, MaterialManager.Names, MaterialManager.Count);
            }*/

            ImGui.Separator();

            if (current != -1)
            {
                var material = MaterialManager.Materials[current];

                var name = material.Name;

                bool hasChanged = false;
                if (ImGui.InputText("Name", ref name, 256, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    if (MaterialManager.Materials.Any(x => x.Name != name))
                    {
                        MaterialManager.Remove(material);
                        material.Name = name;
                        MaterialManager.Add(material);
                    }
                }

                var color = material.BaseColor;
                if (ImGui.ColorEdit3("Color", ref color, ImGuiColorEditFlags.Float))
                {
                    material.BaseColor = color;
                    hasChanged = true;
                }

                var opacity = material.Opacity;
                if (ImGui.SliderFloat("Opacity", ref opacity, 0, 1))
                {
                    material.Opacity = opacity;
                    hasChanged = true;
                }

                var roughness = material.Roughness;
                if (ImGui.SliderFloat("Roughness", ref roughness, 0, 1))
                {
                    material.Roughness = roughness;
                    hasChanged = true;
                }

                var Metalness = material.Metalness;
                if (ImGui.SliderFloat("Metalness", ref Metalness, 0, 1))
                {
                    material.Metalness = Metalness;
                    hasChanged = true;
                }

                var Ao = material.Ao;
                if (ImGui.SliderFloat("Ao", ref Ao, 0, 1))
                {
                    material.Ao = Ao;
                    hasChanged = true;
                }

                var Anisotropic = material.Anisotropic;
                if (ImGui.SliderFloat("Anisotropic", ref Anisotropic, 0, 1))
                {
                    material.Anisotropic = Anisotropic;
                    hasChanged = true;
                }

                var Emissivness = material.Emissive;
                if (ImGui.ColorEdit3("Emissivness", ref Emissivness, ImGuiColorEditFlags.Float))
                {
                    material.Emissive = Emissivness;
                    hasChanged = true;
                }

                var texAlbedo = material.BaseColorTextureMap;
                if (ImGui.InputText("Albedo Tex", ref texAlbedo, 256))
                {
                    material.BaseColorTextureMap = texAlbedo;
                    hasChanged = true;
                }

                var texNormal = material.NormalTextureMap;
                if (ImGui.InputText("Normal Tex", ref texNormal, 256))
                {
                    material.NormalTextureMap = texNormal;
                    hasChanged = true;
                }

                var texdisplacement = material.DisplacementTextureMap;
                if (ImGui.InputText("Displacement Tex", ref texdisplacement, 256))
                {
                    material.DisplacementTextureMap = texdisplacement;
                    hasChanged = true;
                }

                var texRoughness = material.RoughnessTextureMap;
                if (ImGui.InputText("Roughness Tex", ref texRoughness, 256))
                {
                    material.RoughnessTextureMap = texRoughness;
                    hasChanged = true;
                }

                var texMetalness = material.MetalnessTextureMap;
                if (ImGui.InputText("Metalness Tex", ref texMetalness, 256))
                {
                    material.MetalnessTextureMap = texMetalness;
                    hasChanged = true;
                }

                var texEmissive = material.EmissiveTextureMap;
                if (ImGui.InputText("Emissive Tex", ref texEmissive, 256))
                {
                    material.EmissiveTextureMap = texEmissive;
                    hasChanged = true;
                }

                var texAo = material.AoTextureMap;
                if (ImGui.InputText("Ao Tex", ref texAo, 256))
                {
                    material.AoTextureMap = texAo;
                    hasChanged = true;
                }

                var texRM = material.RoughnessMetalnessTextureMap;
                if (ImGui.InputText("RM Tex", ref texRM, 256))
                {
                    material.RoughnessMetalnessTextureMap = texRM;
                    hasChanged = true;
                }

                if (hasChanged)
                    MaterialManager.Update(material);
            }
        }
    }
}