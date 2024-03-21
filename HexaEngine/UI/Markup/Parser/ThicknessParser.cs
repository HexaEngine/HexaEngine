namespace HexaEngine.UI.Markup.Parser
{
    using System;
    using System.Globalization;

    public struct ThicknessParser : IPropertyParser
    {
        public readonly bool TryParse(Type type, string value, CultureInfo cultureInfo, out object? result)
        {
            if (type != typeof(Thickness))
            {
                result = null;
                return false;
            }

            Thickness thickness;

            value = value.Replace(" ", "");

            string[] values = value.Split(',');

            if (values.Length == 1)
            {
                float uniformValue = float.Parse(values[0], cultureInfo);
                thickness = new Thickness(uniformValue);
            }
            else if (values.Length == 2)
            {
                float leftRightValue = float.Parse(values[0], cultureInfo);
                float topBottomValue = float.Parse(values[1], cultureInfo);
                thickness = new Thickness(leftRightValue, topBottomValue, leftRightValue, topBottomValue);
            }
            else if (values.Length == 4)
            {
                float left = float.Parse(values[0], cultureInfo);
                float top = float.Parse(values[1], cultureInfo);
                float right = float.Parse(values[2], cultureInfo);
                float bottom = float.Parse(values[3], cultureInfo);
                thickness = new Thickness(left, top, right, bottom);
            }
            else
            {
                throw new FormatException("Ungültige Thickness-Formatzeichenfolge.");
            }

            result = thickness;

            return true;
        }
    }
}