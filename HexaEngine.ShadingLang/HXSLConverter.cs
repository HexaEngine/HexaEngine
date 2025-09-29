namespace HexaEngine.ShadingLang
{
    public abstract class HXSLConverter
    {
        public abstract bool CanConvert(Type objectType);

        public abstract object? ReadHSL(ref ObjectReader reader, Type type, object? existingValue);

        public abstract void WriteHSL(ref ObjectWriter writer, object? value);
    }

    public abstract class HXSLConverter<T> : HXSLConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(T);
        }

        public override object? ReadHSL(ref ObjectReader reader, Type type, object? existingValue)
        {
            return ReadHSL(ref reader, type, (T?)existingValue);
        }

        public abstract T? ReadHSL(ref ObjectReader reader, Type type, T? existingValue);

        public override void WriteHSL(ref ObjectWriter writer, object? value)
        {
            WriteHSL(ref writer, (T?)value);
        }

        public abstract void WriteHSL(ref ObjectWriter writer, T? value);
    }
}