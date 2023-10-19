namespace HexaEngine.UI.Text
{
    using System.Numerics;

    public struct TextVertex : IEquatable<TextVertex>
    {
        public Vector3 Position;
        public Vector2 Texture;

        public TextVertex(Vector3 position, Vector2 texture)
        {
            Position = position;
            Texture = texture;
        }

        public override bool Equals(object? obj)
        {
            return obj is TextVertex vertex && Equals(vertex);
        }

        public bool Equals(TextVertex other)
        {
            return Position.Equals(other.Position) &&
                   Texture.Equals(other.Texture);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Position, Texture);
        }

        public static bool operator ==(TextVertex left, TextVertex right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TextVertex left, TextVertex right)
        {
            return !(left == right);
        }
    }
}