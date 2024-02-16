namespace HexaEngine.Mathematics
{
    using System;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a geometric <see cref="Triangle"/> defined by three points in 3D space.
    /// </summary>
    public struct Triangle : IEquatable<Triangle>
    {
        /// <summary>
        /// The first vertex of the <see cref="Triangle"/>.
        /// </summary>
        public Vector3 Point1;

        /// <summary>
        /// The second vertex of the <see cref="Triangle"/>.
        /// </summary>
        public Vector3 Point2;

        /// <summary>
        /// The third vertex of the <see cref="Triangle"/>.
        /// </summary>
        public Vector3 Point3;

        /// <summary>
        /// Initializes a new instance of the <see cref="Triangle"/> struct with three vertices.
        /// </summary>
        /// <param name="vertex1">The first vertex of the <see cref="Triangle"/>.</param>
        /// <param name="vertex2">The second vertex of the <see cref="Triangle"/>.</param>
        /// <param name="vertex3">The third vertex of the <see cref="Triangle"/>.</param>
        public Triangle(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
        {
            Point1 = vertex1;
            Point2 = vertex2;
            Point3 = vertex3;
        }

        /// <summary>
        /// Gets or sets the vertices of the Triangle
        /// </summary>
        /// <param name="index">0 to 2 is valid.</param>
        /// <returns>The vertex of the triangle</returns>
        public unsafe Vector3 this[int index]
        {
            get => ((Vector3*)Unsafe.AsPointer(ref this))[index];
            set => ((Vector3*)Unsafe.AsPointer(ref this))[index] = value;
        }

        /// <summary>
        /// Calculates the area of the <see cref="Triangle"/>.
        /// </summary>
        /// <returns>The area of the <see cref="Triangle"/>.</returns>
        public readonly float Area()
        {
            var ab = Point2 - Point1;
            var ac = Point3 - Point1;
            var cross = Vector3.Cross(ab, ac);
            return 0.5f * cross.Length();
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj)
        {
            return obj is Triangle triangle && Equals(triangle);
        }

        /// <summary>
        /// Determines whether the current <see cref="Triangle"/> is equal to another <see cref="Triangle"/>.
        /// </summary>
        /// <param name="other">The <see cref="Triangle"/> to compare with the current <see cref="Triangle"/>.</param>
        /// <returns><c>true</c> if the Triangles are equal; otherwise, <c>false</c>.</returns>
        public readonly bool Equals(Triangle other)
        {
            return Point1.Equals(other.Point1) &&
                   Point2.Equals(other.Point2) &&
                   Point3.Equals(other.Point3);
        }

        /// <summary>
        /// Returns the hash code for this <see cref="Triangle"/>.
        /// </summary>
        /// <returns>The hash code of the <see cref="Triangle"/>.</returns>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Point1, Point2, Point3);
        }

        /// <summary>
        /// Determines if two Triangles are equal.
        /// </summary>
        /// <param name="left">The first Triangle to compare.</param>
        /// <param name="right">The second Triangle to compare.</param>
        /// <returns><c>true</c> if the Triangles are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Triangle left, Triangle right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines if two Triangles are not equal.
        /// </summary>
        /// <param name="left">The first Triangle to compare.</param>
        /// <param name="right">The second Triangle to compare.</param>
        /// <returns><c>true</c> if the Triangles are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Triangle left, Triangle right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Computes the middle point between two given points.
        /// </summary>
        /// <param name="point1">The first point.</param>
        /// <param name="point2">The second point.</param>
        /// <returns>The middle point between the two given points.</returns>
        public static Vector3 MiddlePoint(Vector3 point1, Vector3 point2)
        {
            return new Vector3((point1.X + point2.X) * 0.5f, (point1.Y + point2.Y) * 0.5f, (point1.Z + point2.Z) * 0.5f);
        }

        /// <summary>
        /// Computes the barycentric coordinates of a point within the <see cref="Triangle"/>.
        /// </summary>
        /// <param name="p">The point for which to compute barycentric coordinates.</param>
        /// <param name="a">The first vertex of the <see cref="Triangle"/>.</param>
        /// <param name="b">The second vertex of the <see cref="Triangle"/>.</param>
        /// <param name="c">The third vertex of the <see cref="Triangle"/>.</param>
        /// <returns>The barycentric coordinates of the point within the <see cref="Triangle"/>.</returns>
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

        /// <summary>
        /// Computes the barycentric coordinates of a point within the <see cref="Triangle"/>.
        /// </summary>
        /// <param name="p">The point for which to compute barycentric coordinates.</param>
        /// <returns>The barycentric coordinates of the point within the <see cref="Triangle"/>.</returns>
        public readonly Vector3 Barycentric(Vector3 p)
        {
            return Barycentric(p, Point1, Point2, Point3);
        }

        /// <summary>
        /// Subdivides the <see cref="Triangle"/> into four smaller triangles.
        /// </summary>
        /// <param name="tri1">The first of the four resulting triangles.</param>
        /// <param name="tri2">The second of the four resulting triangles.</param>
        /// <param name="tri3">The third of the four resulting triangles.</param>
        /// <param name="tri4">The fourth of the four resulting triangles.</param>
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

        /// <summary>
        /// Computes the middle points of the edges of a <see cref="Triangle"/>.
        /// </summary>
        /// <param name="point1">The first vertex of the <see cref="Triangle"/>.</param>
        /// <param name="point2">The second vertex of the <see cref="Triangle"/>.</param>
        /// <param name="point3">The third vertex of the <see cref="Triangle"/>.</param>
        /// <param name="a">The middle point between the first and second vertices.</param>
        /// <param name="b">The middle point between the second and third vertices.</param>
        /// <param name="c">The middle point between the third and first vertices.</param>
        public static void GetMiddlePoints(Vector3 point1, Vector3 point2, Vector3 point3, out Vector3 a, out Vector3 b, out Vector3 c)
        {
            a = MiddlePoint(point1, point2);
            b = MiddlePoint(point2, point3);
            c = MiddlePoint(point3, point1);
        }

        /// <summary>
        /// Computes the middle points of the edges of the <see cref="Triangle"/>.
        /// </summary>
        /// <param name="a">The middle point between the first and second vertices.</param>
        /// <param name="b">The middle point between the second and third vertices.</param>
        /// <param name="c">The middle point between the third and first vertices.</param>
        public readonly void GetMiddlePoints(out Vector3 a, out Vector3 b, out Vector3 c)
        {
            a = MiddlePoint(Point1, Point2);
            b = MiddlePoint(Point2, Point3);
            c = MiddlePoint(Point3, Point1);
        }

        /// <summary>
        /// Returns a string representation of the <see cref="Triangle"/>.
        /// </summary>
        /// <returns>A string representation of the <see cref="Triangle"/>.</returns>
        public override readonly string ToString()
        {
            return $"<{Point1}, {Point2}, {Point3}>";
        }
    }
}