﻿namespace HexaEngine.Mathematics
{
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Defines an axis-aligned box-shaped 3D volume.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct BoundingBox : IEquatable<BoundingBox>, IFormattable
    {
        private static readonly Vector3[] g_BoxOffset =
[
    new Vector3(-1.0f, -1.0f, 1.0f),
    new Vector3(1.0f, -1.0f, 1.0f),
    new Vector3(1.0f, 1.0f, 1.0f),
    new Vector3(-1.0f, 1.0f, 1.0f),
    new Vector3(-1.0f, -1.0f, -1.0f),
    new Vector3(1.0f, -1.0f, -1.0f),
    new Vector3(1.0f, 1.0f, -1.0f),
    new Vector3(-1.0f, 1.0f, -1.0f),
];

        /// <summary>
        /// Specifies the total number of corners (8) in the BoundingBox.
        /// </summary>
        public const int CornerCount = 8;

        /// <summary>
        /// The minimum point of the box.
        /// </summary>
        public Vector3 Min;

        /// <summary>
        /// The maximum point of the box.
        /// </summary>
        public Vector3 Max;

        /// <summary>
        /// A <see cref="BoundingBox"/> which represents an empty space.
        /// </summary>
        public static readonly BoundingBox Empty = new(new Vector3(float.MaxValue), new Vector3(float.MinValue));

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundingBox"/> struct.
        /// </summary>
        /// <param name="min">The minimum vertex of the bounding box.</param>
        /// <param name="max">The maximum vertex of the bounding box.</param>
        public BoundingBox(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundingBox"/> struct.
        /// </summary>
        /// <param name="sphere">The <see cref="BoundingSphere"/> to initialize from.</param>
        public BoundingBox(in BoundingSphere sphere)
        {
            Min = new Vector3(sphere.Center.X - sphere.Radius, sphere.Center.Y - sphere.Radius, sphere.Center.Z - sphere.Radius);
            Max = new Vector3(sphere.Center.X + sphere.Radius, sphere.Center.Y + sphere.Radius, sphere.Center.Z + sphere.Radius);
        }

        /// <summary>
        /// Initializes the bounding box with specified minimum and maximum points.
        /// </summary>
        /// <param name="min">The minimum corner of the bounding box.</param>
        /// <param name="max">The maximum corner of the bounding box.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Initialize(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Initializes the bounding box with the same dimensions as the provided bounding box.
        /// </summary>
        /// <param name="box">The bounding box to copy dimensions from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Initialize(BoundingBox box)
        {
            Min = box.Min;
            Max = box.Max;
        }

        /// <summary>
        /// Gets the center of this bounding box.
        /// </summary>
        public readonly Vector3 Center => (Min + Max) / 2;

        /// <summary>
        /// Gets the extent of this bounding box.
        /// </summary>
        public readonly Vector3 Extent => (Max - Min) / 2;

        /// <summary>
        /// Gets size of this bounding box.
        /// </summary>
        public readonly Vector3 Size => Max - Min;

        /// <summary>
        /// Gets or sets the width of the bounding box.
        /// </summary>
        public float Width
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return Extent.X * 2.0f;
            }
        }

        /// <summary>
        /// Gets or sets the height of the bounding box.
        /// </summary>
        public float Height
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return Extent.Y * 2.0f;
            }
        }

        /// <summary>
        /// Gets or sets the depth of the bounding box.
        /// </summary>
        public float Depth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return Extent.Z * 2.0f;
            }
        }

        /// <summary>
        /// Creates a new bounding box from an array of points.
        /// </summary>
        /// <param name="points">An array of Vector3 points.</param>
        /// <returns>A new BoundingBox that encapsulates the provided points.</returns>
        public static BoundingBox CreateFromPoints(Vector3[] points)
        {
            return CreateFromPoints(points.AsSpan());
        }

        /// <summary>
        /// Creates a new bounding box from a span of points.
        /// </summary>
        /// <param name="points">A span of Vector3 points.</param>
        /// <returns>A new BoundingBox that encapsulates the provided points.</returns>
        public static BoundingBox CreateFromPoints(Span<Vector3> points)
        {
            Vector3 min = new(float.MaxValue);
            Vector3 max = new(float.MinValue);

            for (int i = 0; i < points.Length; ++i)
            {
                min = Vector3.Min(min, points[i]);
                max = Vector3.Max(max, points[i]);
            }

            return new BoundingBox(min, max);
        }

        /// <summary>
        /// Creates a new bounding box by merging two existing bounding boxes.
        /// </summary>
        /// <param name="original">The original bounding box.</param>
        /// <param name="additional">The additional bounding box to merge with the original.</param>
        /// <returns>A new BoundingBox that encompasses both the original and additional bounding boxes.</returns>
        public static BoundingBox CreateMerged(in BoundingBox original, in BoundingBox additional)
        {
            return new BoundingBox(
                Vector3.Min(original.Min, additional.Min),
                Vector3.Max(original.Max, additional.Max)
            );
        }

        /// <summary>
        /// Transforms given <see cref="BoundingBox"/> using a given <see cref="Matrix4x4"/>.
        /// </summary>
        /// <param name="box">The source <see cref="BoundingBox"/>.</param>
        /// <param name="transform">A transformation matrix that might include translation, rotation, or uniform scaling.</param>
        /// <returns>The transformed BoundingBox.</returns>
        public static BoundingBox Transform(in BoundingBox box, in Matrix4x4 transform)
        {
            Transform(box, transform, out BoundingBox result);
            return result;
        }

        /// <summary>
        /// Transforms given <see cref="BoundingBox"/> using a given <see cref="Matrix4x4"/>.
        /// </summary>
        /// <param name="box">The source <see cref="BoundingBox"/>.</param>
        /// <param name="transform">A transformation matrix that might include translation, rotation, or uniform scaling.</param>
        /// <param name="result">The transformed BoundingBox.</param>
        public static void Transform(in BoundingBox box, in Matrix4x4 transform, out BoundingBox result)
        {
            Vector3 vCenter = box.Center;
            Vector3 vExtend = box.Extent;

            Vector3 corner = vExtend * g_BoxOffset[0] + vCenter;

            corner = Vector3.Transform(corner, transform);

            Vector3 min, max;
            min = max = corner;

            for (uint i = 1; i < CornerCount; ++i)
            {
                corner = vExtend * g_BoxOffset[i] + vCenter;
                corner = Vector3.Transform(corner, transform);

                min = Vector3.Min(min, corner);
                max = Vector3.Max(max, corner);
            }

            result = new(min, max);
        }

        /// <summary>
        /// Retrieves the eight corners of the bounding box.
        /// </summary>
        /// <returns>An array of points representing the eight corners of the bounding box.</returns>
        public readonly Vector3[] GetCorners()
        {
            Vector3[] results = new Vector3[CornerCount];
            GetCorners(results);
            return results;
        }

        /// <summary>
        /// Retrieves the eight corners of the bounding box.
        /// </summary>
        /// <returns>An array of points representing the eight corners of the bounding box.</returns>
        public readonly void GetCorners(Vector3[] corners)
        {
            ArgumentNullException.ThrowIfNull(corners);

            if (corners.Length < CornerCount)
            {
                throw new ArgumentOutOfRangeException(nameof(corners), $"GetCorners need at least {CornerCount} elements to copy corners.");
            }

            corners[0] = new Vector3(Min.X, Max.Y, Max.Z);
            corners[1] = new Vector3(Max.X, Max.Y, Max.Z);
            corners[2] = new Vector3(Max.X, Min.Y, Max.Z);
            corners[3] = new Vector3(Min.X, Min.Y, Max.Z);
            corners[4] = new Vector3(Min.X, Max.Y, Min.Z);
            corners[5] = new Vector3(Max.X, Max.Y, Min.Z);
            corners[6] = new Vector3(Max.X, Min.Y, Min.Z);
            corners[7] = new Vector3(Min.X, Min.Y, Min.Z);
        }

        /// <summary>
        /// Retrieves the eight corners of the bounding box.
        /// </summary>
        /// <returns>An pointer of points representing the eight corners of the bounding box.</returns>
        public readonly unsafe void GetCorners(Vector3* corners)
        {
            corners[0] = new Vector3(Min.X, Max.Y, Max.Z);
            corners[1] = new Vector3(Max.X, Max.Y, Max.Z);
            corners[2] = new Vector3(Max.X, Min.Y, Max.Z);
            corners[3] = new Vector3(Min.X, Min.Y, Max.Z);
            corners[4] = new Vector3(Min.X, Max.Y, Min.Z);
            corners[5] = new Vector3(Max.X, Max.Y, Min.Z);
            corners[6] = new Vector3(Max.X, Min.Y, Min.Z);
            corners[7] = new Vector3(Min.X, Min.Y, Min.Z);
        }

        /// <summary>
        /// Calculates the surface area of a <see cref="BoundingBox"/>
        /// </summary>
        /// <returns>The surface area.</returns>
        public readonly float Area()
        {
            float length = Max.X - Min.X;
            float width = Max.Y - Min.Y;
            float height = Max.Z - Min.Z;

            float areaXZ = 2 * length * height;
            float areaXY = 2 * length * width;
            float areaYZ = 2 * width * height;

            return areaXZ + areaXY + areaYZ;
        }

        /// <summary>
        /// Determines whether the bounding box contains the specified point.
        /// </summary>
        /// <param name="point">The point to check for containment.</param>
        /// <returns>A value indicating whether the point is contained within the bounding box.</returns>
        public readonly ContainmentType Contains(in Vector3 point)
        {
            if (Min.X <= point.X && Max.X >= point.X &&
                Min.Y <= point.Y && Max.Y >= point.Y &&
                Min.Z <= point.Z && Max.Z >= point.Z)
            {
                return ContainmentType.Contains;
            }

            return ContainmentType.Disjoint;
        }

        /// <summary>
        /// Determines whether the bounding box contains the specified bounding box.
        /// </summary>
        /// <param name="box">The bounding box to check for containment.</param>
        /// <returns>A value indicating whether the specified bounding box is contained within, intersects, or is disjoint from this bounding box.</returns>
        public readonly ContainmentType Contains(in BoundingBox box)
        {
            if (Max.X < box.Min.X || Min.X > box.Max.X)
            {
                return ContainmentType.Disjoint;
            }

            if (Max.Y < box.Min.Y || Min.Y > box.Max.Y)
            {
                return ContainmentType.Disjoint;
            }

            if (Max.Z < box.Min.Z || Min.Z > box.Max.Z)
            {
                return ContainmentType.Disjoint;
            }

            if (Min.X <= box.Min.X && (box.Max.X <= Max.X &&
                Min.Y <= box.Min.Y && box.Max.Y <= Max.Y) &&
                Min.Z <= box.Min.Z && box.Max.Z <= Max.Z)
            {
                return ContainmentType.Contains;
            }

            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Determines whether the bounding box contains the specified bounding sphere.
        /// </summary>
        /// <param name="sphere">The bounding sphere to check for containment.</param>
        /// <returns>A value indicating whether the specified bounding sphere is contained within, intersects, or is disjoint from this bounding box.</returns>
        public readonly ContainmentType Contains(in BoundingSphere sphere)
        {
            Vector3 vector = Vector3.Clamp(sphere.Center, Min, Max);
            float distance = Vector3.DistanceSquared(sphere.Center, vector);

            if (distance > sphere.Radius * sphere.Radius)
            {
                return ContainmentType.Disjoint;
            }

            if (((Min.X + sphere.Radius <= sphere.Center.X) && (sphere.Center.X <= Max.X - sphere.Radius) && (Max.X - Min.X > sphere.Radius)) &&
               ((Min.Y + sphere.Radius <= sphere.Center.Y) && (sphere.Center.Y <= Max.Y - sphere.Radius) && (Max.Y - Min.Y > sphere.Radius)) &&
               ((Min.Z + sphere.Radius <= sphere.Center.Z) && (sphere.Center.Z <= Max.Z - sphere.Radius) && (Max.Z - Min.Z > sphere.Radius)))
            {
                return ContainmentType.Contains;
            }

            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Checks whether the current <see cref="BoundingBox"/> intersects with a specified <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="box">The other <see cref="BoundingBox"/> to check.</param>
        /// <returns>True if intersects, false otherwise.</returns>
        public readonly bool Intersects(in BoundingBox box)
        {
            if (Max.X < box.Min.X || Min.X > box.Max.X)
            {
                return false;
            }

            if (Max.Y < box.Min.Y || Min.Y > box.Max.Y)
            {
                return false;
            }

            if (Max.Z < box.Min.Z || Min.Z > box.Max.Z)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks whether the current <see cref="BoundingBox"/> intersects with a specified <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The <see cref="BoundingSphere"/> to check for intersection with the current <see cref="BoundingBox"/>.</param>
        /// <returns>True if intersects, false otherwise.</returns>
        public readonly bool Intersects(in BoundingSphere sphere)
        {
            Vector3 clampedVector = Vector3.Clamp(sphere.Center, Min, Max);
            float distance = Vector3.DistanceSquared(sphere.Center, clampedVector);
            return distance <= sphere.Radius * sphere.Radius;
        }

        /// <summary>
        /// Checks whether the current <see cref="BoundingBox"/> intersects with a specified <see cref="BoundingFrustum"/>.
        /// </summary>
        /// <param name="frustum">The <see cref="BoundingFrustum"/> to check for intersection with the current <see cref="BoundingBox"/>.</param>
        /// <returns>True if intersects, false otherwise.</returns>
        public readonly bool Intersects(in BoundingFrustum frustum)
        {
            return frustum.Intersects(this);
        }

        /// <summary>
        /// Checks whether the current <see cref="BoundingBox"/> intersects with a specified <see cref="Ray"/>.
        /// </summary>
        /// <param name="ray">The <see cref="Ray"/> to check for intersection with the current <see cref="BoundingBox"/>.</param>
        /// <returns>Distance value if intersects, null otherwise.</returns>
        public readonly float? Intersects(in Ray ray)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 179

            float distance = 0.0f;
            float tmax = float.MaxValue;

            if (MathF.Abs(ray.Direction.X) < float.Epsilon)
            {
                if (ray.Position.X < Min.X || ray.Position.X > Max.X)
                {
                    return null;
                }
            }
            else
            {
                float inverse = 1.0f / ray.Direction.X;
                float t1 = (Min.X - ray.Position.X) * inverse;
                float t2 = (Max.X - ray.Position.X) * inverse;

                if (t1 > t2)
                {
                    (t2, t1) = (t1, t2);
                }

                distance = Math.Max(t1, distance);
                tmax = Math.Min(t2, tmax);

                if (distance > tmax)
                {
                    return null;
                }
            }

            if (MathF.Abs(ray.Direction.Y) < float.Epsilon)
            {
                if (ray.Position.Y < Min.Y || ray.Position.Y > Max.Y)
                {
                    return null;
                }
            }
            else
            {
                float inverse = 1.0f / ray.Direction.Y;
                float t1 = (Min.Y - ray.Position.Y) * inverse;
                float t2 = (Max.Y - ray.Position.Y) * inverse;

                if (t1 > t2)
                {
                    (t2, t1) = (t1, t2);
                }

                distance = Math.Max(t1, distance);
                tmax = Math.Min(t2, tmax);

                if (distance > tmax)
                {
                    return null;
                }
            }

            if (MathF.Abs(ray.Direction.Z) < float.Epsilon)
            {
                if (ray.Position.Z < Min.Z || ray.Position.Z > Max.Z)
                {
                    return null;
                }
            }
            else
            {
                float inverse = 1.0f / ray.Direction.Z;
                float t1 = (Min.Z - ray.Position.Z) * inverse;
                float t2 = (Max.Z - ray.Position.Z) * inverse;

                if (t1 > t2)
                {
                    (t2, t1) = (t1, t2);
                }

                distance = Math.Max(t1, distance);
                tmax = Math.Min(t2, tmax);

                if (distance > tmax)
                {
                    return null;
                }
            }

            return distance;
        }

        /// <summary>
        /// Determines the intersection type between the bounding box and a plane.
        /// </summary>
        /// <param name="plane">The plane to check for intersection with the bounding box.</param>
        /// <returns>The type of intersection between the bounding box and the plane.</returns>
        public readonly PlaneIntersectionType Intersects(in Plane plane)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 161

            Vector3 min;
            Vector3 max;

            max.X = (plane.Normal.X >= 0.0f) ? Min.X : Max.X;
            max.Y = (plane.Normal.Y >= 0.0f) ? Min.Y : Max.Y;
            max.Z = (plane.Normal.Z >= 0.0f) ? Min.Z : Max.Z;
            min.X = (plane.Normal.X >= 0.0f) ? Max.X : Min.X;
            min.Y = (plane.Normal.Y >= 0.0f) ? Max.Y : Min.Y;
            min.Z = (plane.Normal.Z >= 0.0f) ? Max.Z : Min.Z;

            float distance = Vector3.Dot(plane.Normal, max);

            if (distance + plane.D > 0.0f)
            {
                return PlaneIntersectionType.Front;
            }

            distance = Vector3.Dot(plane.Normal, min);

            if (distance + plane.D < 0.0f)
            {
                return PlaneIntersectionType.Back;
            }

            return PlaneIntersectionType.Intersecting;
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj)
        {
            return obj is BoundingBox value && Equals(value);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(BoundingBox other)
        {
            return Min.Equals(other.Min)
                && Max.Equals(other.Max);
        }

        /// <summary>
        /// Compares two <see cref="BoundingBox"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="BoundingBox"/> on the left hand of the operand.</param>
        /// <param name="right">The <see cref="BoundingBox"/> on the right hand of the operand.</param>
        /// <returns>
        /// True if the current left is equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(BoundingBox left, BoundingBox right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two <see cref="BoundingBox"/> objects for inequality.
        /// </summary>
        /// <param name="left">The <see cref="BoundingBox"/> on the left hand of the operand.</param>
        /// <param name="right">The <see cref="BoundingBox"/> on the right hand of the operand.</param>
        /// <returns>
        /// True if the current left is unequal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(BoundingBox left, BoundingBox right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Min, Max);
        }

        /// <inheritdoc />
        public override readonly string ToString()
        {
            return ToString(format: null, formatProvider: null);
        }

        /// <inheritdoc />
        public readonly string ToString(string? format, IFormatProvider? formatProvider)
        {
            return $"{nameof(BoundingBox)} {{ {nameof(Min)} = {Min.ToString(format, formatProvider)}, {nameof(Max)} = {Max.ToString(format, formatProvider)} }}";
        }

        /// <summary>
        /// Reads a BoundingBox from a stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to read the BoundingBox from.</param>
        /// <param name="endianness">The endianness to use for reading data from the stream.</param>
        /// <returns>The BoundingBox read from the stream.</returns>
        public static BoundingBox Read(Stream stream, Endianness endianness)
        {
            Vector3 min = stream.ReadVector3(endianness);
            Vector3 max = stream.ReadVector3(endianness);
            return new BoundingBox(min, max);
        }

        /// <summary>
        /// Writes the BoundingBox to a stream using the specified endianness.
        /// </summary>
        /// <param name="stream">The stream to write the BoundingBox to.</param>
        /// <param name="endianness">The endianness to use for writing data to the stream.</param>
        public readonly void Write(Stream stream, Endianness endianness)
        {
            stream.WriteVector3(Min, endianness);
            stream.WriteVector3(Max, endianness);
        }
    }
}