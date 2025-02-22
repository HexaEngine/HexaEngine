namespace HexaEngine.Editor.MaterialEditor.Pins
{
    using Hexa.NET.ImGui;
    using HexaEngine.Materials;
    using HexaEngine.Materials.Pins;

    public class DoublePinRenderer : BasePinRenderer<DoublePin>
    {
        protected override unsafe void DrawContent(DoublePin pin)
        {
            var flags = pin.Flags;
            var Type = pin.Type;
            double* val = stackalloc double[4] { pin.valueX, pin.valueY, pin.valueZ, pin.valueW };
            double min = 0;
            double max = 1;
            bool changed = false;
            pin.IsActive = false;
            if (pin.Kind == PinKind.Input && pin.Links.Count == 0 || (flags & PinFlags.AllowOutput) != 0)
            {
                int components = PinTypeHelper.ComponentCount(Type);
                ImGui.PushItemWidth(100);

                if (flags == PinFlags.None)
                {
                    changed |= ImGui.InputScalarN(pin.name, ImGuiDataType.Double, val, components);
                }
                else if (flags == PinFlags.Slider)
                {
                    changed |= ImGui.SliderScalarN(pin.name, ImGuiDataType.Double, val, components, &min, &max);
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