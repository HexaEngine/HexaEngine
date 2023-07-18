namespace HexaEngine.Rendering.Graph
{
    public class ResourceBinding
    {
        public ResourceBinding(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public static bool operator ==(ResourceBinding left, ResourceBinding right)
        {
            return left.Name == right.Name;
        }

        public static bool operator !=(ResourceBinding left, ResourceBinding right)
        {
            return !(left == right);
        }

        public override bool Equals(object? obj)
        {
            if (obj is ResourceBinding other)
            {
                return this == other;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}