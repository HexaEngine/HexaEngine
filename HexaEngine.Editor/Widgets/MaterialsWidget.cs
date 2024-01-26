﻿namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Core.UI;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using Hexa.NET.ImGui;
    using MaterialTexture = Core.IO.Materials.MaterialTexture;
    using HexaEngine.Core.Extensions;

    public unsafe class MaterialsWidget : EditorWindow
    {
        private int current = -1;
        private bool hasChanged;
        private bool hasFileSaved = true;
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

            var manager = scene.MaterialManager;

            if (manager.Count == 0)
            {
                current = -1;
            }

            lock (manager.Materials)
            {
                ImGui.PushItemWidth(200);
                if (ImGui.BeginListBox("##Materials"))
                {
                    for (int i = 0; i < manager.Count; i++)
                    {
                        var material = manager.Materials[i];
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
                EditMaterial(manager, manager.Materials[current]);
            }
            ImGui.EndChild();
        }

        private string newPropName = string.Empty;
        private MaterialPropertyType newPropType;
        private MaterialValueType newPropValueType;

        private string newTexPath = string.Empty;
        private MaterialTextureType newTexType;

        public void EditMaterial(MaterialManager manager, MaterialData material)
        {
            var name = material.Name;
            isActive = false;
            if (ImGui.InputText("Name", ref name, 256, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                manager.Rename(material.Name, name);
            }

            if (ImGui.Button("ObjectAdded Property"))
            {
                ImGui.OpenPopup("AddMaterialProperty");
            }

            if (ImGui.BeginPopup("AddMaterialProperty", ImGuiWindowFlags.None))
            {
                ImGui.InputText("Name", ref newPropName, 256);
                if (ComboEnumHelper<MaterialPropertyType>.Combo("Type", ref newPropType))
                {
                    newPropName = newPropType.ToString();
                }
                ComboEnumHelper<MaterialValueType>.Combo("Value Type", ref newPropValueType);
                if (ImGui.Button("Cancel"))
                {
                    ImGui.CloseCurrentPopup();
                }
                ImGui.SameLine();
                if (ImGui.Button("ObjectAdded"))
                {
                    material.Properties.Add(new(newPropName, newPropType, newPropValueType, default, default, new byte[MaterialProperty.GetByteCount(newPropValueType)]));
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }

            for (int i = 0; i < material.Properties.Count; i++)
            {
                var prop = material.Properties[i];
                switch (prop.ValueType)
                {
                    case MaterialValueType.Float:
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

                    case MaterialValueType.Float2:
                        break;

                    case MaterialValueType.Float3:
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

                    case MaterialValueType.Float4:
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

                    case MaterialValueType.Bool:
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

                    case MaterialValueType.UInt8:
                        break;

                    case MaterialValueType.UInt16:
                        break;

                    case MaterialValueType.UInt32:
                        break;

                    case MaterialValueType.UInt64:
                        break;

                    case MaterialValueType.Int8:
                        break;

                    case MaterialValueType.Int16:
                        break;

                    case MaterialValueType.Int32:
                        break;

                    case MaterialValueType.Int64:
                        break;
                }
            }

            var flags = (int)material.Flags;
            if (ImGui.CheckboxFlags("Transparent", ref flags, (int)MaterialFlags.Transparent))
            {
                material.Flags = (MaterialFlags)flags;
                hasChanged = true;
            }
            isActive |= ImGui.IsItemActive();

            ImGui.Separator();

            if (ImGui.Button("Add Texture"))
            {
                ImGui.OpenPopup("AddMaterialTexture");
            }

            if (ImGui.BeginPopup("AddMaterialTexture", ImGuiWindowFlags.None))
            {
                ComboEnumHelper<MaterialTextureType>.Combo("Type", ref newTexType);
                ImGui.InputText("Path", ref newTexPath, 256);

                if (ImGui.Button("Cancel"))
                {
                    ImGui.CloseCurrentPopup();
                }
                ImGui.SameLine();
                if (ImGui.Button("Add"))
                {
                    material.Textures.Add(new(newTexType, newTexPath, BlendMode.Default, TextureOp.None, 0, 0, TextureMapMode.Wrap, TextureMapMode.Wrap, TextureFlags.None));
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }

            for (int i = 0; i < material.Textures.Count; i++)
            {
                var tex = material.Textures[i];

                var iType = Array.IndexOf(MaterialTexture.TextureTypes, tex.Type);
                if (ImGui.Combo($"Type##{i}", ref iType, MaterialTexture.TextureTypeNames, MaterialTexture.TextureTypeNames.Length))
                {
                    material.Textures.MutateItem(i, x => { x.Type = MaterialTexture.TextureTypes[iType]; return x; });
                    hasChanged = true;
                }
                isActive |= ImGui.IsItemActive();

                var file = tex.File;
                if (ImGui.InputText($"File##{i}", ref file, 1024))
                {
                    material.Textures.MutateItem(i, x => { x.File = file; return x; });
                    hasChanged = true;
                }
                isActive |= ImGui.IsItemActive();

                var iBlend = Array.IndexOf(MaterialTexture.BlendModes, tex.Blend);
                if (ImGui.Combo($"Blend##{i}", ref iBlend, MaterialTexture.BlendModeNames, MaterialTexture.BlendModeNames.Length))
                {
                    material.Textures.MutateItem(i, x => { x.Blend = MaterialTexture.BlendModes[iBlend]; return x; });
                    hasChanged = true;
                }
                isActive |= ImGui.IsItemActive();

                var iOp = Array.IndexOf(MaterialTexture.TextureOps, tex.Op);
                if (ImGui.Combo($"TextureOp##{i}", ref iOp, MaterialTexture.TextureOpNames, MaterialTexture.TextureOpNames.Length))
                {
                    material.Textures.MutateItem(i, x => { x.Op = MaterialTexture.TextureOps[iOp]; return x; });
                    hasChanged = true;
                }
                isActive |= ImGui.IsItemActive();

                var mapping = tex.Mapping;
                if (ImGui.InputInt($"Mapping##{i}", ref mapping))
                {
                    material.Textures.MutateItem(i, x => { x.Mapping = mapping; return x; });
                    hasChanged = true;
                }
                isActive |= ImGui.IsItemActive();

                var uvwSrc = tex.UVWSrc;
                if (ImGui.InputInt($"UVWSrc##{i}", ref uvwSrc))
                {
                    material.Textures.MutateItem(i, x => { x.UVWSrc = uvwSrc; return x; });
                    hasChanged = true;
                }
                isActive |= ImGui.IsItemActive();

                var iU = Array.IndexOf(MaterialTexture.TextureMapModes, tex.U);
                if (ImGui.Combo($"U##{i}", ref iU, MaterialTexture.TextureMapModeNames, MaterialTexture.TextureMapModeNames.Length))
                {
                    material.Textures.MutateItem(i, x => { x.U = MaterialTexture.TextureMapModes[iU]; return x; });
                    hasChanged = true;
                }
                isActive |= ImGui.IsItemActive();

                var iV = Array.IndexOf(MaterialTexture.TextureMapModes, tex.V);
                if (ImGui.Combo($"V##{i}", ref iV, MaterialTexture.TextureMapModeNames, MaterialTexture.TextureMapModeNames.Length))
                {
                    material.Textures.MutateItem(i, x => { x.V = MaterialTexture.TextureMapModes[iV]; return x; });
                    hasChanged = true;
                }
                isActive |= ImGui.IsItemActive();

                var texFlags = (int)tex.Flags;
                if (ImGui.CheckboxFlags($"Invert##{i}", ref texFlags, (int)TextureFlags.Invert))
                {
                    material.Textures.MutateItem(i, x => { x.Flags ^= TextureFlags.Invert; return x; });
                    hasChanged = true;
                }
                isActive |= ImGui.IsItemActive();
                if (ImGui.CheckboxFlags($"UseAlpha##{i}", ref texFlags, (int)TextureFlags.UseAlpha))
                {
                    material.Textures.MutateItem(i, x => { x.Flags ^= TextureFlags.UseAlpha; return x; });
                    hasChanged = true;
                }
                isActive |= ImGui.IsItemActive();
                if (ImGui.CheckboxFlags($"IgnoreAlpha##{i}", ref texFlags, (int)TextureFlags.IgnoreAlpha))
                {
                    material.Textures.MutateItem(i, x => { x.Flags ^= TextureFlags.IgnoreAlpha; return x; });
                    hasChanged = true;
                }
                isActive |= ImGui.IsItemActive();
                if (i < material.Textures.Count - 1)
                {
                    ImGui.Separator();
                }
            }

            //TODO: ObjectAdded new material texture system
            if (hasChanged && !isActive)
            {
                manager.Update(material);
                hasChanged = false;
                hasFileSaved = false;
            }
            ImGui.BeginDisabled(hasFileSaved);
            if (ImGui.Button("Save"))
            {
                //var lib = manager.GetMaterialLibraryForm(material);
                //var path = Paths.CurrentProjectFolder + manager.GetPathToMaterialLibrary(lib);
                //lib.Save(path, Encoding.UTF8);
            }
            ImGui.EndDisabled();

            ImGui.Text($"HasChanged: {hasChanged}, IsActive: {isActive}");
        }
    }
}