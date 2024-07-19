/*
 * Based of GIMP source code.
 * This part is Licensed under GPL-3.0.
 * Fair use of code, this code section is trivial to the rest.
 */

namespace HexaEngine.Mathematics
{
    using System.Text.Json.Serialization;

    public unsafe struct Curve
    {
        public Curve(IEnumerable<CurvePoint> points, int initSampleCount = 256)
        {
            Points = new(points);
            Samples = new float[initSampleCount];
        }

        public Curve()
        {
            Points = [];
            Samples = new float[256];
        }

        public List<CurvePoint> Points;

        [JsonIgnore, Newtonsoft.Json.JsonIgnore]
        public float[] Samples;

        public CurveType Type;

        public void Compute()
        {
            Samples ??= new float[256];
            CalculateCurve(ref this);
        }

        public static void CalculateCurve(ref Curve curve)
        {
            switch (curve.Type)
            {
                case CurveType.Smooth:
                    /*  Initialize boundary curve points */
                    if (curve.Points.Count > 0)
                    {
                        CurvePoint point;
                        int boundary;

                        point = curve.Points[0];
                        boundary = (int)Math.Round(point.X * (double)(curve.Samples.Length - 1));

                        for (int i = 0; i < boundary; i++)
                        {
                            curve.Samples[i] = point.Y;
                        }

                        point = curve.Points[^1];
                        boundary = (int)Math.Round(point.X * (double)(curve.Samples.Length - 1));

                        for (int i = boundary; i < curve.Samples.Length; i++)
                        {
                            curve.Samples[i] = point.Y;
                        }
                    }

                    for (int i = 0; i < curve.Points.Count - 1; i++)
                    {
                        int p1 = Math.Max(i - 1, 0);
                        int p2 = i;
                        int p3 = i + 1;
                        int p4 = Math.Min(i + 2, curve.Points.Count - 1);

                        if (curve.Points[p2].Type == CurvePointType.Corner)
                        {
                            p1 = p2;
                        }

                        if (curve.Points[p3].Type == CurvePointType.Corner)
                        {
                            p4 = p3;
                        }

                        PlotCurve(ref curve, p1, p2, p3, p4);
                    }

                    // Ensure that the control points are used exactly
                    for (int i = 0; i < curve.Points.Count; i++)
                    {
                        float x = curve.Points[i].X;
                        float y = curve.Points[i].Y;
                        int index = (int)Math.Round(x * (double)(curve.Samples.Length - 1));
                        curve.Samples[index] = y;
                    }
                    break;

                case CurveType.Freehand:
                    // Handle freehand case if needed
                    break;
            }
        }

        private static void PlotCurve(ref Curve curve, int p1, int p2, int p3, int p4)
        {
            double x0 = curve.Points[p2].X;
            double y0 = curve.Points[p2].Y;
            double x3 = curve.Points[p3].X;
            double y3 = curve.Points[p3].Y;

            double dx = x3 - x0;
            double dy = y3 - y0;

            if (dx <= double.Epsilon)
            {
                int index = (int)Math.Round(x0 * (curve.Samples.Length - 1));
                curve.Samples[index] = (float)y3;
                return;
            }

            double y1, y2;
            if (p1 == p2 && p3 == p4)
            {
                y1 = y0 + dy / 3.0;
                y2 = y0 + dy * 2.0 / 3.0;
            }
            else if (p1 == p2)
            {
                double slope = (curve.Points[p4].Y - y0) / (curve.Points[p4].X - x0);
                y2 = y3 - slope * dx / 3.0;
                y1 = y0 + (y2 - y0) / 2.0;
            }
            else if (p3 == p4)
            {
                double slope = (y3 - curve.Points[p1].Y) / (x3 - curve.Points[p1].X);
                y1 = y0 + slope * dx / 3.0;
                y2 = y3 + (y1 - y3) / 2.0;
            }
            else
            {
                double slope1 = (y3 - curve.Points[p1].Y) / (x3 - curve.Points[p1].X);
                y1 = y0 + slope1 * dx / 3.0;
                double slope2 = (curve.Points[p4].Y - y0) / (curve.Points[p4].X - x0);
                y2 = y3 - slope2 * dx / 3.0;
            }

            for (int i = 0; i <= (int)Math.Round(dx * (curve.Samples.Length - 1)); i++)
            {
                double t = i / dx / (curve.Samples.Length - 1);
                double y = y0 * (1 - t) * (1 - t) * (1 - t) +
                           3 * y1 * (1 - t) * (1 - t) * t +
                           3 * y2 * (1 - t) * t * t +
                           y3 * t * t * t;
                int index = i + (int)Math.Round(x0 * (curve.Samples.Length - 1));
                if (index < curve.Samples.Length)
                    curve.Samples[index] = (float)Math.Clamp(y, 0, 1);
            }
        }
    }
}