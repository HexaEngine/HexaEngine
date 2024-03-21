namespace HexaEngine.UI.Markup.Parser
{
    using System;
    using System.Globalization;

    public struct StringParser : IPropertyParser
    {
        public readonly bool TryParse(Type type, string value, CultureInfo cultureInfo, out object? result)
        {
            if (type == typeof(string))
            {
                result = value;
                return true;
            }
            result = null;
            return false;
        }
    }
}