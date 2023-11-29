namespace HexaEngine.Mathematics
{
    using System.Numerics;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a Bezier curve with two control points.
    /// </summary>
    [InlineArray(2)]
    public unsafe struct BezierCurve
    {
        private Vector2 _element0;

        /// <summary>
        /// Initializes a new instance of the <see cref="BezierCurve"/> struct.
        /// </summary>
        /// <param name="p0">Control point 0 of the curve.</param>
        /// <param name="p1">Control point 1 of the curve.</param>
        public BezierCurve(Vector2 p0, Vector2 p1)
        {
            this[0] = p0;
            this[1] = p1;
        }

        /// <summary>
        /// Computes a point on the Bezier curve at the given parameter <paramref name="t"/>.
        /// </summary>
        /// <param name="t">The parameter ranging from 0 to 1.</param>
        /// <returns>The computed point on the Bezier curve.</returns>
        public readonly Vector2 ComputePoint(float t)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            float p0 = uuu;
            float p1 = 3 * uu * t;
            float p2 = 3 * u * tt;
            float p3 = ttt;

            Vector2* Q = stackalloc Vector2[] { new Vector2(0, 0), this[0], this[1], new Vector2(1, 1) };

            Vector2 point = new(
                p0 * Q[0].X + p1 * this[1].X + p2 * Q[2].X + p3 * Q[3].X,
                p0 * Q[0].Y + p1 * this[1].Y + p2 * Q[2].Y + p3 * Q[3].Y);

            return point;
        }
    }
}