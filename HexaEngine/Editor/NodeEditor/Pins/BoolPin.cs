namespace HexaEngine.Editor.NodeEditor.Pins
{
    using HexaEngine.Editor.NodeEditor;
    using Hexa.NET.ImGui;
    using Hexa.NET.ImNodes;
    using Newtonsoft.Json;
    using System.Diagnostics;
    using System.Globalization;

    public class BoolPin : Pin, IDefaultValuePin
    {
        private readonly string name;
        private readonly string nameY;
        private readonly string nameZ;
        private readonly string nameW;
        private readonly PinFlags flags;
        private bool valueX;
        private bool valueY;
        private bool valueZ;
        private bool valueW;
        private bool changing;

        [JsonConstructor]
        public BoolPin(int id, string name, ImNodesPinShape shape, PinKind kind, PinType type, uint maxLinks, PinFlags flags, bool valueX, bool valueY, bool valueZ, bool valueW) : base(id, name, shape, kind, type, maxLinks)
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

        public BoolPin(int id, string name, ImNodesPinShape shape, PinKind kind, PinType type, uint maxLinks = uint.MaxValue, PinFlags flags = PinFlags.None) : base(id, name, shape, kind, type, maxLinks)
        {
            this.name = $"{name}##Value{Id.ToString(CultureInfo.InvariantCulture)}";
            nameY = $"##Value1{Id.ToString(CultureInfo.InvariantCulture)}";
            nameZ = $"##Value2{Id.ToString(CultureInfo.InvariantCulture)}";
            nameW = $"##Value3{Id.ToString(CultureInfo.InvariantCulture)}";
            this.flags = flags;
            SanityChecks();
        }

        public BoolPin(int id, string name, ImNodesPinShape shape, PinKind kind, PinType type, bool value, uint maxLinks = uint.MaxValue, PinFlags flags = PinFlags.None) : base(id, name, shape, kind, type, maxLinks)
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

        public bool ValueX { get => valueX; set => valueX = value; }

        public bool ValueY { get => valueY; set => valueY = value; }

        public bool ValueZ { get => valueZ; set => valueZ = value; }

        public bool ValueW { get => valueW; set => valueW = value; }

        public PinFlags Flags => flags;

        private void SanityChecks()
        {
            Trace.Assert(Type == PinType.Bool || Type == PinType.Bool2 || Type == PinType.Bool3 || Type == PinType.Bool4 || Type == PinType.Bool2OrBool || Type == PinType.Bool3OrBool || Type == PinType.Bool4OrBool, $"PinType {Type} is not a bool!");
            Trace.Assert(Flags == PinFlags.None || (Flags & PinFlags.ColorPicker) != 0 || (Flags & PinFlags.ColorEdit) != 0 || (Flags & PinFlags.Slider) != 0 || (Flags & PinFlags.AllowOutput) != 0, $"PinFlags {Flags} is not supported!");
            Trace.Assert((Flags & PinFlags.ColorPicker) == 0 && (Flags & PinFlags.ColorEdit) == 0 && (Flags & PinFlags.Slider) == 0, $"ColorPicker and ColorEdit is not supported!");
        }

        public string GetDefaultValue()
        {
            return Type switch
            {
                PinType.Bool => valueX.ToString(CultureInfo.InvariantCulture),
                PinType.Bool2 => $"bool2({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)})",
                PinType.Bool3 => $"bool3({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)},{valueZ.ToString(CultureInfo.InvariantCulture)})",
                PinType.Bool4 => $"bool4({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)},{valueZ.ToString(CultureInfo.InvariantCulture)},{valueW.ToString(CultureInfo.InvariantCulture)})",
                PinType.Bool2OrBool => $"bool2({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)})",
                PinType.Bool3OrBool => $"bool3({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)},{valueZ.ToString(CultureInfo.InvariantCulture)})",
                PinType.Bool4OrBool => $"bool4({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)},{valueZ.ToString(CultureInfo.InvariantCulture)},{valueW.ToString(CultureInfo.InvariantCulture)})",

                _ => "0",
            };
        }

        protected override unsafe void DrawContent()
        {
            IsActive = false;
            bool* val = stackalloc bool[4] { valueX, valueY, valueZ, valueW };
            if (Kind == PinKind.Input && Links.Count == 0 || (Flags & PinFlags.AllowOutput) != 0)
            {
                ImGui.PushItemWidth(100);

                if (Type == PinType.Bool && flags != PinFlags.Slider && flags != PinFlags.ColorEdit && flags != PinFlags.ColorPicker)
                {
                    if (ImGui.Checkbox(name, val))
                    {
                        valueX = val[0];
                        OnValueChanging();
                        changing = true;
                    }
                    IsActive |= ImGui.IsItemActive();
                }
                if ((Type == PinType.Bool || Type == PinType.Bool2OrBool) && flags != PinFlags.Slider)
                {
                    bool changed = false;
                    changed |= ImGui.Checkbox(name, val);
                    IsActive |= ImGui.IsItemActive();
                    changed |= ImGui.Checkbox(nameY, &val[1]);
                    IsActive |= ImGui.IsItemActive();
                    if (changed)
                    {
                        valueX = val[0];
                        valueY = val[1];
                        OnValueChanging();
                        changing = true;
                    }
                }
                if ((Type == PinType.Bool3 || Type == PinType.Bool3OrBool) && flags != PinFlags.Slider && flags != PinFlags.ColorEdit && flags != PinFlags.ColorPicker)
                {
                    bool changed = false;
                    changed |= ImGui.Checkbox(name, val);
                    IsActive |= ImGui.IsItemActive();
                    changed |= ImGui.Checkbox(nameY, &val[1]);
                    IsActive |= ImGui.IsItemActive();
                    changed |= ImGui.Checkbox(nameZ, &val[2]);
                    IsActive |= ImGui.IsItemActive();
                    if (changed)
                    {
                        valueX = val[0];
                        valueY = val[1];
                        valueZ = val[2];
                        OnValueChanging();
                        changing = true;
                    }
                }
                if ((Type == PinType.Bool4 || Type == PinType.Bool4OrBool) && flags != PinFlags.Slider && flags != PinFlags.ColorEdit && flags != PinFlags.ColorPicker)
                {
                    bool changed = false;
                    changed |= ImGui.Checkbox(name, val);
                    IsActive |= ImGui.IsItemActive();
                    changed |= ImGui.Checkbox(nameY, &val[1]);
                    IsActive |= ImGui.IsItemActive();
                    changed |= ImGui.Checkbox(nameZ, &val[2]);
                    IsActive |= ImGui.IsItemActive();
                    changed |= ImGui.Checkbox(nameW, &val[3]);
                    IsActive |= ImGui.IsItemActive();
                    if (changed)
                    {
                        valueX = val[0];
                        valueY = val[1];
                        valueZ = val[2];
                        valueW = val[3];
                        OnValueChanging();
                        changing = true;
                    }
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