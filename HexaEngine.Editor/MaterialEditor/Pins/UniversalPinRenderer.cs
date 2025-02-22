namespace HexaEngine.Editor.MaterialEditor.Pins
{
    using Hexa.NET.ImGui;
    using Hexa.NET.Mathematics;
    using HexaEngine.Materials;
    using HexaEngine.Materials.Pins;
    using System.Numerics;

    public class UniversalPinRenderer : BasePinRenderer<UniversalPin>
    {
        protected override unsafe void DrawContent(UniversalPin pin)
        {
            var flags = pin.Flags;
            var Type = pin.Type;

            bool changed = false;
            pin.IsActive = false;
            if (pin.Kind == PinKind.Input && pin.Links.Count == 0 || (flags & PinFlags.AllowOutput) != 0)
            {
                int components = PinTypeHelper.ComponentCount(Type);
                NumericType numericType = PinTypeHelper.GetNumericType(Type);
                ImGui.PushItemWidth(100);

                ImGuiDataType dataType = 0;
                void* data = null;
                void* min = null;
                void* max = null;
                switch (numericType)
                {
                    case NumericType.Bool:
                        dataType = ImGuiDataType.Bool;
                        var boolVector = pin.boolVector;
                        data = &boolVector;
                        break;

                    case NumericType.Half:
                        dataType = ImGuiDataType.Float;
                        Vector4 halfVector = pin.halfVector; // expand to float to avoid issues with imgui.
                        data = &halfVector;
                        break;

                    case NumericType.Float:
                        dataType = ImGuiDataType.Float;
                        var floatVector = pin.floatVector;
                        data = &floatVector;
                        break;

                    case NumericType.Double:
                        dataType = ImGuiDataType.Double;
                        var doubleVector = pin.doubleVector;
                        data = &doubleVector;
                        break;

                    case NumericType.Int:
                        dataType = ImGuiDataType.S32;
                        var intVector = pin.intVector;
                        data = &intVector;
                        break;

                    case NumericType.UInt:
                        dataType = ImGuiDataType.U32;
                        var uintVector = pin.uintVector;
                        data = &uintVector;
                        break;
                }

                if (data != null)
                {
                    if (numericType == NumericType.Bool)
                    {
                        for (int i = 0; i < components; i++)
                        {
                            changed |= ImGui.Checkbox(pin.nameY, &((bool*)data)[i]);
                            pin.IsActive |= ImGui.IsItemActive();
                        }
                    }
                    else
                    {
                        if (flags == PinFlags.Slider)
                        {
                            changed |= ImGui.SliderScalarN(pin.name, dataType, data, components, min, max);
                        }
                        else if (flags == PinFlags.ColorEdit && dataType == ImGuiDataType.Float)
                        {
                            changed |= components == 3 && ImGui.ColorEdit3(pin.name, (float*)data, ImGuiColorEditFlags.DisplayHex | ImGuiColorEditFlags.Hdr);
                            changed |= components == 4 && ImGui.ColorEdit4(pin.name, (float*)data, ImGuiColorEditFlags.DisplayHex | ImGuiColorEditFlags.Hdr);
                        }
                        else if (flags == PinFlags.ColorPicker && dataType == ImGuiDataType.Float)
                        {
                            changed |= components == 3 && ImGui.ColorPicker3(pin.name, (float*)data, ImGuiColorEditFlags.DisplayHex | ImGuiColorEditFlags.Hdr);
                            changed |= components == 4 && ImGui.ColorPicker4(pin.name, (float*)data, ImGuiColorEditFlags.DisplayHex | ImGuiColorEditFlags.Hdr);
                        }
                        else
                        {
                            changed |= ImGui.InputScalarN(pin.name, dataType, data, components);
                        }
                        pin.IsActive |= ImGui.IsItemActive();
                    }
                    if (changed)
                    {
                        switch (numericType)
                        {
                            case NumericType.Bool:
                                pin.boolVector = *(BoolVector4*)data;
                                break;

                            case NumericType.Half:
                                pin.halfVector = (HalfVector4)(*(Vector4*)data); // expand to float to avoid issues with imgui.
                                break;

                            case NumericType.Float:
                                pin.floatVector = *(Vector4*)data;
                                break;

                            case NumericType.Double:
                                pin.doubleVector = *(Vector4D*)data;
                                break;

                            case NumericType.Int:
                                pin.intVector = *(Point4*)data;
                                break;

                            case NumericType.UInt:
                                pin.uintVector = *(UPoint4*)data;
                                break;
                        }
                        pin.OnValueChanging();
                        pin.changing = true;
                    }
                }
                else
                {
                    ImGui.Text(pin.Name);
                }

                ImGui.PopItemWidth();
            }
            else
            {
                base.DrawContent(pin);
            }

            if (pin.changing && !pin.IsActive)
            {
                pin.changing = false;
                pin.OnValueChanged();
            }
        }
    }
}