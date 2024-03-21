namespace HexaEngine.UI.Markup
{
    using System;
    using System.Globalization;

    public interface IPropertyParser
    {
        bool TryParse(Type type, string value, CultureInfo cultureInfo, out object? result);
    }
}