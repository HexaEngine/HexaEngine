namespace HexaEngine.Resources
{
    public struct ResourceGuid : IEquatable<ResourceGuid>
    {
        public Guid AssetGuid;
        public int UsageType;

        public ResourceGuid(Guid assetGuid, int usageType)
        {
            AssetGuid = assetGuid;
            UsageType = usageType;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is ResourceGuid guid && Equals(guid);
        }

        public readonly bool Equals(ResourceGuid other)
        {
            return AssetGuid.Equals(other.AssetGuid) &&
                   UsageType == other.UsageType;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(AssetGuid, UsageType);
        }

        public static bool operator ==(ResourceGuid left, ResourceGuid right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ResourceGuid left, ResourceGuid right)
        {
            return !(left == right);
        }

        public override readonly string ToString()
        {
            return $"{ResourceTypeRegistry.GetName(UsageType)}-{AssetGuid}";
        }
    }
}