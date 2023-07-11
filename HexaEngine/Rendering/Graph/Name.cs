namespace HexaEngine.Rendering.Graph
{
    using System;

    public readonly struct Name : IEquatable<Name>
    {
        private readonly uint id;

        public Name()
        {
            id = NameRegistry.InvalidID;
        }

        public Name(uint id)
        {
            this.id = id;
        }

        public Name(string name)
        {
            id = NameRegistry.Shared.ToId(name);
        }

        public uint ToId()
        {
            return id;
        }

        public bool IsValid()
        {
            return id != NameRegistry.InvalidID;
        }

        public override string ToString()
        {
            return NameRegistry.Shared.GetName(id);
        }

        public override bool Equals(object? obj)
        {
            return obj is Name name && Equals(name);
        }

        public bool Equals(Name other)
        {
            return id == other.id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(id);
        }

        public static bool operator ==(Name left, Name right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Name left, Name right)
        {
            return !(left == right);
        }
    }
}