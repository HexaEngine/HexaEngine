namespace HexaEngine.Editor.MaterialEditor.Pins
{
    using Hexa.NET.ImGui;
    using HexaEngine.Materials;
    using HexaEngine.Materials.Pins;

    public class IntPinRenderer : BasePinRenderer<IntPin>
    {
        protected override unsafe void DrawContent(IntPin pin)
        {
            var flags = pin.Flags;
            var Type = pin.Type;
            int* val = stackalloc int[4] { pin.valueX, pin.valueY, pin.valueZ, pin.valueW };
            int min = 0;
            int max = 1;

            bool changed = false;
            pin.IsActive = false;
            if (pin.Kind == PinKind.Input && pin.Links.Count == 0 || (flags & PinFlags.AllowOutput) != 0)
            {
                int components = PinTypeHelper.ComponentCount(Type);
                ImGui.PushItemWidth(100);

                if (flags != PinFlags.ColorEdit && flags != PinFlags.ColorPicker)
                {
                    changed |= ImGui.InputScalarN(pin.name, ImGuiDataType.S32, val, components);
                }
                else if (flags == PinFlags.Slider)
                {
                    changed |= ImGui.SliderScalarN(pin.name, ImGuiDataType.S32, val, components, &min, &max);
                }
                pin.IsActive |= ImGui.IsItemActive();

                if (changed)
                {
                    pin.valueX = val[0];
                    pin.valueY = val[1];
                    pin.valueZ = val[2];
                    pin.valueW = val[3];
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