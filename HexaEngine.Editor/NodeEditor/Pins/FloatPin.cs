namespace HexaEngine.Editor.NodeEditor.Pins
{
    using HexaEngine.Editor.NodeEditor;
    using Hexa.NET.ImGui;
    using Hexa.NET.ImNodes;
    using Newtonsoft.Json;
    using System.Diagnostics;
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
        private bool changed;
        private string? defaultExpression;

        [JsonConstructor]
        public FloatPin(int id, string name, ImNodesPinShape shape, PinKind kind, PinType type, uint maxLinks, PinFlags flags, float valueX, float valueY, float valueZ, float valueW, string defaultExpression) : base(id, name, shape, kind, type, maxLinks)
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
            this.defaultExpression = defaultExpression;
            SanityChecks();
        }

        public FloatPin(int id, string name, ImNodesPinShape shape, PinKind kind, PinType type, uint maxLinks = uint.MaxValue, PinFlags flags = PinFlags.None, string? defaultExpression = null) : base(id, name, shape, kind, type, maxLinks)
        {
            this.name = $"{name}##Value{Id.ToString(CultureInfo.InvariantCulture)}";
            nameY = $"##Value1{Id.ToString(CultureInfo.InvariantCulture)}";
            nameZ = $"##Value2{Id.ToString(CultureInfo.InvariantCulture)}";
            nameW = $"##Value3{Id.ToString(CultureInfo.InvariantCulture)}";
            this.defaultExpression = defaultExpression;
            this.flags = flags;
            SanityChecks();
        }

        public FloatPin(int id, string name, ImNodesPinShape shape, PinKind kind, PinType type, Vector4 value, uint maxLinks = uint.MaxValue, PinFlags flags = PinFlags.None, string? defaultExpression = null) : base(id, name, shape, kind, type, maxLinks)
        {
            this.name = $"{name}##Value{Id.ToString(CultureInfo.InvariantCulture)}";
            nameY = $"##Value1{Id.ToString(CultureInfo.InvariantCulture)}";
            nameZ = $"##Value2{Id.ToString(CultureInfo.InvariantCulture)}";
            nameW = $"##Value3{Id.ToString(CultureInfo.InvariantCulture)}";
            this.defaultExpression = defaultExpression;
            this.flags = flags;
            valueX = value.X;
            valueY = value.Y;
            valueZ = value.Z;
            valueW = value.W;
            SanityChecks();
        }

        public float ValueX { get => valueX; set => valueX = value; }

        public float ValueY { get => valueY; set => valueY = value; }

        public float ValueZ { get => valueZ; set => valueZ = value; }

        public float ValueW { get => valueW; set => valueW = value; }

        [JsonIgnore]
        public Vector2 Vector2 => new(ValueX, ValueY);

        [JsonIgnore]
        public Vector3 Vector3 => new(ValueX, ValueY, ValueZ);

        [JsonIgnore]
        public Vector4 Vector4 => new(ValueX, ValueY, ValueZ, ValueW);

        public PinFlags Flags => flags;

        public string? DefaultExpression { get => defaultExpression; set => defaultExpression = value; }

        private void SanityChecks()
        {
            Trace.Assert(Type == PinType.Float || Type == PinType.Float2 || Type == PinType.Float3 || Type == PinType.Float4 || Type == PinType.Float2OrFloat || Type == PinType.Float3OrFloat || Type == PinType.Float4OrFloat, $"PinType {Type} is not a float!");
            Trace.Assert(Flags == PinFlags.None || (Flags & PinFlags.ColorPicker) != 0 || (Flags & PinFlags.ColorEdit) != 0 || (Flags & PinFlags.Slider) != 0 || (Flags & PinFlags.AllowOutput) != 0, $"PinFlags {Flags} is not supported!");
        }

        public string GetDefaultValue()
        {
            if (defaultExpression != null)
                return defaultExpression;
            return Type switch
            {
                PinType.Float => valueX.ToString(CultureInfo.InvariantCulture),
                PinType.Float2 => $"float2({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)})",
                PinType.Float3 => $"float3({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)},{valueZ.ToString(CultureInfo.InvariantCulture)})",
                PinType.Float4 => $"float4({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)},{valueZ.ToString(CultureInfo.InvariantCulture)},{valueW.ToString(CultureInfo.InvariantCulture)})",
                PinType.Float2OrFloat => $"float2({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)})",
                PinType.Float3OrFloat => $"float3({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)},{valueZ.ToString(CultureInfo.InvariantCulture)})",
                PinType.Float4OrFloat => $"float4({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)},{valueZ.ToString(CultureInfo.InvariantCulture)},{valueW.ToString(CultureInfo.InvariantCulture)})",

                _ => "0",
            };
        }

        protected override unsafe void DrawContent()
        {
            if (defaultExpression != null)
            {
                base.DrawContent();
                return;
            }

            float* val = stackalloc float[4] { valueX, valueY, valueZ, valueW };

            IsActive = false;
            if (Kind == PinKind.Input && Links.Count == 0 || (Flags & PinFlags.AllowOutput) != 0)
            {
                ImGui.PushItemWidth(100);

                if (Type == PinType.Float && flags != PinFlags.Slider && flags != PinFlags.ColorEdit && flags != PinFlags.ColorPicker)
                {
                    if (ImGui.InputFloat(name, val))
                    {
                        valueX = val[0];
                        OnValueChanging();
                        changed = true;
                    }
                    IsActive |= ImGui.IsItemActive();
                }
                if (Type == PinType.Float && flags == PinFlags.Slider)
                {
                    if (ImGui.SliderFloat(name, val, 0, 1))
                    {
                        valueX = val[0];
                        OnValueChanging();
                        changed = true;
                    }
                    IsActive |= ImGui.IsItemActive();
                }
                if ((Type == PinType.Float2 || Type == PinType.Float2OrFloat) && flags != PinFlags.Slider)
                {
                    bool changed = false;
                    changed |= ImGui.InputFloat(name, &val[0]);
                    IsActive |= ImGui.IsItemActive();
                    changed |= ImGui.InputFloat(nameY, &val[1]);
                    IsActive |= ImGui.IsItemActive();
                    if (changed)
                    {
                        valueX = val[0];
                        valueY = val[1];
                        OnValueChanging();
                        this.changed = true;
                    }
                }
                if ((Type == PinType.Float2 || Type == PinType.Float2OrFloat) && flags == PinFlags.Slider)
                {
                    if (ImGui.SliderFloat2(name, val, 0, 1))
                    {
                        valueX = val[0];
                        valueY = val[1];
                        OnValueChanging();
                        changed = true;
                    }
                    IsActive |= ImGui.IsItemActive();
                }
                if ((Type == PinType.Float3 || Type == PinType.Float3OrFloat) && flags != PinFlags.Slider && flags != PinFlags.ColorEdit && flags != PinFlags.ColorPicker)
                {
                    bool changed = false;
                    changed |= ImGui.InputFloat(name, &val[0]);
                    IsActive |= ImGui.IsItemActive();
                    changed |= ImGui.InputFloat(nameY, &val[1]);
                    IsActive |= ImGui.IsItemActive();
                    changed |= ImGui.InputFloat(nameZ, &val[2]);
                    IsActive |= ImGui.IsItemActive();
                    if (changed)
                    {
                        valueX = val[0];
                        valueY = val[1];
                        valueZ = val[2];
                        OnValueChanging();
                        this.changed = true;
                    }
                }
                if ((Type == PinType.Float3 || Type == PinType.Float3OrFloat) && flags == PinFlags.ColorEdit)
                {
                    if (ImGui.ColorEdit3(name, val, ImGuiColorEditFlags.DisplayHex | ImGuiColorEditFlags.Hdr))
                    {
                        valueX = val[0];
                        valueY = val[1];
                        valueZ = val[2];
                        OnValueChanging();
                        changed = true;
                    }
                    IsActive |= ImGui.IsItemActive();
                }
                if ((Type == PinType.Float3 || Type == PinType.Float3OrFloat) && flags == PinFlags.ColorPicker)
                {
                    if (ImGui.ColorPicker3(name, val, ImGuiColorEditFlags.DisplayHex | ImGuiColorEditFlags.Hdr))
                    {
                        valueX = val[0];
                        valueY = val[1];
                        valueZ = val[2];
                        OnValueChanging();
                        changed = true;
                    }
                    IsActive |= ImGui.IsItemActive();
                }
                if ((Type == PinType.Float3 || Type == PinType.Float3OrFloat) && flags == PinFlags.Slider)
                {
                    if (ImGui.SliderFloat3(name, val, 0, 1))
                    {
                        valueX = val[0];
                        valueY = val[1];
                        valueZ = val[2];
                        OnValueChanging();
                        changed = true;
                    }
                    IsActive |= ImGui.IsItemActive();
                }
                if ((Type == PinType.Float4 || Type == PinType.Float4OrFloat) && flags != PinFlags.Slider && flags != PinFlags.ColorEdit && flags != PinFlags.ColorPicker)
                {
                    bool changed = false;
                    changed |= ImGui.InputFloat(name, &val[0]);
                    IsActive |= ImGui.IsItemActive();
                    changed |= ImGui.InputFloat(nameY, &val[1]);
                    IsActive |= ImGui.IsItemActive();
                    changed |= ImGui.InputFloat(nameZ, &val[2]);
                    IsActive |= ImGui.IsItemActive();
                    changed |= ImGui.InputFloat(nameW, &val[3]);
                    IsActive |= ImGui.IsItemActive();
                    if (changed)
                    {
                        valueX = val[0];
                        valueY = val[1];
                        valueZ = val[2];
                        valueW = val[3];
                        OnValueChanging();
                        this.changed = true;
                    }
                }
                if ((Type == PinType.Float4 || Type == PinType.Float4OrFloat) && flags == PinFlags.ColorEdit)
                {
                    if (ImGui.ColorEdit4(name, val, ImGuiColorEditFlags.DisplayHex | ImGuiColorEditFlags.Hdr))
                    {
                        valueX = val[0];
                        valueY = val[1];
                        valueZ = val[2];
                        valueW = val[3];
                        OnValueChanging();
                        changed = true;
                    }
                    IsActive |= ImGui.IsItemActive();
                }
                if ((Type == PinType.Float4 || Type == PinType.Float4OrFloat) && flags == PinFlags.ColorPicker)
                {
                    if (ImGui.ColorPicker4(name, val, ImGuiColorEditFlags.DisplayHex | ImGuiColorEditFlags.Hdr))
                    {
                        valueX = val[0];
                        valueY = val[1];
                        valueZ = val[2];
                        valueW = val[3];
                        OnValueChanging();
                        changed = true;
                    }
                    IsActive |= ImGui.IsItemActive();
                }
                if ((Type == PinType.Float4 || Type == PinType.Float4OrFloat) && flags == PinFlags.Slider)
                {
                    if (ImGui.SliderFloat4(name, val, 0, 1))
                    {
                        valueX = val[0];
                        valueY = val[1];
                        valueZ = val[2];
                        valueW = val[3];
                        OnValueChanging();
                        changed = true;
                    }
                    IsActive |= ImGui.IsItemActive();
                }

                ImGui.PopItemWidth();
            }
            else
            {
                base.DrawContent();
            }

            if (changed && !IsActive)
            {
                changed = false;
                OnValueChanged();
            }
        }
    }
}