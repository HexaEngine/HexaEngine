namespace HexaEngine.UI.Graphics.Text
{
    using System.Collections.Generic;

    public class FontFamily
    {
        public FontFamily(string name, List<FontFileInfo> fonts)
        {
            Name = name;
            Fonts = fonts;
        }

        public string Name { get; }

        public List<FontFileInfo> Fonts { get; }
    }
}