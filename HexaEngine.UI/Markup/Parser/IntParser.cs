namespace HexaEngine.UI.Markup.Parser
{
    using System;
    using System.Globalization;

    public struct IntParser : IPropertyParser
    {
        public readonly bool TryParse(Type type, string value, CultureInfo cultureInfo, out object? result)
        {
            if (type == typeof(int))
            {
                result = int.Parse(value, NumberStyles.Any, cultureInfo);
                return true;
            }
            result = null;
            return false;
        }
    }
}