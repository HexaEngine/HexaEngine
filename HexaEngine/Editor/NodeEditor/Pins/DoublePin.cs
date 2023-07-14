namespace HexaEngine.Editor.NodeEditor.Pins
{
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.ImGuiNET;
    using HexaEngine.ImNodesNET;
    using Newtonsoft.Json;
    using System.Diagnostics;
    using System.Globalization;

    public class DoublePin : Pin, IDefaultValuePin
    {
        private readonly string name;
        private readonly string nameY;
        private readonly string nameZ;
        private readonly string nameW;
        private readonly PinFlags flags;
        private double valueX;
        private double valueY;
        private double valueZ;
        private double valueW;
        private bool changing;

        [JsonConstructor]
        public DoublePin(int id, string name, ImNodesPinShape shape, PinKind kind, PinType type, uint maxLinks, PinFlags flags, float valueX, double valueY, double valueZ, double valueW) : base(id, name, shape, kind, type, maxLinks)
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

        public DoublePin(int id, string name, ImNodesPinShape shape, PinKind kind, PinType type, uint maxLinks = uint.MaxValue, PinFlags flags = PinFlags.None) : base(id, name, shape, kind, type, maxLinks)
        {
            this.name = $"{name}##Value{Id.ToString(CultureInfo.InvariantCulture)}";
            nameY = $"##Value1{Id.ToString(CultureInfo.InvariantCulture)}";
            nameZ = $"##Value2{Id.ToString(CultureInfo.InvariantCulture)}";
            nameW = $"##Value3{Id.ToString(CultureInfo.InvariantCulture)}";
            this.flags = flags;
            SanityChecks();
        }

        public DoublePin(int id, string name, ImNodesPinShape shape, PinKind kind, PinType type, double value, uint maxLinks = uint.MaxValue, PinFlags flags = PinFlags.None) : base(id, name, shape, kind, type, maxLinks)
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

        public double ValueX { get => valueX; set => valueX = value; }

        public double ValueY { get => valueY; set => valueY = value; }

        public double ValueZ { get => valueZ; set => valueZ = value; }

        public double ValueW { get => valueW; set => valueW = value; }

        public PinFlags Flags => flags;

        private void SanityChecks()
        {
            Trace.Assert(Type == PinType.Double || Type == PinType.Double2 || Type == PinType.Double3 || Type == PinType.Double4 || Type == PinType.Double2OrDouble || Type == PinType.Double3OrDouble || Type == PinType.Double4OrDouble, $"PinType {Type} is not a double!");
            Trace.Assert(Flags == PinFlags.None || (Flags & PinFlags.ColorPicker) != 0 || (Flags & PinFlags.ColorEdit) != 0 || (Flags & PinFlags.Slider) != 0 || (Flags & PinFlags.AllowOutput) != 0, $"PinFlags {Flags} is not supported!");
            Trace.Assert((Flags & PinFlags.ColorPicker) == 0 && (Flags & PinFlags.ColorEdit) == 0, $"ColorPicker and ColorEdit is not supported!");
        }

        public string GetDefaultValue()
        {
            return Type switch
            {
                PinType.Float => valueX.ToString(CultureInfo.InvariantCulture),
                PinType.Float2 => $"double2({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)})",
                PinType.Float3 => $"double3({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)},{valueZ.ToString(CultureInfo.InvariantCulture)})",
                PinType.Float4 => $"double4({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)},{valueZ.ToString(CultureInfo.InvariantCulture)},{valueW.ToString(CultureInfo.InvariantCulture)})",
                PinType.Float2OrFloat => $"double2({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)})",
                PinType.Float3OrFloat => $"double3({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)},{valueZ.ToString(CultureInfo.InvariantCulture)})",
                PinType.Float4OrFloat => $"double4({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)},{valueZ.ToString(CultureInfo.InvariantCulture)},{valueW.ToString(CultureInfo.InvariantCulture)})",

                _ => "0",
            };
        }

        protected override unsafe void DrawContent()
        {
            double* val = stackalloc double[4] { valueX, valueY, valueZ, valueW };
            double min = 0;
            double max = 1;
            IsActive = false;
            if (Kind == PinKind.Input && Links.Count == 0 || (Flags & PinFlags.AllowOutput) != 0)
            {
                ImGui.PushItemWidth(100);

                if (Type == PinType.Double && flags != PinFlags.Slider && flags != PinFlags.ColorEdit && flags != PinFlags.ColorPicker)
                {
                    if (ImGui.InputDouble(name, ref valueX))
                    {
                        OnValueChanging();
                        changing = true;
                    }
                    IsActive |= ImGui.IsItemActive();
                }
                if (Type == PinType.Double && flags == PinFlags.Slider)
                {
                    if (ImGui.SliderScalarN(name, ImGuiDataType.Double, val, 1, &min, &max))
                    {
                        valueX = val[0];
                        OnValueChanging();
                        changing = true;
                    }
                    IsActive |= ImGui.IsItemActive();
                }
                if ((Type == PinType.Double2 || Type == PinType.Double2OrDouble) && flags != PinFlags.Slider)
                {
                    bool changed = false;
                    changed |= ImGui.InputDouble(name, ref valueX);
                    IsActive |= ImGui.IsItemActive();
                    changed |= ImGui.InputDouble(nameY, ref valueY);
                    IsActive |= ImGui.IsItemActive();
                    if (changed)
                    {
                        OnValueChanging();
                        changing = true;
                    }
                }
                if ((Type == PinType.Double2 || Type == PinType.Double2OrDouble) && flags == PinFlags.Slider)
                {
                    if (ImGui.SliderScalarN(name, ImGuiDataType.Double, val, 2, &min, &max))
                    {
                        valueX = val[0];
                        valueY = val[1];
                        OnValueChanging();
                        changing = true;
                    }
                    IsActive |= ImGui.IsItemActive();
                }
                if ((Type == PinType.Double3 || Type == PinType.Double3OrDouble) && flags != PinFlags.Slider && flags != PinFlags.ColorEdit && flags != PinFlags.ColorPicker)
                {
                    bool changed = false;
                    changed |= ImGui.InputDouble(name, ref valueX);
                    IsActive |= ImGui.IsItemActive();
                    changed |= ImGui.InputDouble(nameY, ref valueY);
                    IsActive |= ImGui.IsItemActive();
                    changed |= ImGui.InputDouble(nameZ, ref valueZ);
                    IsActive |= ImGui.IsItemActive();
                    if (changed)
                    {
                        OnValueChanging();
                        changing = true;
                    }
                }
                if ((Type == PinType.Double3 || Type == PinType.Double3OrDouble) && flags == PinFlags.Slider)
                {
                    if (ImGui.SliderScalarN(name, ImGuiDataType.Double, val, 3, &min, &max))
                    {
                        valueX = val[0];
                        valueY = val[1];
                        valueZ = val[2];
                        OnValueChanging();
                        changing = true;
                    }
                    IsActive |= ImGui.IsItemActive();
                }
                if ((Type == PinType.Double4 || Type == PinType.Double4OrDouble) && flags != PinFlags.Slider && flags != PinFlags.ColorEdit && flags != PinFlags.ColorPicker)
                {
                    bool changed = false;
                    changed |= ImGui.InputDouble(name, ref valueX);
                    IsActive |= ImGui.IsItemActive();
                    changed |= ImGui.InputDouble(nameY, ref valueY);
                    IsActive |= ImGui.IsItemActive();
                    changed |= ImGui.InputDouble(nameZ, ref valueZ);
                    IsActive |= ImGui.IsItemActive();
                    changed |= ImGui.InputDouble(nameW, ref valueW);
                    IsActive |= ImGui.IsItemActive();
                    if (changed)
                    {
                        OnValueChanging();
                        changing = true;
                    }
                }
                if ((Type == PinType.Double4 || Type == PinType.Double4OrDouble) && flags == PinFlags.Slider)
                {
                    if (ImGui.SliderScalarN(name, ImGuiDataType.Double, val, 4, &min, &max))
                    {
                        valueX = val[0];
                        valueY = val[1];
                        valueZ = val[2];
                        valueW = val[3];
                        OnValueChanging();
                        changing = true;
                    }
                    IsActive |= ImGui.IsItemActive();
                }

                ImGui.PopItemWidth();
            }
            else
            {
                base.DrawContent();
            }

            if (changing && !IsActive)
            {
                changing = false;
                OnValueChanged();
            }
        }
    }
}