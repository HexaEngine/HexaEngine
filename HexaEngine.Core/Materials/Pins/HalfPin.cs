namespace HexaEngine.Materials.Pins
{
    using HexaEngine.Materials;
    using Newtonsoft.Json;
    using System.Diagnostics;
    using System.Globalization;

    public class HalfPin : Pin, IDefaultValuePin
    {
        [JsonIgnore] public readonly string name;
        [JsonIgnore] public readonly string nameY;
        [JsonIgnore] public readonly string nameZ;
        [JsonIgnore] public readonly string nameW;
        [JsonIgnore] public readonly PinFlags flags;
        [JsonIgnore] public Half valueX;
        [JsonIgnore] public Half valueY;
        [JsonIgnore] public Half valueZ;
        [JsonIgnore] public Half valueW;
        [JsonIgnore] public bool changing;
        [JsonIgnore] public string? defaultExpression;

        [JsonConstructor]
        public HalfPin(int id, string name, PinShape shape, PinKind kind, PinType type, uint maxLinks, PinFlags flags, Half valueX, Half valueY, Half valueZ, Half valueW, string defaultExpression) : base(id, name, shape, kind, type, maxLinks)
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

        public HalfPin(int id, string name, PinShape shape, PinKind kind, PinType type, uint maxLinks = uint.MaxValue, PinFlags flags = PinFlags.None, string? defaultExpression = null) : base(id, name, shape, kind, type, maxLinks)
        {
            this.name = $"{name}##Value{Id.ToString(CultureInfo.InvariantCulture)}";
            nameY = $"##Value1{Id.ToString(CultureInfo.InvariantCulture)}";
            nameZ = $"##Value2{Id.ToString(CultureInfo.InvariantCulture)}";
            nameW = $"##Value3{Id.ToString(CultureInfo.InvariantCulture)}";
            this.defaultExpression = defaultExpression;
            this.flags = flags;
            SanityChecks();
        }

        public HalfPin(int id, string name, PinShape shape, PinKind kind, PinType type, Half value, uint maxLinks = uint.MaxValue, PinFlags flags = PinFlags.None, string? defaultExpression = null) : base(id, name, shape, kind, type, maxLinks)
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
    }
}