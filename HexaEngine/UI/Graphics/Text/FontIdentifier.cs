namespace HexaEngine.UI.Graphics.Text
{
    using System;

    public struct FontIdentifier(string family, FontStyle style, FontWeight weight) : IEquatable<FontIdentifier>
    {
        public string Family = family;
        public FontStyle Style = style;
        public FontWeight Weight = weight;

        public override readonly bool Equals(object? obj)
        {
            return obj is FontIdentifier identifier && Equals(identifier);
        }

        public readonly bool Equals(FontIdentifier other)
        {
            return Family == other.Family &&
                   Style == other.Style &&
                   Weight == other.Weight;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Family, Style, Weight);
        }

        public static bool operator ==(FontIdentifier left, FontIdentifier right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FontIdentifier left, FontIdentifier right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"{Family}, {Style}, {Weight}";
        }
    }
}