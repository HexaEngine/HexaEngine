namespace HexaEngine.Mathematics
{
    using System.Numerics;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Represents a structure for computing a Bezier table.
    /// </summary>
    public unsafe struct BezierTable
    {
        private readonly int steps;
        private float* c;
        private float* k;

        /// <summary>
        /// Initializes a new instance of the <see cref="BezierTable"/> struct with the specified number of steps.
        /// </summary>
        /// <param name="steps">The number of steps for the Bezier table.</param>
        public BezierTable(int steps)
        {
            c = (float*)Marshal.AllocHGlobal((steps + 1) * 4 * sizeof(float));
            this.steps = steps;
        }

        /// <summary>
        /// Gets the Bezier control points array.
        /// </summary>
        public readonly float* C => C;

        /// <summary>
        /// Gets the number of steps in the Bezier table.
        /// </summary>
        public readonly int Steps => steps;

        /// <summary>
        /// Computes the Bezier curve given the control points.
        /// </summary>
        /// <param name="p">An array of four control points for the Bezier curve.</param>
        /// <param name="results">An array to store the computed points on the Bezier curve.</param>
        /// <remarks>
        /// The <paramref name="p"/> array must be 4 elements long, and the <paramref name="results"/>
        /// array must be equal to <see cref="Steps"/> + 1 in length.
        /// </remarks>
        public void Compute(Vector2* p /* must be 4 long */, Vector2* results /* must be equals to Steps + 1 long */)
        {
            if (k == null)
            {
                k = c;
                for (uint step = 0; step <= steps; ++step)
                {
                    float t = (float)step / steps;
                    c[step * 4 + 0] = (1 - t) * (1 - t) * (1 - t); // * P0
                    c[step * 4 + 1] = 3 * (1 - t) * (1 - t) * t;   // * P1
                    c[step * 4 + 2] = 3 * (1 - t) * t * t;         // * P2
                    c[step * 4 + 3] = t * t * t;                   // * P3
                }
            }

            for (uint step = 0; step <= steps; ++step)
            {
                Vector2 point = new(
                k[step * 4 + 0] * p[0].X + k[step * 4 + 1] * p[1].X + k[step * 4 + 2] * p[2].X + k[step * 4 + 3] * p[3].X,
                k[step * 4 + 0] * p[0].Y + k[step * 4 + 1] * p[1].Y + k[step * 4 + 2] * p[2].Y + k[step * 4 + 3] * p[3].Y);
                results[step] = point;
            }
        }

        /// <summary>
        /// Computes points on the Bezier curve using the provided <paramref name="curve"/> and stores the results in the specified <paramref name="results"/>.
        /// </summary>
        /// <param name="curve">The Bezier curve with four control points.</param>
        /// <param name="results">The array to store the computed points. Must be equal to <see cref="Steps"/> + 1 in length.</param>
        public void Compute(BezierCurve curve, Vector2* results /* must be equals to Steps + 1 long */)
        {
            if (k == null)
            {
                k = c;
                for (uint step = 0; step <= steps; ++step)
                {
                    float t = (float)step / steps;
                    c[step * 4 + 0] = (1 - t) * (1 - t) * (1 - t); // * P0
                    c[step * 4 + 1] = 3 * (1 - t) * (1 - t) * t;   // * P1
                    c[step * 4 + 2] = 3 * (1 - t) * t * t;         // * P2
                    c[step * 4 + 3] = t * t * t;                   // * P3
                }
            }

            Vector2* Q = stackalloc Vector2[] { new Vector2(0, 0), curve[0], curve[1], new Vector2(1, 1) };

            for (uint step = 0; step <= steps; ++step)
            {
                Vector2 point = new(
                k[step * 4 + 0] * Q[0].X + k[step * 4 + 1] * Q[1].X + k[step * 4 + 2] * Q[2].X + k[step * 4 + 3] * Q[3].X,
                k[step * 4 + 0] * Q[0].Y + k[step * 4 + 1] * Q[1].Y + k[step * 4 + 2] * Q[2].Y + k[step * 4 + 3] * Q[3].Y);
                results[step] = point;
            }
        }

        /// <summary>
        /// Computes a single point on the Bezier curve given a parameter <paramref name="t"/>.
        /// </summary>
        /// <param name="p">Array of control points. It must be 4 long.</param>
        /// <param name="t">Parameter value typically ranging from 0 to 1.</param>
        /// <returns>The computed point on the Bezier curve.</returns>
        public readonly Vector2 ComputePoint(Vector2* p, float t)
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

            Vector2 point = new(
                p0 * p[0].X + p1 * p[1].X + p2 * p[2].X + p3 * p[3].X,
                p0 * p[0].Y + p1 * p[1].Y + p2 * p[2].Y + p3 * p[3].Y);

            return point;
        }

        /// <summary>
        /// Releases the resources associated with the Bezier table.
        /// </summary>
        public void Release()
        {
            Marshal.FreeHGlobal((nint)c);
            c = null;
            k = null;
        }
    }
}