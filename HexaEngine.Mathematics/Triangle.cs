namespace HexaEngine.Mathematics
{
    using System;
    using System.Numerics;

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

        public static Vector3 MiddlePoint(Vector3 point1, Vector3 point2)
        {
            return new Vector3((point1.X + point2.X) * 0.5f, (point1.Y + point2.Y) * 0.5f, (point1.Z + point2.Z) * 0.5f);
        }

        public static Vector3 Barycentric(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 v0 = b - a, v1 = c - a, v2 = p - a;
            float d00 = Vector3.Dot(v0, v0);
            float d01 = Vector3.Dot(v0, v1);
            float d11 = Vector3.Dot(v1, v1);
            float d20 = Vector3.Dot(v2, v0);
            float d21 = Vector3.Dot(v2, v1);
            float denom = d00 * d11 - d01 * d01;
            float v = (d11 * d20 - d01 * d21) / denom;
            float w = (d00 * d21 - d01 * d20) / denom;
            float u = 1.0f - v - w;
            return new Vector3(u, v, w);
        }

        public readonly Vector3 Barycentric(Vector3 p)
        {
            return Barycentric(p, Point1, Point2, Point3);
        }

        public readonly void Subdivide(out Triangle tri1, out Triangle tri2, out Triangle tri3, out Triangle tri4)
        {
            Vector3 a = MiddlePoint(Point1, Point2);
            Vector3 b = MiddlePoint(Point2, Point3);
            Vector3 c = MiddlePoint(Point3, Point1);
            tri1 = new Triangle(Point1, a, c);
            tri2 = new Triangle(Point2, b, a);
            tri3 = new Triangle(Point3, c, b);
            tri4 = new Triangle(a, b, c);
        }

        public static void GetMiddlePoints(Vector3 point1, Vector3 point2, Vector3 point3, out Vector3 a, out Vector3 b, out Vector3 c)
        {
            a = MiddlePoint(point1, point2);
            b = MiddlePoint(point2, point3);
            c = MiddlePoint(point3, point1);
        }

        public readonly void GetMiddlePoints(out Vector3 a, out Vector3 b, out Vector3 c)
        {
            a = MiddlePoint(Point1, Point2);
            b = MiddlePoint(Point2, Point3);
            c = MiddlePoint(Point3, Point1);
        }

        public override readonly string ToString()
        {
            return $"<{Point1}, {Point2}, {Point3}>";
        }
    }
}