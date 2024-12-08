namespace HexaEngine.Editor.MaterialEditor.Pins
{
    using Hexa.NET.ImGui;
    using HexaEngine.Materials;
    using HexaEngine.Materials.Pins;

    public class BoolPinRenderer : BasePinRenderer<BoolPin>
    {
        protected override unsafe void DrawContent(BoolPin pin)
        {
            var flags = pin.Flags;
            var Type = pin.Type;
            bool changed = false;
            pin.IsActive = false;
            bool* val = stackalloc bool[4] { pin.valueX, pin.valueY, pin.valueZ, pin.valueW };
            if (pin.Kind == PinKind.Input && pin.Links.Count == 0 || (flags & PinFlags.AllowOutput) != 0)
            {
                int components = PinTypeHelper.ComponentCount(Type);
                ImGui.PushItemWidth(100);

                if (flags != PinFlags.ColorEdit && flags != PinFlags.ColorPicker)
                {
                    for (int i = 0; i < components; i++)
                    {
                        changed |= ImGui.Checkbox(pin.nameY, &val[i]);
                        pin.IsActive |= ImGui.IsItemActive();
                    }
                }

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