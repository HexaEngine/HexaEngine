namespace FontEditor.IO
{
    using System.IO;

    public class LegacyConverter : IConverter<LegacyFontFile, FontFile>
    {
        public FontFile Convert(LegacyFontFile t)
        {
            int i = 0;
            var chars = t.FontCharacters.ConvertAll(x =>
            {
                var output = new FontCharacter() { Char = (char)(i + 32), Top = 0, Left = x.Left, Height = 16, Width = x.Right - x.Left };
                i++;
                return output;
            });
            return new FontFile() { Characters = chars, Texture = File.ReadAllBytes(t.Texture) };
        }
    }
}