namespace HexaEngine.Editor.MaterialEditor.Pins
{
    using Hexa.NET.ImGui;
    using HexaEngine.Materials;
    using HexaEngine.Materials.Pins;

    public class FloatPinRenderer : BasePinRenderer<FloatPin>
    {
        protected override unsafe void DrawContent(FloatPin pin)
        {
            if (pin.defaultExpression != null)
            {
                base.DrawContent(pin);
                return;
            }

            var flags = pin.Flags;
            var Type = pin.Type;
            float* val = stackalloc float[4] { pin.valueX, pin.valueY, pin.valueZ, pin.valueW };
            float min = 0;
            float max = 1;

            bool changed = false;
            pin.IsActive = false;
            if (pin.Kind == PinKind.Input && pin.Links.Count == 0 || (flags & PinFlags.AllowOutput) != 0)
            {
                int components = PinTypeHelper.ComponentCount(Type);
                ImGui.PushItemWidth(100);

                if (flags != PinFlags.ColorEdit && flags != PinFlags.ColorPicker)
                {
                    changed |= ImGui.InputScalarN(pin.name, ImGuiDataType.Float, val, components, "%.6f");
                }
                else if (flags == PinFlags.Slider)
                {
                    changed |= ImGui.SliderScalarN(pin.name, ImGuiDataType.Float, val, components, &min, &max, "%.6f");
                }
                else if (flags == PinFlags.ColorEdit)
                {
                    changed |= components == 3 && ImGui.ColorEdit3(pin.name, val, ImGuiColorEditFlags.DisplayHex | ImGuiColorEditFlags.Hdr);
                    changed |= components == 4 && ImGui.ColorEdit4(pin.name, val, ImGuiColorEditFlags.DisplayHex | ImGuiColorEditFlags.Hdr);
                }
                else if (flags == PinFlags.ColorPicker)
                {
                    changed |= components == 3 && ImGui.ColorPicker3(pin.name, val, ImGuiColorEditFlags.DisplayHex | ImGuiColorEditFlags.Hdr);
                    changed |= components == 4 && ImGui.ColorPicker4(pin.name, val, ImGuiColorEditFlags.DisplayHex | ImGuiColorEditFlags.Hdr);
                }
                pin.IsActive |= ImGui.IsItemActive();

                if (changed)
                {
                    pin.valueX = val[0];
                    pin.valueY = val[1];
                    pin.valueZ = val[2];
                    pin.valueW = val[3];
                    pin.OnValueChanging();
                    pin.changed = true;
                }

                ImGui.PopItemWidth();
            }
            else
            {
                base.DrawContent(pin);
            }

            if (pin.changed && !pin.IsActive)
            {
                pin.changed = false;
                pin.OnValueChanged();
            }
        }
    }
}