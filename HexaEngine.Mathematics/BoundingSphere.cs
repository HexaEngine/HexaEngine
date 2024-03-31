namespace HexaEngine.Mathematics
{
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Defines an sphere in three dimensional space.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct BoundingSphere : IEquatable<BoundingSphere>, IFormattable
    {
        /// <summary>
        /// An empty bounding sphere (Center = 0 and Radius = 0).
        /// </summary>
        public static readonly BoundingSphere Empty = new();

        /// <summary>
        /// The center point of the sphere.
        /// </summary>
        public Vector3 Center;

        /// <summary>
        /// The radius of the sphere.
        /// </summary>
        public float Radius;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundingSphere"/> struct.
        /// </summary>
        /// <param name="center">The center of the sphere.</param>
        /// <param name="radius">The radius of the sphere.</param>
        public BoundingSphere(Vector3 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        /// <summary>
        /// Creates a <see cref="BoundingBox"/> from the sum of 2 <see cref="BoundingBox"/>es.
        /// </summary>
        /// <param name="primary"><see cref="BoundingBox"/> is the first subject</param>
        /// <param name="secondary"><see cref="BoundingBox"/> is second subject</param>
        public static BoundingBox MergeBoundingBoxes(in BoundingBox primary, in BoundingBox secondary)
        {
            return new BoundingBox(
                Vector3.Min(primary.Min, secondary.Min),
                Vector3.Max(primary.Max, secondary.Max)
                );
        }

        /// <summary>
        /// Creates a <see cref="BoundingBox"/> from the sum of 2 <see cref="BoundingBox"/>es.
        /// </summary>
        /// <param name="primary"><see cref="BoundingBox"/> is the first subject</param>
        /// <param name="secondary"><see cref="BoundingBox"/> is second subject</param>
        /// <param name="result">The created <see cref="BoundingBox"/> is the result of the sum of the input.</param>
        public static void MergeBoundingBoxes(in BoundingBox primary, in BoundingBox secondary, out BoundingBox result)
        {
            result = new BoundingBox(
                Vector3.Min(primary.Min, secondary.Min),
                Vector3.Max(primary.Max, secondary.Max)
                );
        }

        /// <summary>
        /// Creates the smallest <see cref="BoundingSphere"/> that can contain a specified <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="box">The <see cref="BoundingBox"/> to create the <see cref="BoundingSphere"/> from.</param>
        /// <returns>The created <see cref="BoundingSphere"/>.</returns>
        public static BoundingSphere CreateFromBoundingBox(in BoundingBox box)
        {
            Unsafe.SkipInit(out BoundingSphere result);
            result.Center = box.Center;
            result.Radius = Vector3.Distance(result.Center, box.Max);
            return result;
        }

        /// <summary>
        /// Translates and scales given <see cref="BoundingSphere"/> using a given <see cref="Matrix4x4"/>.
        /// </summary>
        /// <param name="sphere">The source <see cref="BoundingSphere"/>.</param>
        /// <param name="transform">A transformation matrix that might include translation, rotation, or uniform scaling.</param>
        /// <returns>The transformed BoundingSphere.</returns>
        public static BoundingSphere Transform(in BoundingSphere sphere, in Matrix4x4 transform)
        {
            Transform(sphere, transform, out BoundingSphere result);
            return result;
        }

        /// <summary>
        /// Translates and scales given <see cref="BoundingSphere"/> using a given <see cref="Matrix4x4"/>.
        /// </summary>
        /// <param name="sphere">The source <see cref="BoundingSphere"/>.</param>
        /// <param name="transform">A transformation matrix that might include translation, rotation, or uniform scaling.</param>
        /// <param name="result">The transformed BoundingSphere.</param>
        public static void Transform(in BoundingSphere sphere, in Matrix4x4 transform, out BoundingSphere result)
        {
            Vector3 center = Vector3.Transform(sphere.Center, transform);

            float majorAxisLengthSquared = Math.Max(
               (transform.M11 * transform.M11) + (transform.M12 * transform.M12) + (transform.M13 * transform.M13), Math.Max(
               (transform.M21 * transform.M21) + (transform.M22 * transform.M22) + (transform.M23 * transform.M23),
               (transform.M31 * transform.M31) + (transform.M32 * transform.M32) + (transform.M33 * transform.M33)));

            float radius = sphere.Radius * (float)Math.Sqrt(majorAxisLengthSquared);
            result = new BoundingSphere(center, radius);
        }

        /// <summary>
        /// Determines whether the bounding sphere contains a specified point.
        /// </summary>
        /// <param name="point">The point to check for containment.</param>
        /// <returns>A value indicating whether the point is contained within, disjoint from, or intersects the bounding sphere.</returns>
        public ContainmentType Contains(in Vector3 point)
        {
            if (Vector3.DistanceSquared(point, Center) <= Radius * Radius)
            {
                return ContainmentType.Contains;
            }

            return ContainmentType.Disjoint;
        }

        /// <summary>
        /// Determines whether the bounding sphere contains a specified bounding box.
        /// </summary>
        /// <param name="box">The bounding box to check for containment.</param>
        /// <returns>A value indicating whether the bounding box is contained within, disjoint from, or intersects the bounding sphere.</returns>
        public readonly ContainmentType Contains(in BoundingBox box)
        {
            return box.Contains(this);
        }

        /// <summary>
        /// Determines whether the bounding sphere contains a specified bounding sphere.
        /// </summary>
        /// <param name="sphere">The bounding sphere to check for containment.</param>
        /// <returns>A value indicating whether the other bounding sphere is contained within, disjoint from, or intersects the bounding sphere.</returns>
        public ContainmentType Contains(in BoundingSphere sphere)
        {
            float distance = Vector3.Distance(Center, sphere.Center);

            if (Radius + sphere.Radius < distance)
            {
                return ContainmentType.Disjoint;
            }

            if (Radius - sphere.Radius < distance)
            {
                return ContainmentType.Intersects;
            }

            return ContainmentType.Contains;
        }

        /// <summary>
        /// Determines whether the bounding sphere intersects with a specified ray.
        /// </summary>
        /// <param name="ray">The ray to check for intersection.</param>
        /// <returns>
        /// The distance along the ray where the intersection occurs, or null if there's no intersection.
        /// </returns>
        public float? Intersects(in Ray ray)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 177

            Vector3 m = Vector3.Subtract(ray.Position, Center);

            float b = Vector3.Dot(m, ray.Direction);
            float c = Vector3.Dot(m, m) - (Radius * Radius);

            if (c > 0f && b > 0f)
            {
                return null;
            }

            float discriminant = b * b - c;

            if (discriminant < 0f)
            {
                return null;
            }

            float distance = -b - (float)Math.Sqrt(discriminant);

            if (distance < 0f)
            {
                distance = 0f;
            }

            return distance;
        }

        /// <summary>
        /// Checks whether the current <see cref="BoundingSphere"/> intersects with a specified <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="box">The <see cref="BoundingBox"/> to check for intersection with the current <see cref="BoundingSphere"/>.</param>
        /// <returns>True if intersects, false otherwise.</returns>
        public bool Intersects(in BoundingBox box)
        {
            Vector3 clampedVector = Vector3.Clamp(Center, box.Min, box.Max);
            float distance = Vector3.DistanceSquared(Center, clampedVector);
            return distance <= Radius * Radius;
        }

        /// <summary>
        /// Checks whether the current <see cref="BoundingSphere"/> intersects with a specified <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="sphere">The <see cref="BoundingSphere"/> to check for intersection with the current <see cref="BoundingSphere"/>.</param>
        /// <returns>True if intersects, false otherwise.</returns>
        public bool Intersects(in BoundingSphere sphere)
        {
            float radiisum = Radius + sphere.Radius;
            return Vector3.DistanceSquared(Center, sphere.Center) <= radiisum * radiisum;
        }

        /// <summary>
        /// Checks whether the current <see cref="BoundingSphere"/> intersects with a specified <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="plane">The <see cref="Plane"/> to check for intersection with the current <see cref="Plane"/>.</param>
        /// <returns>True if intersects, false otherwise.</returns>
        public PlaneIntersectionType Intersects(in Plane plane)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 160

            float distance = Vector3.Dot(plane.Normal, Center);
            distance += plane.D;

            if (distance > Radius)
            {
                return PlaneIntersectionType.Front;
            }

            if (distance < -Radius)
            {
                return PlaneIntersectionType.Back;
            }

            return PlaneIntersectionType.Intersecting;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is BoundingSphere value && Equals(value);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(BoundingSphere other)
        {
            return Center.Equals(other.Center)
                && Radius == other.Radius;
        }

        /// <summary>
        /// Compares two <see cref="BoundingSphere"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="BoundingSphere"/> on the left hand of the operand.</param>
        /// <param name="right">The <see cref="BoundingSphere"/> on the right hand of the operand.</param>
        /// <returns>
        /// True if the current left is equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(BoundingSphere left, BoundingSphere right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="BoundingSphere"/> objects for inequality.
        /// </summary>
        /// <param name="left">The <see cref="BoundingSphere"/> on the left hand of the operand.</param>
        /// <param name="right">The <see cref="BoundingSphere"/> on the right hand of the operand.</param>
        /// <returns>
        /// True if the current left is unequal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(BoundingSphere left, BoundingSphere right) => !left.Equals(right);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            HashCode hashCode = new();
            {
                hashCode.Add(Center);
                hashCode.Add(Radius);
            }
            return hashCode.ToHashCode();
        }

        /// <inheritdoc />
        public override string ToString() => ToString(format: null, formatProvider: null);

        /// <inheritdoc />
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return $"Center:{Center.ToString(format, formatProvider)} Radius:{Radius.ToString(format, formatProvider)}";
        }

        /// <summary>
        /// Reads a <see cref="BoundingSphere"/> from a binary stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The binary stream to read from.</param>
        /// <param name="endianness">The endianness to use for reading the data.</param>
        /// <returns>The <see cref="BoundingSphere"/> read from the stream.</returns>
        public static BoundingSphere Read(Stream stream, Endianness endianness)
        {
            Vector3 center = stream.ReadVector3(endianness);
            float radius = stream.ReadFloat(endianness);
            return new(center, radius);
        }

        /// <summary>
        /// Writes the <see cref="BoundingSphere"/> to a binary stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The binary stream to write to.</param>
        /// <param name="endianness">The endianness to use for writing the data.</param>
        public void Write(Stream stream, Endianness endianness)
        {
            stream.WriteVector3(Center, endianness);
            stream.WriteFloat(Radius, endianness);
        }
    }
}