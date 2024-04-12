namespace HexaEngine.Editor.NodeEditor.Pins
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImNodes;
    using HexaEngine.Editor.NodeEditor;
    using Newtonsoft.Json;
    using System.Diagnostics;
    using System.Globalization;

    public class HalfPin : Pin, IDefaultValuePin
    {
        private readonly string name;
        private readonly string nameY;
        private readonly string nameZ;
        private readonly string nameW;
        private readonly PinFlags flags;
        private Half valueX;
        private Half valueY;
        private Half valueZ;
        private Half valueW;
        private bool changing;
        private string? defaultExpression;

        [JsonConstructor]
        public HalfPin(int id, string name, ImNodesPinShape shape, PinKind kind, PinType type, uint maxLinks, PinFlags flags, Half valueX, Half valueY, Half valueZ, Half valueW, string defaultExpression) : base(id, name, shape, kind, type, maxLinks)
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

        public HalfPin(int id, string name, ImNodesPinShape shape, PinKind kind, PinType type, uint maxLinks = uint.MaxValue, PinFlags flags = PinFlags.None, string? defaultExpression = null) : base(id, name, shape, kind, type, maxLinks)
        {
            this.name = $"{name}##Value{Id.ToString(CultureInfo.InvariantCulture)}";
            nameY = $"##Value1{Id.ToString(CultureInfo.InvariantCulture)}";
            nameZ = $"##Value2{Id.ToString(CultureInfo.InvariantCulture)}";
            nameW = $"##Value3{Id.ToString(CultureInfo.InvariantCulture)}";
            this.defaultExpression = defaultExpression;
            this.flags = flags;
            SanityChecks();
        }

        public HalfPin(int id, string name, ImNodesPinShape shape, PinKind kind, PinType type, Half value, uint maxLinks = uint.MaxValue, PinFlags flags = PinFlags.None, string? defaultExpression = null) : base(id, name, shape, kind, type, maxLinks)
        {
            this.name = $"{name}##Value{Id.ToString(CultureInfo.InvariantCulture)}";
            nameY = $"##Value1{Id.ToString(CultureInfo.InvariantCulture)}";
            nameZ = $"##Value2{Id.ToString(CultureInfo.InvariantCulture)}";
            nameW = $"##Value3{Id.ToString(CultureInfo.InvariantCulture)}";
            this.defaultExpression = defaultExpression;
            this.flags = flags;
            valueX = value;
            valueY = value;
            valueZ = value;
            valueW = value;
            SanityChecks();
        }

        public Half ValueX { get => valueX; set => valueX = value; }

        public Half ValueY { get => valueY; set => valueY = value; }

        public Half ValueZ { get => valueZ; set => valueZ = value; }

        public Half ValueW { get => valueW; set => valueW = value; }

        public PinFlags Flags => flags;

        public string? DefaultExpression { get => defaultExpression; set => defaultExpression = value; }

        private void SanityChecks()
        {
            Trace.Assert(Type == PinType.Half || Type == PinType.Half2 || Type == PinType.Half3 || Type == PinType.Half4 || Type == PinType.Half2OrHalf || Type == PinType.Half3OrHalf || Type == PinType.Half4OrHalf, $"PinType {Type} is not a float!");
            Trace.Assert(Flags == PinFlags.None || (Flags & PinFlags.ColorPicker) != 0 || (Flags & PinFlags.ColorEdit) != 0 || (Flags & PinFlags.Slider) != 0 || (Flags & PinFlags.AllowOutput) != 0, $"PinFlags {Flags} is not supported!");
        }

        public string GetDefaultValue()
        {
            if (defaultExpression != null)
            {
                return defaultExpression;
            }

            return Type switch
            {
                PinType.Half => valueX.ToString(CultureInfo.InvariantCulture),
                PinType.Half2 => $"half2({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)})",
                PinType.Half3 => $"half3({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)},{valueZ.ToString(CultureInfo.InvariantCulture)})",
                PinType.Half4 => $"half4({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)},{valueZ.ToString(CultureInfo.InvariantCulture)},{valueW.ToString(CultureInfo.InvariantCulture)})",
                PinType.Half2OrHalf => $"half2({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)})",
                PinType.Half3OrHalf => $"half3({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)},{valueZ.ToString(CultureInfo.InvariantCulture)})",
                PinType.Half4OrHalf => $"half4({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)},{valueZ.ToString(CultureInfo.InvariantCulture)},{valueW.ToString(CultureInfo.InvariantCulture)})",

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

            float* val = stackalloc float[4] { (float)valueX, (float)valueY, (float)valueZ, (float)valueW };

            IsActive = false;
            if (Kind == PinKind.Input && Links.Count == 0 || (Flags & PinFlags.AllowOutput) != 0)
            {
                ImGui.PushItemWidth(100);

                if (Type == PinType.Float && flags != PinFlags.Slider && flags != PinFlags.ColorEdit && flags != PinFlags.ColorPicker)
                {
                    if (ImGui.InputFloat(name, val))
                    {
                        valueX = (Half)val[0];
                        OnValueChanging();
                        changing = true;
                    }
                    IsActive |= ImGui.IsItemActive();
                }
                if (Type == PinType.Float && flags == PinFlags.Slider)
                {
                    if (ImGui.SliderFloat(name, val, 0, 1))
                    {
                        valueX = (Half)val[0];
                        OnValueChanging();
                        changing = true;
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
                        valueX = (Half)val[0];
                        valueY = (Half)val[1];
                        OnValueChanging();
                        changing = true;
                    }
                }
                if ((Type == PinType.Float2 || Type == PinType.Float2OrFloat) && flags == PinFlags.Slider)
                {
                    if (ImGui.SliderFloat2(name, val, 0, 1))
                    {
                        valueX = (Half)val[0];
                        valueY = (Half)val[1];
                        OnValueChanging();
                        changing = true;
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
                        valueX = (Half)val[0];
                        valueY = (Half)val[1];
                        valueZ = (Half)val[2];
                        OnValueChanging();
                        changing = true;
                    }
                }
                if ((Type == PinType.Float3 || Type == PinType.Float3OrFloat) && flags == PinFlags.ColorEdit)
                {
                    if (ImGui.ColorEdit3(name, val, ImGuiColorEditFlags.DisplayHex | ImGuiColorEditFlags.Hdr))
                    {
                        valueX = (Half)val[0];
                        valueY = (Half)val[1];
                        valueZ = (Half)val[2];
                        OnValueChanging();
                        changing = true;
                    }
                    IsActive |= ImGui.IsItemActive();
                }
                if ((Type == PinType.Float3 || Type == PinType.Float3OrFloat) && flags == PinFlags.ColorPicker)
                {
                    if (ImGui.ColorPicker3(name, val, ImGuiColorEditFlags.DisplayHex | ImGuiColorEditFlags.Hdr))
                    {
                        valueX = (Half)val[0];
                        valueY = (Half)val[1];
                        valueZ = (Half)val[2];
                        OnValueChanging();
                        changing = true;
                    }
                    IsActive |= ImGui.IsItemActive();
                }
                if ((Type == PinType.Float3 || Type == PinType.Float3OrFloat) && flags == PinFlags.Slider)
                {
                    if (ImGui.SliderFloat3(name, val, 0, 1))
                    {
                        valueX = (Half)val[0];
                        valueY = (Half)val[1];
                        valueZ = (Half)val[2];
                        OnValueChanging();
                        changing = true;
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
                        valueX = (Half)val[0];
                        valueY = (Half)val[1];
                        valueZ = (Half)val[2];
                        valueW = (Half)val[3];
                        OnValueChanging();
                        changing = true;
                    }
                }
                if ((Type == PinType.Float4 || Type == PinType.Float4OrFloat) && flags == PinFlags.ColorEdit)
                {
                    if (ImGui.ColorEdit4(name, val, ImGuiColorEditFlags.DisplayHex | ImGuiColorEditFlags.Hdr))
                    {
                        valueX = (Half)val[0];
                        valueY = (Half)val[1];
                        valueZ = (Half)val[2];
                        valueW = (Half)val[3];
                        OnValueChanging();
                        changing = true;
                    }
                    IsActive |= ImGui.IsItemActive();
                }
                if ((Type == PinType.Float4 || Type == PinType.Float4OrFloat) && flags == PinFlags.ColorPicker)
                {
                    if (ImGui.ColorPicker4(name, val, ImGuiColorEditFlags.DisplayHex | ImGuiColorEditFlags.Hdr))
                    {
                        valueX = (Half)val[0];
                        valueY = (Half)val[1];
                        valueZ = (Half)val[2];
                        valueW = (Half)val[3];
                        OnValueChanging();
                        changing = true;
                    }
                    IsActive |= ImGui.IsItemActive();
                }
                if ((Type == PinType.Float4 || Type == PinType.Float4OrFloat) && flags == PinFlags.Slider)
                {
                    if (ImGui.SliderFloat4(name, val, 0, 1))
                    {
                        valueX = (Half)val[0];
                        valueY = (Half)val[1];
                        valueZ = (Half)val[2];
                        valueW = (Half)val[3];
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