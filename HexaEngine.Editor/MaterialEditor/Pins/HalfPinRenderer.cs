namespace HexaEngine.Editor.MaterialEditor.Pins
{
    using Hexa.NET.ImGui;
    using HexaEngine.Materials;
    using HexaEngine.Materials.Pins;

    public class HalfPinRenderer : BasePinRenderer<HalfPin>
    {
        protected override unsafe void DrawContent(HalfPin pin)
        {
            if (pin.defaultExpression != null)
            {
                base.DrawContent(pin);
                return;
            }

            var flags = pin.Flags;
            var Type = pin.Type;
            float* val = stackalloc float[4] { (float)pin.valueX, (float)pin.valueY, (float)pin.valueZ, (float)pin.valueW };
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
                    changed |= ImGui.InputScalarN(pin.name, ImGuiDataType.Float, val, components);
                }
                else if (flags == PinFlags.Slider)
                {
                    changed |= ImGui.SliderScalarN(pin.name, ImGuiDataType.Float, val, components, &min, &max);
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
                    pin.valueX = (Half)val[0];
                    pin.valueY = (Half)val[1];
                    pin.valueZ = (Half)val[2];
                    pin.valueW = (Half)val[3];
                    pin.OnValueChanging();
                    pin.changing = true;
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