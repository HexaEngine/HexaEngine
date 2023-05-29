namespace HexaEngine.Mathematics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Text;
    using System.Threading.Tasks;

    public struct Triangle : IEquatable<Triangle>
    {
        public Vector3 Point1;
        public Vector3 Point2;
        public Vector3 Point3;

        public Triangle(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
        {
            Point1 = vertex1;
            Point2 = vertex2;
            Point3 = vertex3;
        }

        public readonly float Area()
        {
            var ab = Point2 - Point1;
            var ac = Point3 - Point1;
            var cross = Vector3.Cross(ab, ac);
            return 0.5f * cross.Length();
        }

        public override bool Equals(object? obj)
        {
            return obj is Triangle triangle && Equals(triangle);
        }

        public bool Equals(Triangle other)
        {
            return Point1.Equals(other.Point1) &&
                   Point2.Equals(other.Point2) &&
                   Point3.Equals(other.Point3);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Point1, Point2, Point3);
        }

        public static bool operator ==(Triangle left, Triangle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Triangle left, Triangle right)
        {
            return !(left == right);
        }
    }
}