namespace HexaEngine.Materials.Pins
{
    using HexaEngine.Materials;
    using Newtonsoft.Json;
    using System.Diagnostics;
    using System.Globalization;

    public class IntPin : Pin, IDefaultValuePin
    {
        [JsonIgnore] public readonly string name;
        [JsonIgnore] public readonly string nameY;
        [JsonIgnore] public readonly string nameZ;
        [JsonIgnore] public readonly string nameW;
        [JsonIgnore] public readonly PinFlags flags;
        [JsonIgnore] public int valueX;
        [JsonIgnore] public int valueY;
        [JsonIgnore] public int valueZ;
        [JsonIgnore] public int valueW;
        [JsonIgnore] public bool changing;

        [JsonConstructor]
        public IntPin(int id, string name, PinShape shape, PinKind kind, PinType type, uint maxLinks, PinFlags flags, int valueX, int valueY, int valueZ, int valueW) : base(id, name, shape, kind, type, maxLinks)
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

        public IntPin(int id, string name, PinShape shape, PinKind kind, PinType type, uint maxLinks = uint.MaxValue, PinFlags flags = PinFlags.None) : base(id, name, shape, kind, type, maxLinks)
        {
            this.name = $"{name}##Value{Id.ToString(CultureInfo.InvariantCulture)}";
            nameY = $"##Value1{Id.ToString(CultureInfo.InvariantCulture)}";
            nameZ = $"##Value2{Id.ToString(CultureInfo.InvariantCulture)}";
            nameW = $"##Value3{Id.ToString(CultureInfo.InvariantCulture)}";
            this.flags = flags;
            SanityChecks();
        }

        public IntPin(int id, string name, PinShape shape, PinKind kind, PinType type, int value, uint maxLinks = uint.MaxValue, PinFlags flags = PinFlags.None) : base(id, name, shape, kind, type, maxLinks)
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
    }
}