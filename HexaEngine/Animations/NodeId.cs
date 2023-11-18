namespace HexaEngine.Animations
{
    using System;

    public struct NodeId : IEquatable<NodeId>
    {
        public uint Id;
        public string Name;
        public bool IsBone;

        public NodeId(uint id, string name, bool isBone)
        {
            Id = id;
            Name = name;
            IsBone = isBone;
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj)
        {
            return obj is NodeId id && Equals(id);
        }

        /// <inheritdoc/>
        public readonly bool Equals(NodeId other)
        {
            return Id == other.Id &&
                   Name == other.Name &&
                   IsBone == other.IsBone;
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Id, Name, IsBone);
        }

        public static bool operator ==(NodeId left, NodeId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NodeId left, NodeId right)
        {
            return !(left == right);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return Name;
        }
    }
}