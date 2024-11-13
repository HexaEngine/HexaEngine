#define MoveAndReplace

namespace HexaEngine.PerformanceTests
{
    using Hexa.NET.Mathematics;

    public struct OctreeObject<T> : IEquatable<OctreeObject<T>>
    {
        public T Value;
        public BoundingSphere Sphere;

        public OctreeObject(T value, BoundingSphere sphere)
        {
            Value = value;
            Sphere = sphere;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is OctreeObject<T> @object && Equals(@object);
        }

        public readonly bool Equals(OctreeObject<T> other)
        {
            return EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Value);
        }

        public static bool operator ==(OctreeObject<T> left, OctreeObject<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(OctreeObject<T> left, OctreeObject<T> right)
        {
            return !(left == right);
        }
    }
}