namespace HexaEngine.UI
{
    using System;
    using System.ComponentModel;

    public class DeferrableContentConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            if (sourceType == typeof(Stream))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object value)
        {
            if (value is Stream stream)
            {
                // Pass the stream to the internal constructor of DeferrableContent
                return new DeferrableContent(stream);
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}