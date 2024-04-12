namespace HexaEngine.Core.Assets
{
    using System;

    public readonly struct DefaultGuidProvider(Guid parentGuid) : IGuidProvider, IEquatable<DefaultGuidProvider>
    {
        public static readonly DefaultGuidProvider Instance = new();

        public Guid ParentGuid { get; } = parentGuid;

        public override readonly bool Equals(object? obj)
        {
            return obj is DefaultGuidProvider provider && Equals(provider);
        }

        public readonly bool Equals(DefaultGuidProvider other)
        {
            return ParentGuid.Equals(other.ParentGuid);
        }

        public readonly Guid GetGuid(string name) => Guid.NewGuid();

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(ParentGuid);
        }

        public static bool operator ==(DefaultGuidProvider left, DefaultGuidProvider right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DefaultGuidProvider left, DefaultGuidProvider right)
        {
            return !(left == right);
        }
    }
}