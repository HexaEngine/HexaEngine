namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core;
    using HexaEngine.Core.Configuration;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Windows;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public class PreferencesWidget : EditorWindow
    {
        private object? displayedKey;

        private Hotkey? recodingHotkey;
        private string? filter = string.Empty;

        private bool unsavedChanges;

        protected override string Name => $"{UwU.Gear} Preferences";

        public PreferencesWidget()
        {
            Core.Input.Keyboard.KeyDown += Keyboard;
        }

        ~PreferencesWidget()
        {
            Core.Input.Keyboard.KeyDown -= Keyboard;
        }

        private void Keyboard(object? sender, Core.Input.Events.KeyboardEventArgs e)
        {
            recodingHotkey?.Add(e.KeyCode);
        }

        public override unsafe void DrawContent(IGraphicsContext context)
        {
            Config config = Config.Global;
            List<ConfigKey> keys = config.Keys;
            ImGui.BeginTable("Config", 2, ImGuiTableFlags.SizingFixedFit);
            ImGui.TableSetupColumn("");
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableNextColumn();

            DisplayKey("Display");

            DisplayKey("Text Editor");
            for (int i = 0; i < keys.Count; i++)
            {
                DisplayKeyNode(keys[i]);
            }

            DisplayKey("Hotkeys");

            ImGui.TableNextColumn();

            ImGui.InputText("Search", ref filter, 256);

            if (displayedKey is ConfigKey configKey)
            {
                ImGui.SameLine();

                ImGui.BeginDisabled(!unsavedChanges);
                if (ImGui.Button("Save"))
                {
                    Config.SaveGlobal();
                    unsavedChanges = false;
                    Flags &= ~ImGuiWindowFlags.UnsavedDocument;
                }
                ImGui.EndDisabled();

                ImGui.Separator();

                ImGui.Text(configKey.Name);

                for (int j = 0; j < configKey.Values.Count; j++)
                {
                    var value = configKey.Values[j];

                    if (!string.IsNullOrEmpty(filter) && !value.Name.Contains(filter, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    var val = value.Value;
                    bool changed = false;
                    if (value.IsReadOnly)
                    {
                        ImGui.BeginDisabled(true);
                    }

                    if (!value.IsReadOnly)
                    {
                        if (ImGui.SmallButton($"\uE777##{value.Name}"))
                        {
                            value.SetToDefault();
                        }

                        ImGui.SameLine();
                    }

                    switch (value.DataType)
                    {
                        case DataType.String:
                            changed = ImGui.InputText(value.Name, ref val, 1024);
                            break;

                        case DataType.Bool:
                            {
                                var v = value.GetBool();
                                changed = ImGui.Checkbox(value.Name, ref v);
                                if (changed)
                                {
                                    val = v.ToString();
                                }
                            }
                            break;

                        case DataType.UInt8:
                            {
                                var v = value.GetUInt8();
                                changed = ImGui.InputScalar(value.Name, ImGuiDataType.U8, Unsafe.AsPointer(ref v));
                                if (changed)
                                {
                                    val = v.ToString();
                                }
                            }
                            break;

                        case DataType.Int8:
                            {
                                var v = value.GetInt8();
                                changed = ImGui.InputScalar(value.Name, ImGuiDataType.S8, Unsafe.AsPointer(ref v));
                                if (changed)
                                {
                                    val = v.ToString();
                                }
                            }
                            break;

                        case DataType.UInt16:
                            {
                                var v = value.GetUInt16();
                                changed = ImGui.InputScalar(value.Name, ImGuiDataType.U16, Unsafe.AsPointer(ref v));
                                if (changed)
                                {
                                    val = v.ToString();
                                }
                            }
                            break;

                        case DataType.Int16:
                            {
                                var v = value.GetInt16();
                                changed = ImGui.InputScalar(value.Name, ImGuiDataType.S16, Unsafe.AsPointer(ref v));
                                if (changed)
                                {
                                    val = v.ToString();
                                }
                            }
                            break;

                        case DataType.UInt32:
                            {
                                var v = value.GetUInt32();
                                changed = ImGui.InputScalar(value.Name, ImGuiDataType.U32, Unsafe.AsPointer(ref v));
                                if (changed)
                                {
                                    val = v.ToString();
                                }
                            }
                            break;

                        case DataType.Int32:
                            {
                                var v = value.GetInt32();
                                changed = ImGui.InputInt(value.Name, ref v);
                                if (changed)
                                {
                                    val = v.ToString();
                                }
                            }
                            break;

                        case DataType.UInt64:
                            {
                                var v = value.GetUInt64();
                                changed = ImGui.InputScalar(value.Name, ImGuiDataType.U64, Unsafe.AsPointer(ref v));
                                if (changed)
                                {
                                    val = v.ToString();
                                }
                            }
                            break;

                        case DataType.Int64:
                            {
                                var v = value.GetInt64();
                                changed = ImGui.InputScalar(value.Name, ImGuiDataType.S64, Unsafe.AsPointer(ref v));
                                if (changed)
                                {
                                    val = v.ToString();
                                }
                            }
                            break;

                        case DataType.Float:
                            {
                                var v = value.GetFloat();
                                changed = ImGui.InputFloat(value.Name, ref v);
                                if (changed)
                                {
                                    val = v.ToString(null, CultureInfo.InvariantCulture);
                                }
                            }
                            break;

                        case DataType.Double:
                            {
                                var v = value.GetDouble();
                                changed = ImGui.InputDouble(value.Name, ref v);
                                if (changed)
                                {
                                    val = v.ToString(null, CultureInfo.InvariantCulture);
                                }
                            }
                            break;

                        case DataType.Float2:
                            {
                                var v = value.GetVector2();
                                changed = ImGui.InputFloat2(value.Name, ref v);
                                if (changed)
                                {
                                    val = v.ToString(null, CultureInfo.InvariantCulture);
                                }
                            }
                            break;

                        case DataType.Float3:
                            {
                                var v = value.GetVector3();
                                changed = ImGui.InputFloat3(value.Name, ref v);
                                if (changed)
                                {
                                    val = v.ToString(null, CultureInfo.InvariantCulture);
                                }
                            }
                            break;

                        case DataType.Float4:
                            {
                                var v = value.GetVector4();
                                changed = ImGui.InputFloat4(value.Name, ref v);
                                if (changed)
                                {
                                    val = v.ToString(null, CultureInfo.InvariantCulture);
                                }
                            }
                            break;

                        case DataType.ColorRGBA:
                            {
                                var v = value.GetVector4();
                                changed = ImGui.ColorPicker4(value.Name, ref v);
                                if (changed)
                                {
                                    val = v.ToString();
                                }
                            }
                            break;

                        case DataType.ColorRGB:
                            {
                                var v = value.GetVector3();
                                changed = ImGui.ColorPicker3(value.Name, ref v);
                                if (changed)
                                {
                                    val = v.ToString();
                                }
                            }
                            break;

                        case DataType.Button:
                            {
                                changed = ImGui.Button(value.Name);
                                if (changed)
                                {
                                    val = null;
                                }
                            }
                            break;

                        case DataType.Keys:
                            {
                            }
                            break;

                        case DataType.Enum:
                            {
                                var type = value.EnumType;
                                if (type == null)
                                {
                                    continue;
                                }

                                var v = value.GetEnum(type);
                                changed = ComboEnumHelper.Combo(value.Name, type, ref v);

                                if (changed)
                                {
                                    val = v.ToString();
                                }
                            }
                            break;
                    }

                    if (value.IsReadOnly)
                    {
                        ImGui.EndDisabled();
                    }

                    if (changed)
                    {
                        value.Value = val;
                        unsavedChanges = true;
                        Flags |= ImGuiWindowFlags.UnsavedDocument;
                    }
                }
            }

            if (displayedKey is string key)
            {
                ImGui.Separator();

                switch (key)
                {
                    case "Display":
                        DisplayPage();
                        break;

                    case "Text Editor":
                        TextEditorPage();
                        break;

                    case "Hotkeys":
                        lock (HotkeyManager.SyncObject)
                        {
                            for (int i = 0; i < HotkeyManager.Count; i++)
                            {
                                EditHotkey(HotkeyManager.Hotkeys[i]);
                            }
                        }

                        break;
                }
            }

            ImGui.EndTable();
        }

        private static void DisplayPage()
        {
            var window = (Window)Application.MainWindow;
            var isAuto = !window.ForceHDR.HasValue;
            if (ImGui.Checkbox("Auto HDR", ref isAuto))
            {
                window.ForceHDR = isAuto ? null : false;
            }
            if (window.ForceHDR.HasValue)
            {
                var forceEnabled = window.ForceHDR.Value;
                if (ImGui.Checkbox("Force HDR", ref forceEnabled))
                {
                    window.ForceHDR = forceEnabled;
                }
            }
        }

        private static unsafe void TextEditorPage()
        {
            bool changed = false;
            var type = EditorConfig.Default.ExternalTextEditorType;
            if (ComboEnumHelper<ExternalTextEditorType>.Combo("##EditorType", ref type))
            {
                EditorConfig.Default.ExternalTextEditorType = type; changed = true;
            }

            if (type == ExternalTextEditorType.Custom)
            {
                var editors = EditorConfig.Default.ExternalTextEditors;
                var selectedEditor = EditorConfig.Default.SelectedExternalTextEditor;
                ImGui.SameLine();
                if (ImGui.Button("+"))
                {
                    string name = "New Text Editor";
                    string newName = name;
                    int i = 1;
                    while (editors.Any(x => x.Name == newName))
                    {
                        newName = $"{name} {i++}";
                    }

                    editors.Add(new(newName, string.Empty, string.Empty));
                }

                for (int i = 0; i < editors.Count; i++)
                {
                    var editor = editors[i];
                    ImGuiChildFlags flags = ImGuiChildFlags.AlwaysUseWindowPadding;

                    bool isSelected = i == selectedEditor;
                    if (isSelected)
                    {
                        ImGui.PushStyleColor(ImGuiCol.ChildBg, ImGui.GetColorU32(ImGuiCol.ButtonActive));
                    }

                    ImGui.BeginChild(editor.Name, new Vector2(400, 120), flags);

                    if (ImGui.IsWindowHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                    {
                        EditorConfig.Default.SelectedExternalTextEditor = i;
                    }

                    string name = editor.Name;
                    if (ImGui.InputText("Name", ref name, 1024))
                    {
                        editor.Name = name;
                    }
                    string programPath = editor.ProgramPath;
                    if (ImGui.InputText("Path", ref programPath, 1024))
                    {
                        editor.ProgramPath = programPath;
                    }
                    string commandLine = editor.CommandLine;
                    if (ImGui.InputText("Command Line", ref commandLine, 1024))
                    {
                        editor.CommandLine = commandLine;
                    }

                    TooltipHelper.Tooltip("Use $solutionPath to insert the path of the solution file,\n" +
                              "$solutionName to insert the name of the solution file (without extension),\n" +
                              "or $projectFolder to insert the path of the project folder.");
                    if (ImGui.BeginPopupContextWindow())
                    {
                        if (ImGui.MenuItem("\xE74D Delete"))
                        {
                            editors.RemoveAt(i);
                            i--;
                        }
                        ImGui.EndPopup();
                    }

                    ImGui.EndChild();
                    if (isSelected)
                    {
                        ImGui.PopStyleColor();
                    }
                }
            }

            if (changed)
            {
                EditorConfig.Default.Save();
            }
        }

        private void EditHotkey(Hotkey hotkey)
        {
            if (ImGui.SmallButton($"\uE777##{hotkey.Name}"))
            {
                hotkey.SetToDefault();
            }

            ImGui.SameLine();

            ImGui.Text(hotkey.Name);

            ImGui.SameLine();

            string val = hotkey.ToString();
            ImGui.InputText($"##{hotkey.Name}", ref val, 256, ImGuiInputTextFlags.ReadOnly);

            if (recodingHotkey == null)
            {
                ImGui.SameLine();
                if (ImGui.SmallButton($"\uE7C8##{hotkey.Name}"))
                {
                    recodingHotkey = hotkey;
                    hotkey.Clear();
                }
            }
            else if (recodingHotkey == hotkey)
            {
                ImGui.SameLine();
                if (ImGui.SmallButton("\uEA3F"))
                {
                    recodingHotkey = null;
                }
            }
        }

        private void DisplayKeyNode(ConfigKey key)
        {
            ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow;
            if (displayedKey == key)
            {
                flags |= ImGuiTreeNodeFlags.Selected;
            }

            if (key.Keys.Count == 0)
            {
                flags |= ImGuiTreeNodeFlags.Leaf;
            }

            bool isOpen = ImGui.TreeNodeEx(key.Name, flags);
            if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            {
                displayedKey = key;
            }
            if (isOpen)
            {
                for (int j = 0; j < key.Keys.Count; j++)
                {
                    DisplayKeyNode(key.Keys[j]);
                }
                ImGui.TreePop();
            }
        }

        private void DisplayKey(string key)
        {
            ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow;
            if (displayedKey is string other && other == key)
            {
                flags |= ImGuiTreeNodeFlags.Selected;
            }

            flags |= ImGuiTreeNodeFlags.Leaf;

            bool isOpen = ImGui.TreeNodeEx(key, flags);
            if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            {
                displayedKey = key;
            }
            if (isOpen)
            {
                ImGui.TreePop();
            }
        }
    }
}