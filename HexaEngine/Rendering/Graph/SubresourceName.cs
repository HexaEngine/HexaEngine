namespace HexaEngine.Rendering.Graph
{
    using System;

    public readonly struct SubresourceName : IEquatable<SubresourceName>
    {
        private readonly ulong value;

        public SubresourceName(ulong value)
        {
            this.value = value;
        }

        public static SubresourceName ConstructSubresourceName(Name resourceName, uint subresourceIndex)
        {
            ulong name = resourceName.ToId();
            name <<= 32;
            name |= subresourceIndex;
            return name;
        }

        public static (Name, uint) DecodeSubresourceName(SubresourceName name)
        {
            return (new Name((uint)(name >> 32)), (uint)(name & 0x0000FFFF));
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is SubresourceName name && Equals(name);
        }

        public readonly bool Equals(SubresourceName other)
        {
            return value == other.value;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(value);
        }

        public static bool operator ==(SubresourceName left, SubresourceName right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SubresourceName left, SubresourceName right)
        {
            return !(left == right);
        }

        public static implicit operator ulong(SubresourceName subresourceName)
        {
            return subresourceName.value;
        }

        public static implicit operator SubresourceName(ulong subresourceName)
        {
            return new(subresourceName);
        }
    }
}