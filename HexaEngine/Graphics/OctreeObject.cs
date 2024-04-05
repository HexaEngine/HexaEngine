namespace HexaEngine.Graphics
{
    using HexaEngine.Mathematics;

    public struct OctreeObject : IEquatable<OctreeObject>
    {
        public int Index;
        public BoundingBox Bounds;

        public OctreeObject(int index, BoundingBox bounds)
        {
            Index = index;
            Bounds = bounds;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is OctreeObject @object && Equals(@object);
        }

        public readonly bool Equals(OctreeObject other)
        {
            return Index == other.Index;
        }

        public override readonly int GetHashCode()
        {
            return Index;
        }

        public static bool operator ==(OctreeObject left, OctreeObject right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(OctreeObject left, OctreeObject right)
        {
            return !(left == right);
        }
    }
}