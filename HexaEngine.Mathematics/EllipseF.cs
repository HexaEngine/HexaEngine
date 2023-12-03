namespace HexaEngine.Mathematics
{
    using System;
    using System.Numerics;

    /// <summary>
    /// Represents an ellipse in floating-point coordinates.
    /// </summary>
    public struct EllipseF : IEquatable<EllipseF>
    {
        /// <summary>
        /// Gets or sets the center of the ellipse.
        /// </summary>
        public Vector2 Center;

        /// <summary>
        /// Gets or sets the radius of the ellipse.
        /// </summary>
        public Vector2 Radius;

        /// <summary>
        /// Initializes a new instance of the <see cref="EllipseF"/> struct with specified center and radius vectors.
        /// </summary>
        public EllipseF(Vector2 origin, Vector2 radius)
        {
            Center = origin;
            Radius = radius;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EllipseF"/> struct with specified center and radius value.
        /// </summary>
        public EllipseF(Vector2 origin, float radius)
        {
            Center = origin;
            Radius = new(radius);
        }

        /// <summary>
        /// Determines whether the ellipse contains the specified point.
        /// </summary>
        public readonly bool Contains(Vector2 point)
        {
            Vector2 normalized = (point - Center) / Radius;
            return normalized.X * normalized.X + normalized.Y * normalized.Y <= 1.0f;
        }

        /// <summary>
        /// Determines whether the ellipse contains another ellipse.
        /// </summary>
        public readonly bool Contains(EllipseF other)
        {
            Vector2 distanceVector = other.Center - Center;
            Vector2 radiusDifference = Radius - other.Radius;

            return distanceVector.X * distanceVector.X / (radiusDifference.X * radiusDifference.X) + distanceVector.Y * distanceVector.Y / (radiusDifference.Y * radiusDifference.Y) <= 1.0f;
        }

        /// <summary>
        /// Determines whether the ellipse contains a rectangle.
        /// </summary>
        public readonly bool Contains(RectangleF rect)
        {
            Vector2 rectCenter = rect.Offset + rect.Size / 2;
            Vector2 distanceVector = Center - rectCenter;
            Vector2 normalizedDistance = new(distanceVector.X / Radius.X, distanceVector.Y / Radius.Y);
            return normalizedDistance.X * normalizedDistance.X + normalizedDistance.Y * normalizedDistance.Y <= 1.0f;
        }

        /// <summary>
        /// Determines whether the ellipse intersects with a rectangle.
        /// </summary>
        public readonly bool Intersects(RectangleF rect)
        {
            float closestX = Math.Clamp(Center.X, rect.Left, rect.Right);
            float closestY = Math.Clamp(Center.Y, rect.Top, rect.Bottom);

            float distanceX = Center.X - closestX;
            float distanceY = Center.Y - closestY;

            return distanceX * distanceX / (Radius.X * Radius.X) + distanceY * distanceY / (Radius.Y * Radius.Y) <= 1.0f;
        }

        /// <summary>
        /// Determines whether the ellipse intersects with another ellipse.
        /// </summary>
        public readonly bool Intersects(EllipseF other)
        {
            Vector2 distanceVector = other.Center - Center;
            Vector2 radiusSum = Radius + other.Radius;

            float normalizedX = distanceVector.X / radiusSum.X;
            float normalizedY = distanceVector.Y / radiusSum.Y;

            return normalizedX * normalizedX + normalizedY * normalizedY <= 1.0f;
        }

        /// <summary>
        /// Merges the current ellipse with another ellipse.
        /// </summary>
        public readonly EllipseF Merge(EllipseF other)
        {
            Vector2 center = (other.Center + Center) / 2;
            Vector2 distanceVector = other.Center - center;

            Vector2 radiusSum = Vector2.Max(Radius, other.Radius) + distanceVector;

            return new(center, radiusSum);
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj)
        {
            return obj is EllipseF f && Equals(f);
        }

        /// <inheritdoc/>
        public readonly bool Equals(EllipseF other)
        {
            return Center.Equals(other.Center) &&
                   Radius.Equals(other.Radius);
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Center, Radius);
        }

        /// <summary>
        /// Equality operator for comparing two ellipses.
        /// </summary>
        public static bool operator ==(EllipseF left, EllipseF right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator for comparing two ellipses.
        /// </summary>
        public static bool operator !=(EllipseF left, EllipseF right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Adds two ellipses by combining their radius vectors.
        /// </summary>
        /// <param name="left">The first ellipse.</param>
        /// <param name="right">The second ellipse.</param>
        /// <returns>A new ellipse resulting from the addition.</returns>
        public static EllipseF operator +(EllipseF left, EllipseF right)
        {
            return new((left.Center + right.Center) / 2, left.Radius + right.Radius);
        }

        /// <summary>
        /// Subtracts the radius vector of the second ellipse from the first ellipse.
        /// </summary>
        /// <param name="left">The first ellipse.</param>
        /// <param name="right">The second ellipse.</param>
        /// <returns>A new ellipse resulting from the subtraction.</returns>
        public static EllipseF operator -(EllipseF left, EllipseF right)
        {
            return new((left.Center + right.Center) / 2, left.Radius - right.Radius);
        }

        /// <summary>
        /// Translates the ellipse by adding the specified vector to its center.
        /// </summary>
        /// <param name="left">The ellipse.</param>
        /// <param name="right">The vector to add to the ellipse's center.</param>
        /// <returns>A new ellipse resulting from the translation.</returns>
        public static EllipseF operator +(EllipseF left, Vector2 right)
        {
            return new(left.Center, left.Radius + right);
        }

        /// <summary>
        /// Translates the ellipse by subtracting the specified vector from its center.
        /// </summary>
        /// <param name="left">The ellipse.</param>
        /// <param name="right">The vector to subtract from the ellipse's center.</param>
        /// <returns>A new ellipse resulting from the translation.</returns>
        public static EllipseF operator -(EllipseF left, Vector2 right)
        {
            return new(left.Center, left.Radius - right);
        }

        /// <summary>
        /// Scales the ellipse by adding the specified value to its radius.
        /// </summary>
        /// <param name="left">The ellipse.</param>
        /// <param name="right">The value to add to the ellipse's radius.</param>
        /// <returns>A new ellipse resulting from the scaling.</returns>
        public static EllipseF operator +(EllipseF left, float right)
        {
            return new(left.Center, left.Radius + new Vector2(right));
        }

        /// <summary>
        /// Scales the ellipse by subtracting the specified value from its radius.
        /// </summary>
        /// <param name="left">The ellipse.</param>
        /// <param name="right">The value to subtract from the ellipse's radius.</param>
        /// <returns>A new ellipse resulting from the scaling.</returns>
        public static EllipseF operator -(EllipseF left, float right)
        {
            return new(left.Center, left.Radius - new Vector2(right));
        }

        /// <summary>
        /// Scales the ellipse by multiplying its radius by the specified value.
        /// </summary>
        /// <param name="left">The ellipse.</param>
        /// <param name="right">The value to multiply the ellipse's radius by.</param>
        /// <returns>A new ellipse resulting from the scaling.</returns>
        public static EllipseF operator *(EllipseF left, float right)
        {
            return new(left.Center, left.Radius * right);
        }

        /// <summary>
        /// Scales the ellipse by dividing its radius by the specified value.
        /// </summary>
        /// <param name="left">The ellipse.</param>
        /// <param name="right">The value to divide the ellipse's radius by.</param>
        /// <returns>A new ellipse resulting from the scaling.</returns>
        public static EllipseF operator /(EllipseF left, float right)
        {
            return new(left.Center, left.Radius / right);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"<Origin: {Center}, Radius: {Radius}>";
        }
    }
}