namespace HexaEngine.Editor.MaterialEditor
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Extensions;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Dialogs;
    using System.Text;

    public class MaterialLibraryEditorWindow : EditorWindow
    {
        private readonly OpenFileDialog openFileDialog = new(null, ".matlib");
        private readonly SaveFileDialog saveFileDialog = new(null, ".matlib");

        private MaterialLibrary? materialLibrary;
        private string? currentFile;
        private MaterialData? material;

        public MaterialLibraryEditorWindow()
        {
            Flags = ImGuiWindowFlags.MenuBar;
        }

        protected override string Name => "Material Library";

        public void Open(string filename)
        {
            try
            {
                materialLibrary = MaterialLibrary.LoadExternal(filename);
                currentFile = filename;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load material library: {filename}", ex.Message);
                MaterialEditorWindow.Logger.Error($"Failed to load material library: {filename}");
                MaterialEditorWindow.Logger.Log(ex);
            }
        }

        public void Unload()
        {
            material = null;
            materialLibrary = null;
            currentFile = null;
        }

        public void Save()
        {
            if (currentFile == null)
                return;

            SaveAs(currentFile);
        }

        public void CreateNew()
        {
            materialLibrary = new();
        }

        public void SaveAs(string filename)
        {
            if (materialLibrary == null)
                return;

            try
            {
                materialLibrary.Save(filename, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save material library: {filename}", ex.Message);
                MaterialEditorWindow.Logger.Error($"Failed to save material library: {filename}");
                MaterialEditorWindow.Logger.Log(ex);
            }
        }

        private void DrawMenuBar()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("New"))
                    {
                        CreateNew();
                    }

                    if (ImGui.MenuItem("Load"))
                    {
                        openFileDialog.Show();
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("Save"))
                    {
                        Save();
                    }

                    if (ImGui.MenuItem("Save as"))
                    {
                        saveFileDialog.Show();
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("Close"))
                    {
                        Unload();
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }
        }

        public override void DrawContent(IGraphicsContext context)
        {
            DrawMenuBar();

            if (openFileDialog.Draw())
            {
                if (openFileDialog.Result == OpenFileResult.Ok)
                {
                    Open(openFileDialog.FullPath);
                }
            }

            if (saveFileDialog.Draw())
            {
                if (saveFileDialog.Result == SaveFileResult.Ok)
                {
                    SaveAs(saveFileDialog.FullPath);
                }
            }

            if (materialLibrary == null)
            {
                ImGui.Text("No material lib opened");
                return;
            }

            var lib = materialLibrary;

            if (ImGui.Button("New material"))
            {
                MaterialData data = new($"material {lib.Materials.Count}");
                lib.Materials.Add(data);
            }

            ImGui.BeginListBox("Materials");

            for (int i = 0; i < lib.Materials.Count; i++)
            {
                var mat = lib.Materials[i];
                if (ImGui.MenuItem(mat.Name, mat == material))
                {
                    material = mat;
                }
            }

            ImGui.EndListBox();

            if (material == null)
            {
                ImGui.Text("No material selected");
                return;
            }

            var sel = material;
            EditMaterial(sel);
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
            isActive = false;
            var name = material.Name;

            if (ImGui.InputText("Name", ref name, 256, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                material.Name = name;
            }

            if (ImGui.Button("Add Property"))
            {
                ImGui.OpenPopup("Add Property");
            }

            if (ImGui.BeginPopup("Add Property", ImGuiWindowFlags.None))
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
                    material.Properties.Add(new(newPropName, newPropType, newPropValueType, default, default, new byte[MaterialProperty.GetByteCount(newPropValueType)]));
                    ImGui.CloseCurrentPopup();
                    hasChanged = true;
                }
                ImGui.EndPopup();
            }

            for (int i = 0; i < material.Properties.Count; i++)
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
            if (ImGui.CheckboxFlags("Alpha Test", ref flags, (int)MaterialFlags.AlphaTest))
            {
                material.Flags = (MaterialFlags)flags;
                hasChanged = true;
            }
            isActive |= ImGui.IsItemActive();

            ImGui.Separator();

            if (ImGui.Button("Add Texture"))
            {
                ImGui.OpenPopup("Add Texture");
            }

            if (ImGui.BeginPopup("Add Texture", ImGuiWindowFlags.None))
            {
                /*  ComboEnumHelper<MaterialTextureType>.Combo("Type", ref newTexType);
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
                      hasChanged = true;
                  }*/
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

                /*  var file = tex.File;
                  if (ImGui.InputText($"File##{i}", ref file, 1024))
                  {
                      material.Textures.MutateItem(i, x => { x.File = file; return x; });
                      hasChanged = true;
                  }
                  isActive |= ImGui.IsItemActive();*/

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