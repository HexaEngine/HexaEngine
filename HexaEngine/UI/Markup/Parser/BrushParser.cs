namespace HexaEngine.UI.Markup.Parser
{
    using Hexa.NET.Mathematics;
    using HexaEngine.UI.Graphics;
    using System;
    using System.Globalization;

    public struct BrushParser : IPropertyParser
    {
        public readonly bool TryParse(Type type, string value, CultureInfo cultureInfo, out object? result)
        {
            if (type == typeof(Brush))
            {
                if (value.StartsWith('#'))
                {
                    Color color;
                    value = value[1..];

                    uint argb = uint.Parse(value, NumberStyles.HexNumber, cultureInfo);

                    if (value.Length == 8)
                    {
                        color = new Color(((byte)(argb >> 24) & 0xff) / (float)0xff, ((byte)(argb >> 16) & 0xff) / (float)0xff, ((byte)(argb >> 8) & 0xff) / (float)0xff, ((byte)argb & 0xff) / (float)0xff);
                    }
                    else if (value.Length == 6)
                    {
                        color = new Color(((byte)(argb >> 16) & 0xff) / (float)0xff, ((byte)(argb >> 8) & 0xff) / (float)0xff, ((byte)argb & 0xff) / (float)0xff, 1);
                    }
                    else if (value.Length == 3)
                    {
                        color = new Color(((byte)(argb >> 16) & 0xff) / (float)0xff, ((byte)(argb >> 8) & 0xff) / (float)0xff, ((byte)argb & 0xff) / (float)0xff, 1);
                    }
                    else
                    {
                        throw new FormatException("Ungültige Farbzeichenfolge.");
                    }

                    result = new SolidColorBrush(color);
                    return true;
                }
            }
            result = null;
            return false;
        }
    }
}