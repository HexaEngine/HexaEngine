namespace HexaEngine.Editor.NodeEditor.Pins
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImNodes;
    using HexaEngine.Editor.NodeEditor;
    using Newtonsoft.Json;
    using System.Diagnostics;
    using System.Globalization;

    public class IntPin : Pin, IDefaultValuePin
    {
        private readonly string name;
        private readonly string nameY;
        private readonly string nameZ;
        private readonly string nameW;
        private readonly PinFlags flags;
        private int valueX;
        private int valueY;
        private int valueZ;
        private int valueW;

        [JsonConstructor]
        public IntPin(int id, string name, ImNodesPinShape shape, PinKind kind, PinType type, uint maxLinks, PinFlags flags, int valueX, int valueY, int valueZ, int valueW) : base(id, name, shape, kind, type, maxLinks)
        {
            this.name = $"{name}##Value{Id.ToString(CultureInfo.InvariantCulture)}";
            nameY = $"##Value1{Id.ToString(CultureInfo.InvariantCulture)}";
            nameZ = $"##Value2{Id.ToString(CultureInfo.InvariantCulture)}";
            nameW = $"##Value3{Id.ToString(CultureInfo.InvariantCulture)}";
            this.flags = flags;
            this.valueX = valueX;
            this.valueY = valueY;
            this.valueZ = valueZ;
            this.valueW = valueW;
            SanityChecks();
        }

        public IntPin(int id, string name, ImNodesPinShape shape, PinKind kind, PinType type, uint maxLinks = uint.MaxValue, PinFlags flags = PinFlags.None) : base(id, name, shape, kind, type, maxLinks)
        {
            this.name = $"{name}##Value{Id.ToString(CultureInfo.InvariantCulture)}";
            nameY = $"##Value1{Id.ToString(CultureInfo.InvariantCulture)}";
            nameZ = $"##Value2{Id.ToString(CultureInfo.InvariantCulture)}";
            nameW = $"##Value3{Id.ToString(CultureInfo.InvariantCulture)}";
            this.flags = flags;
            SanityChecks();
        }

        public IntPin(int id, string name, ImNodesPinShape shape, PinKind kind, PinType type, int value, uint maxLinks = uint.MaxValue, PinFlags flags = PinFlags.None) : base(id, name, shape, kind, type, maxLinks)
        {
            this.name = $"{name}##Value{Id.ToString(CultureInfo.InvariantCulture)}";
            nameY = $"##Value1{Id.ToString(CultureInfo.InvariantCulture)}";
            nameZ = $"##Value2{Id.ToString(CultureInfo.InvariantCulture)}";
            nameW = $"##Value3{Id.ToString(CultureInfo.InvariantCulture)}";
            this.flags = flags;
            valueX = value;
            valueY = value;
            valueZ = value;
            valueW = value;
            SanityChecks();
        }

        public int ValueX { get => valueX; set => valueX = value; }

        public int ValueY { get => valueY; set => valueY = value; }

        public int ValueZ { get => valueZ; set => valueZ = value; }

        public int ValueW { get => valueW; set => valueW = value; }

        public PinFlags Flags => flags;

        private void SanityChecks()
        {
            Trace.Assert(Type == PinType.Int || Type == PinType.Int2 || Type == PinType.Int3 || Type == PinType.Int4 || Type == PinType.Int2OrInt || Type == PinType.Int3OrInt || Type == PinType.Int4OrInt, $"PinType {Type} is not a int!");
            Trace.Assert(Flags == PinFlags.None || (Flags & PinFlags.ColorPicker) != 0 || (Flags & PinFlags.ColorEdit) != 0 || (Flags & PinFlags.Slider) != 0 || (Flags & PinFlags.AllowOutput) != 0, $"PinFlags {Flags} is not supported!");
            Trace.Assert((Flags & PinFlags.ColorPicker) == 0 && (Flags & PinFlags.ColorEdit) == 0, $"ColorPicker and ColorEdit is not supported!");
        }

        public string GetDefaultValue()
        {
            return Type switch
            {
                PinType.Int => valueX.ToString(CultureInfo.InvariantCulture),
                PinType.Int2 => $"int2({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)})",
                PinType.Int3 => $"int3({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)},{valueZ.ToString(CultureInfo.InvariantCulture)})",
                PinType.Int4 => $"int4({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)},{valueZ.ToString(CultureInfo.InvariantCulture)},{valueW.ToString(CultureInfo.InvariantCulture)})",
                PinType.Int2OrInt => $"int2({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)})",
                PinType.Int3OrInt => $"int3({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)},{valueZ.ToString(CultureInfo.InvariantCulture)})",
                PinType.Int4OrInt => $"int4({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)},{valueZ.ToString(CultureInfo.InvariantCulture)},{valueW.ToString(CultureInfo.InvariantCulture)})",

                _ => "0",
            };
        }

        protected override unsafe void DrawContent()
        {
            if (Kind == PinKind.Input && Links.Count == 0 || (Flags & PinFlags.AllowOutput) != 0)
            {
                ImGui.PushItemWidth(100);

                if (Type == PinType.Int && flags != PinFlags.Slider && flags != PinFlags.ColorEdit && flags != PinFlags.ColorPicker)
                {
                    if (ImGui.InputInt(name, ref valueX))
                    {
                        OnValueChanging();
                    }
                }
                if (Type == PinType.Int && flags == PinFlags.Slider)
                {
                    if (ImGui.SliderInt(name, ref valueX, 0, 1))
                    {
                        OnValueChanging();
                    }
                }
                if ((Type == PinType.Int2 || Type == PinType.Int2OrInt) && flags != PinFlags.Slider)
                {
                    bool changed = false;
                    changed |= ImGui.InputInt(name, ref valueX);
                    changed |= ImGui.InputInt(nameY, ref valueY);
                    if (changed)
                    {
                        OnValueChanging();
                    }
                }
                if ((Type == PinType.Int2 || Type == PinType.Int2OrInt) && flags == PinFlags.Slider)
                {
                    int* val = stackalloc int[] { valueX, valueY, valueZ };
                    if (ImGui.SliderInt2(name, val, 0, 1))
                    {
                        valueX = val[0];
                        valueY = val[1];
                        OnValueChanging();
                    }
                }
                if ((Type == PinType.Int3 || Type == PinType.Int3OrInt) && flags != PinFlags.Slider && flags != PinFlags.ColorEdit && flags != PinFlags.ColorPicker)
                {
                    bool changed = false;
                    changed |= ImGui.InputInt(name, ref valueX);
                    changed |= ImGui.InputInt(nameY, ref valueY);
                    changed |= ImGui.InputInt(nameZ, ref valueZ);
                    if (changed)
                    {
                        OnValueChanging();
                    }
                }
                if ((Type == PinType.Int3 || Type == PinType.Int3OrInt) && flags == PinFlags.Slider)
                {
                    int* val = stackalloc int[] { valueX, valueY, valueZ };
                    if (ImGui.SliderInt3(name, val, 0, 1))
                    {
                        valueX = val[0];
                        valueY = val[1];
                        valueZ = val[2];
                        OnValueChanging();
                    }
                }
                if ((Type == PinType.Int4 || Type == PinType.Int4OrInt) && flags != PinFlags.Slider && flags != PinFlags.ColorEdit && flags != PinFlags.ColorPicker)
                {
                    bool changed = false;
                    changed |= ImGui.InputInt(name, ref valueX);
                    changed |= ImGui.InputInt(nameY, ref valueY);
                    changed |= ImGui.InputInt(nameZ, ref valueZ);
                    changed |= ImGui.InputInt(nameW, ref valueW);
                    if (changed)
                    {
                        OnValueChanging();
                    }
                }
                if ((Type == PinType.Int4 || Type == PinType.Int4OrInt) && flags == PinFlags.Slider)
                {
                    int* val = stackalloc int[] { valueX, valueY, valueZ, valueW };
                    if (ImGui.SliderInt4(name, val, 0, 1))
                    {
                        valueX = val[0];
                        valueY = val[1];
                        valueZ = val[2];
                        valueW = val[3];
                        OnValueChanging();
                    }
                }

                ImGui.PopItemWidth();
            }
            else
            {
                base.DrawContent();
            }
        }
    }
}