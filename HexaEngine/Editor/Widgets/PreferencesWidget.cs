namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using ImGuiNET;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class PreferencesWidget : ImGuiWindow
    {
        private ConfigKey? displayedKey;

        protected override string Name => "Preferences";

        public override unsafe void DrawContent(IGraphicsContext context)
        {
            Config config = Config.Global;
            List<ConfigKey> keys = config.Keys;
            ImGui.BeginChild("Keys", new(100, 0));
            for (int i = 0; i < keys.Count; i++)
            {
                DisplayKeyNode(keys[i]);
            }
            ImGui.EndChild();

            void DisplayKeyNode(ConfigKey key)
            {
                ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow;
                if (displayedKey == key)
                    flags |= ImGuiTreeNodeFlags.Selected;
                if (key.Keys.Count == 0)
                    flags |= ImGuiTreeNodeFlags.Leaf;
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
            ImGui.SameLine();
            ImGui.BeginChild("Settings");
            if (displayedKey != null)
            {
                ImGui.Text(displayedKey.Name);

                for (int j = 0; j < displayedKey.Values.Count; j++)
                {
                    var value = displayedKey.Values[j];
                    var val = value.Value;
                    bool changed = false;
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
                                    val = v.ToString();
                            }
                            break;

                        case DataType.UInt8:
                            {
                                var v = value.GetUInt8();
                                changed = ImGui.InputScalar(value.Name, ImGuiDataType.U8, (nint)Unsafe.AsPointer(ref v));
                                if (changed)
                                    val = v.ToString();
                            }
                            break;

                        case DataType.Int8:
                            {
                                var v = value.GetInt8();
                                changed = ImGui.InputScalar(value.Name, ImGuiDataType.S8, (nint)Unsafe.AsPointer(ref v));
                                if (changed)
                                    val = v.ToString();
                            }
                            break;

                        case DataType.UInt16:
                            {
                                var v = value.GetUInt16();
                                changed = ImGui.InputScalar(value.Name, ImGuiDataType.U16, (nint)Unsafe.AsPointer(ref v));
                                if (changed)
                                    val = v.ToString();
                            }
                            break;

                        case DataType.Int16:
                            {
                                var v = value.GetInt16();
                                changed = ImGui.InputScalar(value.Name, ImGuiDataType.S16, (nint)Unsafe.AsPointer(ref v));
                                if (changed)
                                    val = v.ToString();
                            }
                            break;

                        case DataType.UInt32:
                            {
                                var v = value.GetUInt32();
                                changed = ImGui.InputScalar(value.Name, ImGuiDataType.U32, (nint)Unsafe.AsPointer(ref v));
                                if (changed)
                                    val = v.ToString();
                            }
                            break;

                        case DataType.Int32:
                            {
                                var v = value.GetInt32();
                                changed = ImGui.InputInt(value.Name, ref v);
                                if (changed)
                                    val = v.ToString();
                            }
                            break;

                        case DataType.UInt64:
                            {
                                var v = value.GetUInt64();
                                changed = ImGui.InputScalar(value.Name, ImGuiDataType.U64, (nint)Unsafe.AsPointer(ref v));
                                if (changed)
                                    val = v.ToString();
                            }
                            break;

                        case DataType.Int64:
                            {
                                var v = value.GetInt64();
                                changed = ImGui.InputScalar(value.Name, ImGuiDataType.S64, (nint)Unsafe.AsPointer(ref v));
                                if (changed)
                                    val = v.ToString();
                            }
                            break;

                        case DataType.Float:
                            {
                                var v = value.GetFloat();
                                changed = ImGui.InputFloat(value.Name, ref v);
                                if (changed)
                                    val = v.ToString();
                            }
                            break;

                        case DataType.Double:
                            {
                                var v = value.GetDouble();
                                changed = ImGui.InputDouble(value.Name, ref v);
                                if (changed)
                                    val = v.ToString();
                            }
                            break;

                        case DataType.Float2:
                            {
                                var v = value.GetVector2();
                                changed = ImGui.InputFloat2(value.Name, ref v);
                                if (changed)
                                    val = v.ToString();
                            }
                            break;

                        case DataType.Float3:
                            {
                                var v = value.GetVector3();
                                changed = ImGui.InputFloat3(value.Name, ref v);
                                if (changed)
                                    val = v.ToString();
                            }
                            break;

                        case DataType.Float4:
                            {
                                var v = value.GetVector4();
                                changed = ImGui.InputFloat4(value.Name, ref v);
                                if (changed)
                                    val = v.ToString();
                            }
                            break;

                        case DataType.ColorRGBA:
                            {
                                var v = value.GetVector4();
                                changed = ImGui.ColorPicker4(value.Name, ref v);
                                if (changed)
                                    val = v.ToString();
                            }
                            break;

                        case DataType.ColorRGB:
                            {
                                var v = value.GetVector3();
                                changed = ImGui.ColorPicker3(value.Name, ref v);
                                if (changed)
                                    val = v.ToString();
                            }
                            break;
                    }

                    if (changed)
                        value.Value = val;
                }
            }
            ImGui.EndChild();
        }
    }
}