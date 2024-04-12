namespace HexaEngine.Core.Assets
{
    using System;

    public readonly struct GuidProvider(Guid guid, Guid parentGuid) : IGuidProvider, IEquatable<GuidProvider>
    {
        public static readonly DefaultGuidProvider Instance = new();
        private readonly Guid guid = guid;

        public Guid ParentGuid { get; } = parentGuid;

        public override bool Equals(object? obj)
        {
            return obj is GuidProvider provider && Equals(provider);
        }

        public bool Equals(GuidProvider other)
        {
            return guid.Equals(other.guid) &&
                   ParentGuid.Equals(other.ParentGuid);
        }

        public readonly Guid GetGuid(string name) => guid;

        public override int GetHashCode()
        {
            return HashCode.Combine(guid, ParentGuid);
        }

        public static bool operator ==(GuidProvider left, GuidProvider right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GuidProvider left, GuidProvider right)
        {
            return !(left == right);
        }
    }
}