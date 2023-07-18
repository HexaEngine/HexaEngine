namespace HexaEngine.Editor.NodeEditor.Pins
{
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.ImGuiNET;
    using HexaEngine.ImNodesNET;
    using Newtonsoft.Json;
    using System.Diagnostics;
    using System.Globalization;

    public class UIntPin : Pin, IDefaultValuePin
    {
        private readonly string name;
        private readonly string nameY;
        private readonly string nameZ;
        private readonly string nameW;
        private readonly PinFlags flags;
        private uint valueX;
        private uint valueY;
        private uint valueZ;
        private uint valueW;

        [JsonConstructor]
        public UIntPin(int id, string name, ImNodesPinShape shape, PinKind kind, PinType type, uint maxLinks, PinFlags flags, uint valueX, uint valueY, uint valueZ, uint valueW) : base(id, name, shape, kind, type, maxLinks)
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

        public UIntPin(int id, string name, ImNodesPinShape shape, PinKind kind, PinType type, uint maxLinks = uint.MaxValue, PinFlags flags = PinFlags.None) : base(id, name, shape, kind, type, maxLinks)
        {
            this.name = $"{name}##Value{Id.ToString(CultureInfo.InvariantCulture)}";
            nameY = $"##Value1{Id.ToString(CultureInfo.InvariantCulture)}";
            nameZ = $"##Value2{Id.ToString(CultureInfo.InvariantCulture)}";
            nameW = $"##Value3{Id.ToString(CultureInfo.InvariantCulture)}";
            this.flags = flags;
            SanityChecks();
        }

        public UIntPin(int id, string name, ImNodesPinShape shape, PinKind kind, PinType type, uint value, uint maxLinks = uint.MaxValue, PinFlags flags = PinFlags.None) : base(id, name, shape, kind, type, maxLinks)
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

        public uint ValueX { get => valueX; set => valueX = value; }

        public uint ValueY { get => valueY; set => valueY = value; }

        public uint ValueZ { get => valueZ; set => valueZ = value; }

        public uint ValueW { get => valueW; set => valueW = value; }

        public PinFlags Flags => flags;

        private void SanityChecks()
        {
            Trace.Assert(Type == PinType.UInt || Type == PinType.UInt2 || Type == PinType.UInt3 || Type == PinType.UInt4 || Type == PinType.UInt2OrUInt || Type == PinType.UInt3OrUInt || Type == PinType.UInt4OrUInt, $"PinType {Type} is not a uint!");
            Trace.Assert(Flags == PinFlags.None || (Flags & PinFlags.ColorPicker) != 0 || (Flags & PinFlags.ColorEdit) != 0 || (Flags & PinFlags.Slider) != 0 || (Flags & PinFlags.AllowOutput) != 0, $"PinFlags {Flags} is not supported!");
            Trace.Assert((Flags & PinFlags.ColorPicker) == 0 && (Flags & PinFlags.ColorEdit) == 0, $"ColorPicker and ColorEdit is not supported!");
        }

        public string GetDefaultValue()
        {
            return Type switch
            {
                PinType.Int => valueX.ToString(CultureInfo.InvariantCulture),
                PinType.Int2 => $"uint2({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)})",
                PinType.Int3 => $"uint3({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)},{valueZ.ToString(CultureInfo.InvariantCulture)})",
                PinType.Int4 => $"uint4({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)},{valueZ.ToString(CultureInfo.InvariantCulture)},{valueW.ToString(CultureInfo.InvariantCulture)})",
                PinType.Int2OrInt => $"uint2({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)})",
                PinType.Int3OrInt => $"uint3({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)},{valueZ.ToString(CultureInfo.InvariantCulture)})",
                PinType.Int4OrInt => $"uint4({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)},{valueZ.ToString(CultureInfo.InvariantCulture)},{valueW.ToString(CultureInfo.InvariantCulture)})",

                _ => "0",
            };
        }

        protected override unsafe void DrawContent()
        {
            uint* val = stackalloc uint[4] { valueX, valueY, valueZ, valueW };
            uint min = 0;
            uint max = 1;
            if (Kind == PinKind.Input && Links.Count == 0 || (Flags & PinFlags.AllowOutput) != 0)
            {
                ImGui.PushItemWidth(100);

                if (Type == PinType.UInt && flags != PinFlags.Slider && flags != PinFlags.ColorEdit && flags != PinFlags.ColorPicker)
                {
                    if (ImGui.InputScalar(name, ImGuiDataType.U32, val))
                    {
                        valueX = val[0];
                        OnValueChanging();
                    }
                }
                if (Type == PinType.UInt && flags == PinFlags.Slider)
                {
                    if (ImGui.SliderScalarN(name, ImGuiDataType.U32, val, 1, &min, &max))
                    {
                        valueX = val[0];
                        OnValueChanging();
                    }
                }
                if ((Type == PinType.UInt2 || Type == PinType.UInt2OrUInt) && flags != PinFlags.Slider)
                {
                    bool changed = false;
                    changed |= ImGui.InputScalar(name, ImGuiDataType.U32, val);
                    changed |= ImGui.InputScalar(nameY, ImGuiDataType.U32, &val[1]);
                    if (changed)
                    {
                        valueX = val[0];
                        valueY = val[1];
                        OnValueChanging();
                    }
                }
                if ((Type == PinType.UInt2 || Type == PinType.UInt2OrUInt) && flags == PinFlags.Slider)
                {
                    if (ImGui.SliderScalarN(name, ImGuiDataType.U32, val, 2, &min, &max))
                    {
                        valueX = val[0];
                        valueY = val[1];
                        OnValueChanging();
                    }
                }
                if ((Type == PinType.UInt3 || Type == PinType.UInt3OrUInt) && flags != PinFlags.Slider && flags != PinFlags.ColorEdit && flags != PinFlags.ColorPicker)
                {
                    bool changed = false;
                    changed |= ImGui.InputScalar(name, ImGuiDataType.U32, val);
                    changed |= ImGui.InputScalar(nameY, ImGuiDataType.U32, &val[1]);
                    changed |= ImGui.InputScalar(nameZ, ImGuiDataType.U32, &val[2]);
                    if (changed)
                    {
                        valueX = val[0];
                        valueY = val[1];
                        valueZ = val[2];
                        OnValueChanging();
                    }
                }
                if ((Type == PinType.UInt3 || Type == PinType.UInt3OrUInt) && flags == PinFlags.Slider)
                {
                    if (ImGui.SliderScalarN(name, ImGuiDataType.U32, val, 3, &min, &max))
                    {
                        valueX = val[0];
                        valueY = val[1];
                        valueZ = val[2];
                        OnValueChanging();
                    }
                }
                if ((Type == PinType.UInt4 || Type == PinType.UInt4OrUInt) && flags != PinFlags.Slider && flags != PinFlags.ColorEdit && flags != PinFlags.ColorPicker)
                {
                    bool changed = false;
                    changed |= ImGui.InputScalar(name, ImGuiDataType.U32, val);
                    changed |= ImGui.InputScalar(nameY, ImGuiDataType.U32, &val[1]);
                    changed |= ImGui.InputScalar(nameZ, ImGuiDataType.U32, &val[2]);
                    changed |= ImGui.InputScalar(nameW, ImGuiDataType.U32, &val[3]);
                    if (changed)
                    {
                        valueX = val[0];
                        valueY = val[1];
                        valueZ = val[2];
                        valueW = val[3];
                        OnValueChanging();
                    }
                }
                if ((Type == PinType.UInt4 || Type == PinType.UInt4OrUInt) && flags == PinFlags.Slider)
                {
                    if (ImGui.SliderScalarN(name, ImGuiDataType.U32, val, 4, &min, &max))
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