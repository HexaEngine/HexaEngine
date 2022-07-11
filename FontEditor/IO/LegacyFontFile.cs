namespace FontEditor.IO
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;

    public class LegacyFontFile
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Character
        {
            // Variables.
            public float Left, Right;

            public int Size;

            // Constructor
            public Character(string fontData)
            {
                var ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                ci.NumberFormat.CurrencyDecimalSeparator = ".";
                var data = fontData.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                Left = float.Parse(data[^3], NumberStyles.Any, ci);
                Right = float.Parse(data[^2], NumberStyles.Any, ci);
                Size = int.Parse(data[^1], NumberStyles.Any, ci);
            }
        }

        public List<Character> FontCharacters { get; set; }

        public string Texture { get; set; }

        public LegacyFontFile(string fontFileName, string textureFileName)
        {
            // Load in the text file containing the font data.
            LoadFontData(fontFileName);
            Texture = textureFileName;
        }

        private void LoadFontData(string fontFileName)
        {
            // Get all the lines containing the font data.
            var fontDataLines = File.ReadAllLines(fontFileName);

            // Create Font and fill with characters.
            FontCharacters = new List<Character>();
            foreach (var line in fontDataLines)
            {
                FontCharacters.Add(new Character(line));
            }
        }
    }
}