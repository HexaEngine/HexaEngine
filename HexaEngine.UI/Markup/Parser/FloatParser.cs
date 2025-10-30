namespace HexaEngine.UI.Markup.Parser
{
    using System;
    using System.Globalization;

    public struct FloatParser : IPropertyParser
    {
        public readonly bool TryParse(Type type, string value, CultureInfo cultureInfo, out object? result)
        {
            if (type == typeof(float))
            {
                result = float.Parse(value, NumberStyles.Any, cultureInfo);
                return true;
            }
            result = null;
            return false;
        }
    }
}