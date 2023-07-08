namespace HexaEngine.Editor.NodeEditor.Pins
{
    using HexaEngine.Editor.MaterialEditor.Nodes;
    using HexaEngine.ImGuiNET;
    using HexaEngine.ImNodesNET;
    using System.Globalization;
    using System.Numerics;

    public class FloatPin : Pin, IDefaultValuePin
    {
        private readonly string name;
        private readonly string nameY;
        private readonly string nameZ;
        private readonly string nameW;
        private readonly PinFlags flags;
        private float valueX;
        private float valueY;
        private float valueZ;
        private float valueW;

        [JsonConstructor]
        public FloatPin(int id, string name, ImNodesPinShape shape, PinKind kind, PinType type, uint maxLinks, PinFlags flags, float valueX, float valueY, float valueZ, float valueW) : base(id, name, shape, kind, type, maxLinks)
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
        }

        public FloatPin(int id, string name, ImNodesPinShape shape, PinKind kind, PinType type, uint maxLinks = uint.MaxValue, PinFlags flags = PinFlags.None) : base(id, name, shape, kind, type, maxLinks)
        {
            this.name = $"{name}##Value{Id.ToString(CultureInfo.InvariantCulture)}";
            nameY = $"##Value1{Id.ToString(CultureInfo.InvariantCulture)}";
            nameZ = $"##Value2{Id.ToString(CultureInfo.InvariantCulture)}";
            nameW = $"##Value3{Id.ToString(CultureInfo.InvariantCulture)}";
            this.flags = flags;
        }

        public FloatPin(int id, string name, ImNodesPinShape shape, PinKind kind, PinType type, Vector4 value, uint maxLinks = uint.MaxValue, PinFlags flags = PinFlags.None) : base(id, name, shape, kind, type, maxLinks)
        {
            this.name = $"{name}##Value{Id.ToString(CultureInfo.InvariantCulture)}";
            nameY = $"##Value1{Id.ToString(CultureInfo.InvariantCulture)}";
            nameZ = $"##Value2{Id.ToString(CultureInfo.InvariantCulture)}";
            nameW = $"##Value3{Id.ToString(CultureInfo.InvariantCulture)}";
            this.flags = flags;
            valueX = value.X;
            valueY = value.Y;
            valueZ = value.Z;
            valueW = value.W;
        }

        public float ValueX { get => valueX; set => valueX = value; }

        public float ValueY { get => valueY; set => valueY = value; }

        public float ValueZ { get => valueZ; set => valueZ = value; }

        public float ValueW { get => valueW; set => valueW = value; }

        public PinFlags Flags => flags;

        public string GetDefaultValue()
        {
            return Type switch
            {
                PinType.Float => valueX.ToString(CultureInfo.InvariantCulture),
                PinType.Float2 => $"float2({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)})",
                PinType.Float3 => $"float3({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)},{valueZ.ToString(CultureInfo.InvariantCulture)})",
                PinType.Float4 => $"float4({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)},{valueZ.ToString(CultureInfo.InvariantCulture)},{valueW.ToString(CultureInfo.InvariantCulture)})",
                _ => "0",
            };
        }

        protected override void DrawContent()
        {
            if (Kind == PinKind.Input && Links.Count == 0)
            {
                ImGui.PushItemWidth(100);

                if (Type == PinType.Float && flags == PinFlags.None)
                {
                    ImGui.InputFloat(name, ref valueX);
                }
                if (Type == PinType.Float && flags == PinFlags.Slider)
                {
                    ImGui.SliderFloat(name, ref valueX, 0, 1);
                }
                if (Type == PinType.Float2 && flags == PinFlags.None)
                {
                    ImGui.InputFloat(name, ref valueX);
                    ImGui.InputFloat(nameY, ref valueY);
                }
                if (Type == PinType.Float2 && flags == PinFlags.Slider)
                {
                    var val = new Vector2(ValueX, ValueY);
                    if (ImGui.SliderFloat2(name, ref val, 0, 1))
                    {
                        valueX = val.X;
                        valueY = val.Y;
                    }
                }
                if (Type == PinType.Float3 && flags == PinFlags.None)
                {
                    ImGui.InputFloat(name, ref valueX);
                    ImGui.InputFloat(nameY, ref valueY);
                    ImGui.InputFloat(nameZ, ref valueZ);
                }
                if (Type == PinType.Float3 && flags == PinFlags.ColorEdit)
                {
                    var val = new Vector3(valueX, valueY, valueZ);
                    if (ImGui.ColorEdit3(name, ref val, ImGuiColorEditFlags.DisplayHex | ImGuiColorEditFlags.Hdr))
                    {
                        valueX = val.X;
                        valueY = val.Y;
                        valueZ = val.Z;
                    }
                }
                if (Type == PinType.Float3 && flags == PinFlags.ColorPicker)
                {
                    var val = new Vector3(valueX, valueY, valueZ);
                    if (ImGui.ColorPicker3(name, ref val, ImGuiColorEditFlags.DisplayHex | ImGuiColorEditFlags.Hdr))
                    {
                        valueX = val.X;
                        valueY = val.Y;
                        valueZ = val.Z;
                    }
                }
                if (Type == PinType.Float4 && flags == PinFlags.Slider)
                {
                    var val = new Vector3(valueX, valueY, valueZ);
                    if (ImGui.SliderFloat3(name, ref val, 0, 1))
                    {
                        valueX = val.X;
                        valueY = val.Y;
                        valueZ = val.Z;
                    }
                }
                if (Type == PinType.Float4 && flags == PinFlags.None)
                {
                    ImGui.InputFloat(name, ref valueX);
                    ImGui.InputFloat(nameY, ref valueY);
                    ImGui.InputFloat(nameZ, ref valueZ);
                    ImGui.InputFloat(nameW, ref valueW);
                }
                if (Type == PinType.Float4 && flags == PinFlags.ColorEdit)
                {
                    var val = new Vector4(valueX, valueY, valueZ, valueW);
                    if (ImGui.ColorEdit4(name, ref val, ImGuiColorEditFlags.DisplayHex | ImGuiColorEditFlags.Hdr))
                    {
                        valueX = val.X;
                        valueY = val.Y;
                        valueZ = val.Z;
                        valueW = val.W;
                    }
                }
                if (Type == PinType.Float4 && flags == PinFlags.ColorPicker)
                {
                    var val = new Vector4(valueX, valueY, valueZ, valueW);
                    if (ImGui.ColorPicker4(name, ref val, ImGuiColorEditFlags.DisplayHex | ImGuiColorEditFlags.Hdr))
                    {
                        valueX = val.X;
                        valueY = val.Y;
                        valueZ = val.Z;
                        valueW = val.W;
                    }
                }
                if (Type == PinType.Float4 && flags == PinFlags.Slider)
                {
                    var val = new Vector4(valueX, valueY, valueZ, valueW);
                    if (ImGui.SliderFloat4(name, ref val, 0, 1))
                    {
                        valueX = val.X;
                        valueY = val.Y;
                        valueZ = val.Z;
                        valueW = val.W;
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