namespace HexaEngine.Materials.Generator.Structs
{
    public struct ConstantBufferDef : IEquatable<ConstantBufferDef>
    {
        public string Name;
        public SType Type;

        public ConstantBufferDef(string name)
        {
            Name = name;
        }

        public ConstantBufferDef(string name, SType type)
        {
            Name = name;
            Type = type;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is ConstantBufferDef def && Equals(def);
        }

        public readonly bool Equals(ConstantBufferDef other)
        {
            return Name == other.Name;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Name);
        }

        public static bool operator ==(ConstantBufferDef left, ConstantBufferDef right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ConstantBufferDef left, ConstantBufferDef right)
        {
            return !(left == right);
        }
    }
}