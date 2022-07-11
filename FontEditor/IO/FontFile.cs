namespace FontEditor.IO
{
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;

    public class FontFile
    {
        public List<FontCharacter> Characters { get; init; } = new();

        public byte[] Texture { get; init; }

        public static FontFile Create(IEnumerable<FontCharacter> characters, string texture)
        {
            return new FontFile() { Characters = new(characters), Texture = File.ReadAllBytes(texture) };
        }
    }

    [StructLayout(LayoutKind.Auto)]
    public struct FontCharacter
    {
        public char Char;
        public float Top, Left;
        public float Width, Height;
    }
}