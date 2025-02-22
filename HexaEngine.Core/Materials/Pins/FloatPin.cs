namespace HexaEngine.Materials.Pins
{
    using HexaEngine.Materials;
    using Newtonsoft.Json;
    using System.Diagnostics;
    using System.Globalization;
    using System.Numerics;

    public class FloatPin : NumericPin
    {
        [JsonIgnore] public readonly string name;
        [JsonIgnore] public readonly string nameY;
        [JsonIgnore] public readonly string nameZ;
        [JsonIgnore] public readonly string nameW;
        [JsonIgnore] public float valueX;
        [JsonIgnore] public float valueY;
        [JsonIgnore] public float valueZ;
        [JsonIgnore] public float valueW;
        [JsonIgnore] public bool changed;
        [JsonIgnore] public string? defaultExpression;

        [JsonConstructor]
        public FloatPin(int id, string name, PinShape shape, PinKind kind, PinType type, uint maxLinks, PinFlags flags, float valueX, float valueY, float valueZ, float valueW, string defaultExpression) : base(id, name, shape, kind, type, maxLinks)
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
            this.defaultExpression = defaultExpression;
            SanityChecks();
        }

        public FloatPin(int id, string name, PinShape shape, PinKind kind, PinType type, uint maxLinks = uint.MaxValue, PinFlags flags = PinFlags.None, string? defaultExpression = null) : base(id, name, shape, kind, type, maxLinks)
        {
            this.name = $"{name}##Value{Id.ToString(CultureInfo.InvariantCulture)}";
            nameY = $"##Value1{Id.ToString(CultureInfo.InvariantCulture)}";
            nameZ = $"##Value2{Id.ToString(CultureInfo.InvariantCulture)}";
            nameW = $"##Value3{Id.ToString(CultureInfo.InvariantCulture)}";
            this.defaultExpression = defaultExpression;
            this.Flags = flags;
            SanityChecks();
        }

        public FloatPin(int id, string name, PinShape shape, PinKind kind, PinType type, Vector4 value, uint maxLinks = uint.MaxValue, PinFlags flags = PinFlags.None, string? defaultExpression = null) : base(id, name, shape, kind, type, maxLinks)
        {
            this.name = $"{name}##Value{Id.ToString(CultureInfo.InvariantCulture)}";
            nameY = $"##Value1{Id.ToString(CultureInfo.InvariantCulture)}";
            nameZ = $"##Value2{Id.ToString(CultureInfo.InvariantCulture)}";
            nameW = $"##Value3{Id.ToString(CultureInfo.InvariantCulture)}";
            this.defaultExpression = defaultExpression;
            this.Flags = flags;
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

        public string? DefaultExpression { get => defaultExpression; set => defaultExpression = value; }

        private void SanityChecks()
        {
            Trace.Assert(Type == PinType.Float || Type == PinType.Float2 || Type == PinType.Float3 || Type == PinType.Float4 || Type == PinType.Float2OrFloat || Type == PinType.Float3OrFloat || Type == PinType.Float4OrFloat, $"PinType {Type} is not a float!");
            Trace.Assert(Flags == PinFlags.None || (Flags & PinFlags.ColorPicker) != 0 || (Flags & PinFlags.ColorEdit) != 0 || (Flags & PinFlags.Slider) != 0 || (Flags & PinFlags.AllowOutput) != 0, $"PinFlags {Flags} is not supported!");
        }

        public override string GetDefaultValue()
        {
            if (defaultExpression != null)
            {
                return defaultExpression;
            }

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
    }
}