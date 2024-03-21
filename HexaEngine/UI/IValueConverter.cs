namespace HexaEngine.UI
{
    using System;
    using System.Globalization;

    public interface IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture);

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture);
    }
}