namespace HexaEngine.Editor.MaterialEditor
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Core.UI;
    using HexaEngine.ImGuiNET;

    public class MaterialLibraryWindow : EditorWindow
    {
        private readonly MaterialEditorWindow materialEditor;

        public MaterialLibraryWindow(MaterialEditorWindow materialEditor)
        {
            this.materialEditor = materialEditor;
            IsShown = true;
        }

        protected override string Name => "Material Library";

        public override void DrawContent(IGraphicsContext context)
        {
            if (materialEditor.MaterialLibrary == null)
            {
                ImGui.Text("No material lib opened");
                return;
            }

            var lib = materialEditor.MaterialLibrary;

            if (ImGui.Button("New material"))
            {
                MaterialData data = new($"material {lib.Materials.Length}");
                ArrayUtils.Add(ref lib.Materials, data);
            }

            ImGui.BeginListBox("Materials");

            for (int i = 0; i < lib.Materials.Length; i++)
            {
                var mat = lib.Materials[i];
                if (ImGui.MenuItem(mat.Name, mat == materialEditor.Material))
                {
                    materialEditor.Material = mat;
                }
            }

            ImGui.EndListBox();

            if (materialEditor.Material == null)
            {
                ImGui.Text("No material selected");
                return;
            }

            var sel = materialEditor.Material;
            //EditMaterial(sel);
        }

        private string newPropName = string.Empty;
        private MaterialPropertyType newPropType;
        private MaterialValueType newPropValueType;

        private string newTexPath = string.Empty;
        private MaterialTextureType newTexType;
        private bool isActive;
        private bool hasChanged;
        private bool hasFileSaved;

        public bool EditMaterial(MaterialData material)
        {
            var name = material.Name;
            isActive = false;
            if (ImGui.InputText("Name", ref name, 256, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                material.Name = name;
                //manager.Rename(material.Name, name);
            }

            if (ImGui.Button("Add Property"))
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
                if (ImGui.Button("Add"))
                {
                    var props = material.Properties;
                    ArrayUtils.Add(ref props, new(newPropName, newPropType, newPropValueType, default, default, new byte[MaterialProperty.GetByteCount(newPropValueType)]));
                    material.Properties = props;
                    ImGui.CloseCurrentPopup();
                    hasChanged = true;
                }
                ImGui.EndPopup();
            }

            for (int i = 0; i < material.Properties.Length; i++)
            {
                var prop = material.Properties[i];
                EditProperty(material, i, prop);
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
                    var textures = material.Textures;
                    ArrayUtils.Add(ref textures, new(newTexType, newTexPath, BlendMode.Default, TextureOp.None, 0, 0, TextureMapMode.Wrap, TextureMapMode.Wrap, TextureFlags.None));
                    material.Textures = textures;
                    ImGui.CloseCurrentPopup();
                    hasChanged = true;
                }
                ImGui.EndPopup();
            }

            for (int i = 0; i < material.Textures.Length; i++)
            {
                var tex = material.Textures[i];

                var iType = Array.IndexOf(MaterialTexture.TextureTypes, tex.Type);
                if (ImGui.Combo($"Type##{i}", ref iType, MaterialTexture.TextureTypeNames, MaterialTexture.TextureTypeNames.Length))
                {
                    material.Textures[i].Type = MaterialTexture.TextureTypes[iType];
                    hasChanged = true;
                }
                isActive |= ImGui.IsItemActive();

                var file = tex.File;
                if (ImGui.InputText($"File##{i}", ref file, 1024))
                {
                    material.Textures[i].File = file;
                    hasChanged = true;
                }
                isActive |= ImGui.IsItemActive();

                var iBlend = Array.IndexOf(MaterialTexture.BlendModes, tex.Blend);
                if (ImGui.Combo($"Blend##{i}", ref iBlend, MaterialTexture.BlendModeNames, MaterialTexture.BlendModeNames.Length))
                {
                    material.Textures[i].Blend = MaterialTexture.BlendModes[iBlend];
                    hasChanged = true;
                }
                isActive |= ImGui.IsItemActive();

                var iOp = Array.IndexOf(MaterialTexture.TextureOps, tex.Op);
                if (ImGui.Combo($"TextureOp##{i}", ref iOp, MaterialTexture.TextureOpNames, MaterialTexture.TextureOpNames.Length))
                {
                    material.Textures[i].Op = MaterialTexture.TextureOps[iOp];
                    hasChanged = true;
                }
                isActive |= ImGui.IsItemActive();

                var mapping = tex.Mapping;
                if (ImGui.InputInt($"Mapping##{i}", ref mapping))
                {
                    material.Textures[i].Mapping = mapping;
                    hasChanged = true;
                }
                isActive |= ImGui.IsItemActive();

                var uvwSrc = tex.UVWSrc;
                if (ImGui.InputInt($"UVWSrc##{i}", ref uvwSrc))
                {
                    material.Textures[i].UVWSrc = uvwSrc;
                    hasChanged = true;
                }
                isActive |= ImGui.IsItemActive();

                var iU = Array.IndexOf(MaterialTexture.TextureMapModes, tex.U);
                if (ImGui.Combo($"U##{i}", ref iU, MaterialTexture.TextureMapModeNames, MaterialTexture.TextureMapModeNames.Length))
                {
                    material.Textures[i].U = MaterialTexture.TextureMapModes[iU];
                    hasChanged = true;
                }
                isActive |= ImGui.IsItemActive();

                var iV = Array.IndexOf(MaterialTexture.TextureMapModes, tex.V);
                if (ImGui.Combo($"V##{i}", ref iV, MaterialTexture.TextureMapModeNames, MaterialTexture.TextureMapModeNames.Length))
                {
                    material.Textures[i].V = MaterialTexture.TextureMapModes[iV];
                    hasChanged = true;
                }
                isActive |= ImGui.IsItemActive();

                var texFlags = (int)tex.Flags;
                if (ImGui.CheckboxFlags($"Invert##{i}", ref texFlags, (int)TextureFlags.Invert))
                {
                    material.Textures[i].Flags ^= TextureFlags.Invert;
                    hasChanged = true;
                }
                isActive |= ImGui.IsItemActive();
                if (ImGui.CheckboxFlags($"UseAlpha##{i}", ref texFlags, (int)TextureFlags.UseAlpha))
                {
                    material.Textures[i].Flags ^= TextureFlags.UseAlpha;
                    hasChanged = true;
                }
                isActive |= ImGui.IsItemActive();
                if (ImGui.CheckboxFlags($"IgnoreAlpha##{i}", ref texFlags, (int)TextureFlags.IgnoreAlpha))
                {
                    material.Textures[i].Flags ^= TextureFlags.IgnoreAlpha;
                    hasChanged = true;
                }
                isActive |= ImGui.IsItemActive();
                if (i < material.Textures.Length - 1)
                {
                    ImGui.Separator();
                }
            }

            bool result = false;

            if (hasChanged && !isActive)
            {
                hasChanged = false;
                hasFileSaved = false;
                result = true;
            }

            ImGui.Text($"HasChanged: {hasChanged}, IsActive: {isActive}");

            return result;
        }

        private void EditProperty(MaterialData material, int i, MaterialProperty prop)
        {
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
    }
}