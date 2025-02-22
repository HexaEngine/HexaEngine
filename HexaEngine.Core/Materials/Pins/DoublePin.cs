namespace HexaEngine.Materials.Pins
{
    using HexaEngine.Materials;
    using Newtonsoft.Json;
    using System.Diagnostics;
    using System.Globalization;

    public class DoublePin : NumericPin
    {
        [JsonIgnore] public readonly string name;
        [JsonIgnore] public readonly string nameY;
        [JsonIgnore] public readonly string nameZ;
        [JsonIgnore] public readonly string nameW;
        [JsonIgnore] public double valueX;
        [JsonIgnore] public double valueY;
        [JsonIgnore] public double valueZ;
        [JsonIgnore] public double valueW;
        [JsonIgnore] public bool changing;

        [JsonConstructor]
        public DoublePin(int id, string name, PinShape shape, PinKind kind, PinType type, uint maxLinks, PinFlags flags, double valueX, double valueY, double valueZ, double valueW) : base(id, name, shape, kind, type, maxLinks)
        {
            this.name = $"{name}##Value{Id.ToString(CultureInfo.InvariantCulture)}";
            nameY = $"##Value1{Id.ToString(CultureInfo.InvariantCulture)}";
            nameZ = $"##Value2{Id.ToString(CultureInfo.InvariantCulture)}";
            nameW = $"##Value3{Id.ToString(CultureInfo.InvariantCulture)}";
            this.Flags = flags;
            this.valueX = valueX;
            this.valueY = valueY;
            this.valueZ = valueZ;
            this.valueW = valueW;
            SanityChecks();
        }

        public DoublePin(int id, string name, PinShape shape, PinKind kind, PinType type, uint maxLinks = uint.MaxValue, PinFlags flags = PinFlags.None) : base(id, name, shape, kind, type, maxLinks)
        {
            this.name = $"{name}##Value{Id.ToString(CultureInfo.InvariantCulture)}";
            nameY = $"##Value1{Id.ToString(CultureInfo.InvariantCulture)}";
            nameZ = $"##Value2{Id.ToString(CultureInfo.InvariantCulture)}";
            nameW = $"##Value3{Id.ToString(CultureInfo.InvariantCulture)}";
            this.Flags = flags;
            SanityChecks();
        }

        public DoublePin(int id, string name, PinShape shape, PinKind kind, PinType type, double value, uint maxLinks = uint.MaxValue, PinFlags flags = PinFlags.None) : base(id, name, shape, kind, type, maxLinks)
        {
            this.name = $"{name}##Value{Id.ToString(CultureInfo.InvariantCulture)}";
            nameY = $"##Value1{Id.ToString(CultureInfo.InvariantCulture)}";
            nameZ = $"##Value2{Id.ToString(CultureInfo.InvariantCulture)}";
            nameW = $"##Value3{Id.ToString(CultureInfo.InvariantCulture)}";
            this.Flags = flags;
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

        private void SanityChecks()
        {
            Trace.Assert(Type == PinType.Double || Type == PinType.Double2 || Type == PinType.Double3 || Type == PinType.Double4 || Type == PinType.Double2OrDouble || Type == PinType.Double3OrDouble || Type == PinType.Double4OrDouble, $"PinType {Type} is not a double!");
            Trace.Assert(Flags == PinFlags.None || (Flags & PinFlags.ColorPicker) != 0 || (Flags & PinFlags.ColorEdit) != 0 || (Flags & PinFlags.Slider) != 0 || (Flags & PinFlags.AllowOutput) != 0, $"PinFlags {Flags} is not supported!");
            Trace.Assert((Flags & PinFlags.ColorPicker) == 0 && (Flags & PinFlags.ColorEdit) == 0, $"ColorPicker and ColorEdit is not supported!");
        }

        public override string GetDefaultValue()
        {
            return Type switch
            {
                PinType.Double => valueX.ToString(CultureInfo.InvariantCulture),
                PinType.Double2 => $"double2({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)})",
                PinType.Double3 => $"double3({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)},{valueZ.ToString(CultureInfo.InvariantCulture)})",
                PinType.Double4 => $"double4({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)},{valueZ.ToString(CultureInfo.InvariantCulture)},{valueW.ToString(CultureInfo.InvariantCulture)})",
                PinType.Double2OrDouble => $"double2({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)})",
                PinType.Double3OrDouble => $"double3({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)},{valueZ.ToString(CultureInfo.InvariantCulture)})",
                PinType.Double4OrDouble => $"double4({valueX.ToString(CultureInfo.InvariantCulture)},{valueY.ToString(CultureInfo.InvariantCulture)},{valueZ.ToString(CultureInfo.InvariantCulture)},{valueW.ToString(CultureInfo.InvariantCulture)})",

                _ => "0",
            };
        }
    }
}