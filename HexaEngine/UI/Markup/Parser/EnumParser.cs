namespace HexaEngine.UI.Markup.Parser
{
    using System;
    using System.Globalization;

    public struct EnumParser : IPropertyParser
    {
        public readonly bool TryParse(Type type, string value, CultureInfo cultureInfo, out object? result)
        {
            if (type.IsEnum)
            {
                result = Enum.Parse(type, value);
                return true;
            }
            result = null;
            return false;
        }
    }
}