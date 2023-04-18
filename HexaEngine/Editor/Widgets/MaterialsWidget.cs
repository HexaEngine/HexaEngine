namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using ImGuiNET;

    public unsafe class MaterialsWidget : ImGuiWindow
    {
        private int current = -1;
        private bool hasChanged;
        private bool isActive;

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
                isActive = false;
                if (ImGui.InputText("Name", ref name, 256, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    MaterialManager.Rename(material.Name, name);
                }

                for (int i = 0; i < material.Properties.Length; i++)
                {
                    var prop = material.Properties[i];
                    switch (prop.ValueType)
                    {
                        case Core.IO.Materials.MaterialValueType.Float:
                            {
                                var value = prop.AsFloat();
                                if (ImGui.SliderFloat(prop.Name, ref value, 0, 1))
                                {
                                    material.Properties[i].SetFloat(value);
                                    hasChanged = true;
                                }
                                isActive |= ImGui.IsItemActive();
                            }
                            break;

                        case Core.IO.Materials.MaterialValueType.Float2:
                            break;

                        case Core.IO.Materials.MaterialValueType.Float3:
                            {
                                var value = prop.AsFloat3();
                                if (ImGui.ColorEdit3(prop.Name, ref value, ImGuiColorEditFlags.Float))
                                {
                                    material.Properties[i].SetFloat3(value);
                                    hasChanged = true;
                                }
                                isActive |= ImGui.IsItemActive();
                            }
                            break;

                        case Core.IO.Materials.MaterialValueType.Float4:
                            {
                                var value = prop.AsFloat4();
                                if (ImGui.ColorEdit4(prop.Name, ref value, ImGuiColorEditFlags.Float))
                                {
                                    material.Properties[i].SetFloat4(value);
                                    hasChanged = true;
                                }
                                isActive |= ImGui.IsItemActive();
                            }
                            break;

                        case Core.IO.Materials.MaterialValueType.Bool:
                            {
                                var value = prop.AsBool();
                                if (ImGui.Checkbox(prop.Name, ref value))
                                {
                                    material.Properties[i].SetBool(value);
                                    hasChanged = true;
                                }
                                isActive |= ImGui.IsItemActive();
                            }
                            break;

                        case Core.IO.Materials.MaterialValueType.UInt8:
                            break;

                        case Core.IO.Materials.MaterialValueType.UInt16:
                            break;

                        case Core.IO.Materials.MaterialValueType.UInt32:
                            break;

                        case Core.IO.Materials.MaterialValueType.UInt64:
                            break;

                        case Core.IO.Materials.MaterialValueType.Int8:
                            break;

                        case Core.IO.Materials.MaterialValueType.Int16:
                            break;

                        case Core.IO.Materials.MaterialValueType.Int32:
                            break;

                        case Core.IO.Materials.MaterialValueType.Int64:
                            break;
                    }
                }

                //TODO: Add new material texture system
                if (hasChanged && !isActive)
                {
                    MaterialManager.Update(material);
                    hasChanged = false;
                }
                ImGui.Text($"HasChanged: {hasChanged}, IsActive: {isActive}");
            }
            ImGui.EndChild();
        }
    }
}