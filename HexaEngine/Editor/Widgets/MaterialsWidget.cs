namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using ImGuiNET;

    public unsafe class MaterialsWidget : ImGuiWindow
    {
        private int current = -1;

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
                var name = MaterialManager.GetFreeName("New Material");
                MaterialManager.Add(new() { Name = name });
            }

            if (MaterialManager.Count == 0)
            {
                current = -1;
            }

            lock (MaterialManager.Materials)
            {
                ImGui.PushItemWidth(200);
                if (ImGui.BeginListBox("##Materials"))
                {
                    for (int i = 0; i < MaterialManager.Count; i++)
                    {
                        var material = MaterialManager.Materials[i];
                        if (ImGui.MenuItem(material.Name))
                        {
                            current = i;
                        }
                    }
                    ImGui.EndListBox();
                }
                ImGui.PopItemWidth();
            }
            ImGui.SameLine();
            ImGui.BeginChild("MaterialEditor");
            if (current != -1)
            {
                var material = MaterialManager.Materials[current];

                var name = material.Name;

                bool hasChanged = false;
                if (ImGui.InputText("Name", ref name, 256, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    MaterialManager.Rename(material.Name, name);
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

                var Specular = material.Specular;
                if (ImGui.SliderFloat("Specular", ref Specular, 0, 1))
                {
                    material.Specular = Specular;
                    hasChanged = true;
                }

                var SpecularTint = material.SpecularTint;
                if (ImGui.SliderFloat("SpecularTint", ref SpecularTint, 0, 1))
                {
                    material.SpecularTint = SpecularTint;
                    hasChanged = true;
                }

                var Sheen = material.Sheen;
                if (ImGui.SliderFloat("Sheen", ref Sheen, 0, 1))
                {
                    material.Sheen = Sheen;
                    hasChanged = true;
                }

                var SheenTint = material.SheenTint;
                if (ImGui.SliderFloat("SheenTint", ref SheenTint, 0, 1))
                {
                    material.SheenTint = SheenTint;
                    hasChanged = true;
                }

                var Cleancoat = material.Cleancoat;
                if (ImGui.SliderFloat("Cleancoat", ref Cleancoat, 0, 1))
                {
                    material.Cleancoat = Cleancoat;
                    hasChanged = true;
                }

                var CleancoatGloss = material.CleancoatGloss;
                if (ImGui.SliderFloat("CleancoatGloss", ref CleancoatGloss, 0, 1))
                {
                    material.CleancoatGloss = CleancoatGloss;
                    hasChanged = true;
                }

                var Subsurface = material.Subsurface;
                if (ImGui.SliderFloat("Subsurface", ref Subsurface, 0, 1))
                {
                    material.Subsurface = Subsurface;
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

                //TODO: Add new material texture system

                if (hasChanged)
                    MaterialManager.Update(material);
            }
            ImGui.EndChild();
        }
    }
}