namespace HexaEngine.Core.UI
{
    using Hexa.NET.ImGui;
    using Hexa.NET.Mathematics;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public static unsafe class ImGuiCurveEditor2
    {
        private enum TYPE
        {
            LINEAR,

            QUADIN, // t^2
            QUADOUT,
            QUADINOUT,
            CUBICIN, // t^3
            CUBICOUT,
            CUBICINOUT,
            QUARTIN, // t^4
            QUARTOUT,
            QUARTINOUT,
            QUINTIN, // t^5
            QUINTOUT,
            QUINTINOUT,
            SINEIN, // Math.Sin(t)
            SINEOUT,
            SINEINOUT,
            EXPOIN, // 2^t
            EXPOOUT,
            EXPOINOUT,
            CIRCIN, // Math.Sqrt(1-t^2)
            CIRCOUT,
            CIRCINOUT,
            ELASTICIN, // exponentially decaying sine wave
            ELASTICOUT,
            ELASTICINOUT,
            BACKIN, // overshooting cubic easing: (s+1)*t^3 - s*t^2
            BACKOUT,
            BACKINOUT,
            BOUNCEIN, // exponentially decaying parabolic bounce
            BOUNCEOUT,
            BOUNCEINOUT,

            SINESQUARE,  // gapjumper's
            EXPONENTIAL, // gapjumper's
            SCHUBRING1,  // terry schubring's formula 1
            SCHUBRING2,  // terry schubring's formula 2
            SCHUBRING3,  // terry schubring's formula 3

            SINPI2, // tomas cepeda's
            SWING,  // tomas cepeda's & lquery's
        }

        private static double tweenbounceout(double p)
        {
            return p < 4 / 11.0 ? 121 * p * p / 16.0
                : p < 8 / 11.0 ? 363 / 40.0 * p * p - 99 / 10.0 * p + 17 / 5.0
                : p < 9 / 10.0 ? 4356 / 361.0 * p * p - 35442 / 1805.0 * p + 16061 / 1805.0
                : 54 / 5.0 * p * p - 513 / 25.0 * p + 268 / 25.0;
        }

        private static double ease(int easetype, double t)
        {
            const double d = 1.0f;
            const double pi = 3.1415926535897932384626433832795;
            const double pi2 = 3.1415926535897932384626433832795 / 2;

            double p = t / d;

            switch ((TYPE)easetype)
            {
                // Modeled after the line y = x
                default:
                case TYPE.LINEAR:
                    {
                        return p;
                    }

                // Modeled after the parabola y = x^2
                case TYPE.QUADIN:
                    {
                        return p * p;
                    }

                // Modeled after the parabola y = -x^2 + 2x
                case TYPE.QUADOUT:
                    {
                        return -(p * (p - 2));
                    }

                // Modeled after the piecewise quadratic
                // y = (1/2)((2x)^2)             ; [0, 0.5)
                // y = -(1/2)((2x-1)*(2x-3) - 1) ; [0.5, 1]
                case TYPE.QUADINOUT:
                    {
                        if (p < 0.5)
                        {
                            return 2 * p * p;
                        }
                        else
                        {
                            return -2 * p * p + 4 * p - 1;
                        }
                    }

                // Modeled after the cubic y = x^3
                case TYPE.CUBICIN:
                    {
                        return p * p * p;
                    }

                // Modeled after the cubic y = (x - 1)^3 + 1
                case TYPE.CUBICOUT:
                    {
                        double f = p - 1;
                        return f * f * f + 1;
                    }

                // Modeled after the piecewise cubic
                // y = (1/2)((2x)^3)       ; [0, 0.5)
                // y = (1/2)((2x-2)^3 + 2) ; [0.5, 1]
                case TYPE.CUBICINOUT:
                    {
                        if (p < 0.5)
                        {
                            return 4 * p * p * p;
                        }
                        else
                        {
                            double f = 2 * p - 2;
                            return 0.5 * f * f * f + 1;
                        }
                    }

                // Modeled after the quartic x^4
                case TYPE.QUARTIN:
                    {
                        return p * p * p * p;
                    }

                // Modeled after the quartic y = 1 - (x - 1)^4
                case TYPE.QUARTOUT:
                    {
                        double f = p - 1;
                        return f * f * f * (1 - p) + 1;
                    }

                // Modeled after the piecewise quartic
                // y = (1/2)((2x)^4)        ; [0, 0.5)
                // y = -(1/2)((2x-2)^4 - 2) ; [0.5, 1]
                case TYPE.QUARTINOUT:
                    {
                        if (p < 0.5)
                        {
                            return 8 * p * p * p * p;
                        }
                        else
                        {
                            double f = p - 1;
                            return -8 * f * f * f * f + 1;
                        }
                    }

                // Modeled after the quintic y = x^5
                case TYPE.QUINTIN:
                    {
                        return p * p * p * p * p;
                    }

                // Modeled after the quintic y = (x - 1)^5 + 1
                case TYPE.QUINTOUT:
                    {
                        double f = p - 1;
                        return f * f * f * f * f + 1;
                    }

                // Modeled after the piecewise quintic
                // y = (1/2)((2x)^5)       ; [0, 0.5)
                // y = (1/2)((2x-2)^5 + 2) ; [0.5, 1]
                case TYPE.QUINTINOUT:
                    {
                        if (p < 0.5)
                        {
                            return 16 * p * p * p * p * p;
                        }
                        else
                        {
                            double f = 2 * p - 2;
                            return 0.5 * f * f * f * f * f + 1;
                        }
                    }

                // Modeled after quarter-cycle of sine wave
                case TYPE.SINEIN:
                    {
                        return Math.Sin((p - 1) * pi2) + 1;
                    }

                // Modeled after quarter-cycle of sine wave (different phase)
                case TYPE.SINEOUT:
                    {
                        return Math.Sin(p * pi2);
                    }

                // Modeled after half sine wave
                case TYPE.SINEINOUT:
                    {
                        return 0.5 * (1 - Math.Cos(p * pi));
                    }

                // Modeled after shifted quadrant IV of unit circle
                case TYPE.CIRCIN:
                    {
                        return 1 - Math.Sqrt(1 - p * p);
                    }

                // Modeled after shifted quadrant II of unit circle
                case TYPE.CIRCOUT:
                    {
                        return Math.Sqrt((2 - p) * p);
                    }

                // Modeled after the piecewise circular function
                // y = (1/2)(1 - Math.Sqrt(1 - 4x^2))           ; [0, 0.5)
                // y = (1/2)(Math.Sqrt(-(2x - 3)*(2x - 1)) + 1) ; [0.5, 1]
                case TYPE.CIRCINOUT:
                    {
                        if (p < 0.5)
                        {
                            return 0.5 * (1 - Math.Sqrt(1 - 4 * (p * p)));
                        }
                        else
                        {
                            return 0.5 * (Math.Sqrt(-(2 * p - 3) * (2 * p - 1)) + 1);
                        }
                    }

                // Modeled after the exponential function y = 2^(10(x - 1))
                case TYPE.EXPOIN:
                    {
                        return (p == 0.0) ? p : Math.Pow(2, 10 * (p - 1));
                    }

                // Modeled after the exponential function y = -2^(-10x) + 1
                case TYPE.EXPOOUT:
                    {
                        return (p == 1.0) ? p : 1 - Math.Pow(2, -10 * p);
                    }

                // Modeled after the piecewise exponential
                // y = (1/2)2^(10(2x - 1))         ; [0,0.5)
                // y = -(1/2)*2^(-10(2x - 1))) + 1 ; [0.5,1]
                case TYPE.EXPOINOUT:
                    {
                        if (p == 0.0 || p == 1.0)
                            return p;

                        if (p < 0.5)
                        {
                            return 0.5 * Math.Pow(2, 20 * p - 10);
                        }
                        else
                        {
                            return -0.5 * Math.Pow(2, -20 * p + 10) + 1;
                        }
                    }

                // Modeled after the damped sine wave y = Math.Sin(13pi/2*x)*Math.Pow(2, 10 * (x - 1))
                case TYPE.ELASTICIN:
                    {
                        return Math.Sin(13 * pi2 * p) * Math.Pow(2, 10 * (p - 1));
                    }

                // Modeled after the damped sine wave y = Math.Sin(-13pi/2*(x + 1))*Math.Pow(2, -10x) + 1
                case TYPE.ELASTICOUT:
                    {
                        return Math.Sin(-13 * pi2 * (p + 1)) * Math.Pow(2, -10 * p) + 1;
                    }

                // Modeled after the piecewise exponentially-damped sine wave:
                // y = (1/2)*Math.Sin(13pi/2*(2*x))*Math.Pow(2, 10 * ((2*x) - 1))      ; [0,0.5)
                // y = (1/2)*(Math.Sin(-13pi/2*((2x-1)+1))*Math.Pow(2,-10(2*x-1)) + 2) ; [0.5, 1]
                case TYPE.ELASTICINOUT:
                    {
                        if (p < 0.5)
                        {
                            return 0.5 * Math.Sin(13 * pi2 * (2 * p)) * Math.Pow(2, 10 * (2 * p - 1));
                        }
                        else
                        {
                            return 0.5 * (Math.Sin(-13 * pi2 * (2 * p - 1 + 1)) * Math.Pow(2, -10 * (2 * p - 1)) + 2);
                        }
                    }

                // Modeled (originally) after the overshooting cubic y = x^3-x*Math.Sin(x*pi)
                case TYPE.BACKIN:
                    { /*
        return p * p * p - p * Math.Sin(p * pi); */
                        double s = 1.70158f;
                        return p * p * ((s + 1) * p - s);
                    }

                // Modeled (originally) after overshooting cubic y = 1-((1-x)^3-(1-x)*Math.Sin((1-x)*pi))
                case TYPE.BACKOUT:
                    { /*
        double f = (1 - p);
        return 1 - (f * f * f - f * Math.Sin(f * pi)); */
                        double s = 1.70158f;
                        --p;
                        return 1.0f * (p * p * ((s + 1) * p + s) + 1);
                    }

                // Modeled (originally) after the piecewise overshooting cubic function:
                // y = (1/2)*((2x)^3-(2x)*Math.Sin(2*x*pi))           ; [0, 0.5)
                // y = (1/2)*(1-((1-x)^3-(1-x)*Math.Sin((1-x)*pi))+1) ; [0.5, 1]
                case TYPE.BACKINOUT:
                    { /*
        if(p < 0.5) {
            double f = 2 * p;
            return 0.5 * (f * f * f - f * Math.Sin(f * pi));
        }
        else {
            double f = (1 - (2*p - 1));
            return 0.5 * (1 - (f * f * f - f * Math.Sin(f * pi))) + 0.5;
        } */
                        double s = 1.70158f * 1.525f;
                        if (p < 0.5)
                        {
                            p *= 2;
                            return 0.5 * p * p * (p * s + p - s);
                        }
                        else
                        {
                            p = p * 2 - 2;
                            return 0.5 * (2 + p * p * (p * s + p + s));
                        }
                    }

                case TYPE.BOUNCEIN:
                    {
                        return 1 - tweenbounceout(1 - p);
                    }

                case TYPE.BOUNCEOUT:
                    {
                        return tweenbounceout(p);
                    }

                case TYPE.BOUNCEINOUT:
                    {
                        if (p < 0.5)
                        {
                            return 0.5 * (1 - tweenbounceout(1 - p * 2));
                        }
                        else
                        {
                            return 0.5 * tweenbounceout(p * 2 - 1) + 0.5;
                        }
                    }

                case TYPE.SINESQUARE:
                    {
                        double A = Math.Sin(p * pi2);
                        return A * A;
                    }

                case TYPE.EXPONENTIAL:
                    {
                        return 1 / (1 + Math.Exp(6 - 12 * p));
                    }

                case TYPE.SCHUBRING1:
                    {
                        return 2 * (p + (0.5f - p) * Math.Abs(0.5f - p)) - 0.5f;
                    }

                case TYPE.SCHUBRING2:
                    {
                        double p1pass = 2 * (p + (0.5f - p) * Math.Abs(0.5f - p)) - 0.5f;
                        double p2pass = 2 * (p1pass + (0.5f - p1pass) * Math.Abs(0.5f - p1pass)) - 0.5f;
                        double pAvg = (p1pass + p2pass) / 2;
                        return pAvg;
                    }

                case TYPE.SCHUBRING3:
                    {
                        double p1pass = 2 * (p + (0.5f - p) * Math.Abs(0.5f - p)) - 0.5f;
                        double p2pass = 2 * (p1pass + (0.5f - p1pass) * Math.Abs(0.5f - p1pass)) - 0.5f;
                        return p2pass;
                    }

                case TYPE.SWING:
                    {
                        return -Math.Cos(pi * p) * 0.5 + 0.5;
                    }

                case TYPE.SINPI2:
                    {
                        return Math.Sin(p * pi2);
                    }
            }
        }

        private const float CurveTerminator = -10000;

        private static float[] coefs =
        [
            -1.0f,
            2.0f,
            -1.0f,
            0.0f,
            3.0f,
            -5.0f,
            0.0f,
            2.0f,
            -3.0f,
            4.0f,
            1.0f,
            0.0f,
            1.0f,
            -1.0f,
            0.0f,
            0.0f
        ];

        private static void spline(int DIM, float* key, int num, float t, float* v)
        {
            int size = DIM + 1;

            // find key
            int k = 0;
            while (key[k * size] < t)
                k++;

            float key0 = key[(k - 1) * size];
            float key1 = key[k * size];

            // interpolant
            float h = (t - key0) / (key1 - key0);

            // init result
            for (int i = 0; i < DIM; i++)
                v[i] = 0.0f;

            // add basis functions
            for (int i = 0; i < 4; ++i)
            {
                Span<float> co = coefs.AsSpan(4 * i);
                float b = 0.5f * (((co[0] * h + co[1]) * h + co[2]) * h + co[3]);

                int kn = Math.Clamp(k + i - 2, 0, num - 1);
                for (int j = 0; j < DIM; j++)
                    v[j] += b * key[kn * size + j + 1];
            }
        }

        private static float CurveValueSmooth(float p, int maxpoints, Vector2* points)
        {
            if (maxpoints < 2 || points == null)
                return 0;
            if (p < 0)
                return points[0].Y;

            float* output = stackalloc float[4];

            spline(1, (float*)points, maxpoints, p, output);

            return output[0];
        }

        private static float CurveValue(float p, int maxpoints, Vector2* points)
        {
            if (maxpoints < 2 || points == null)
                return 0;
            if (p < 0)
                return points[0].Y;

            int left = 0;
            while (left < maxpoints && points[left].X < p && points[left].X != -1)
                left++;
            if (left != 0)
                left--;

            if (left == maxpoints - 1)
                return points[maxpoints - 1].Y;

            float d = (p - points[left].X) / (points[left + 1].X - points[left].X);

            return points[left].Y + (points[left + 1].Y - points[left].Y) * d;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Remap(float v, float a, float b, float c, float d)
        {
            return (c + (d - c) * (v - a) / (b - a));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2 Remap(Vector2 v, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            return new Vector2(Remap(v.X, a.X, b.X, c.X, d.X), Remap(v.Y, a.Y, b.Y, c.Y, d.Y));
        }

        private enum ActionType
        {
            None,
            AddPoint,
            DeletePoint
        };

        // smooth curve
        private const int Smoothness = 256;

        private static string[] items = {
        "Custom",

        "Linear",          "Quad in",     "Quad out",   "Quad in  out",  "Cubic in",   "Cubic out",
        "Cubic in  out",   "Quart in",    "Quart out",  "Quart in  out", "Quint in",   "Quint out",
        "Quint in  out",   "Sine in",     "Sine out",   "Sine in  out",  "Expo in",    "Expo out",
        "Expo in  out",    "Circ in",     "Circ out",   "Circ in  out",  "Elastic in", "Elastic out",
        "Elastic in  out", "Back in",     "Back out",   "Back in  out",  "Bounce in",  "Bounce out",
        "Bounce in out",

        "Sine square",     "Exponential",

        "Schubring1",      "Schubring2",  "Schubring3",

        "SinPi2",          "Swing"
    };

        public static int Curve(string label, Vector2 size, int maxpoints, ref int pointCount, Vector2* points, int* selection, Vector2 rangeMin, Vector2 rangeMax)
        {
            int modified = 0;
            int i;
            if (maxpoints < 2 || points == null)
                return 0;

            if (points[0].X <= CurveTerminator)
            {
                points[0] = rangeMin;
                points[1] = rangeMax;
                points[2].X = CurveTerminator;
            }

            ImGuiWindow* window = ImGui.GetCurrentWindow();
            ImGuiContext* g = ImGui.GetCurrentContext();

            uint id = ImGui.ImGuiWindowGetID(window, label, (byte*)null);
            if (window->SkipItems != 0)
                return 0;

            ImRect bb = new() { Min = window->DC.CursorPos, Max = window->DC.CursorPos + size };
            ImGui.ItemSizeRect(bb, -1);
            if (!ImGui.ItemAdd(bb, 0, null, 0))
                return 0;

            ImGui.PushID(label);

            int currentSelection = selection != null ? *selection : -1;

            bool hovered = ImGui.ItemHoverable(bb, id, 0);

            ImGuiStylePtr style = ImGui.GetStyle();
            ImGuiIOPtr io = ImGui.GetIO();
            ImGui.RenderFrame(bb.Min, bb.Max, ImGui.GetColorU32(ImGuiCol.FrameBg, 1), true, style.FrameRounding);

            float ht = bb.Max.Y - bb.Min.Y;
            float wd = bb.Max.X - bb.Min.X;

            int hoveredPoint = -1;

            const float pointRadiusInPixels = 5.0f;

            // Handle point selection
            if (hovered)
            {
                Vector2 hoverPos = (io.MousePos - bb.Min) / (bb.Max - bb.Min);
                hoverPos.Y = 1.0f - hoverPos.Y;

                Vector2 pos = Remap(hoverPos, new Vector2(0, 0), new Vector2(1, 1), rangeMin, rangeMax);

                int left = 0;
                while (left < pointCount && points[left].X < pos.X)
                    left++;
                if (left != 0)
                    left--;

                Vector2 hoverPosScreen = Remap(hoverPos, new Vector2(0, 0), new Vector2(1, 1), bb.Min, bb.Max);
                Vector2 p1s = Remap(points[left], rangeMin, rangeMax, bb.Min, bb.Max);
                Vector2 p2s = Remap(points[left + 1], rangeMin, rangeMax, bb.Min, bb.Max);

                float p1d = ((p1s - hoverPosScreen).Length());
                float p2d = ((p2s - hoverPosScreen).Length());

                if (p1d < pointRadiusInPixels)
                    hoveredPoint = left;

                if (p2d < pointRadiusInPixels)
                    hoveredPoint = left + 1;

                if (io.MouseDown[0])
                {
                    if (currentSelection == -1)
                        currentSelection = hoveredPoint;
                }
                else
                    currentSelection = -1;

                ActionType action = ActionType.None;

                if (currentSelection == -1)
                {
                    if (io.MouseDoubleClicked[0] || ImGui.IsMouseDragging(0))
                        action = ActionType.AddPoint;
                }
                else if (io.MouseDoubleClicked[0])
                    action = ActionType.DeletePoint;

                if (action == ActionType.AddPoint)
                {
                    if (pointCount < maxpoints)
                    {
                        // select
                        currentSelection = left + 1;

                        ++pointCount;
                        for (i = pointCount; i > left; --i)
                            points[i] = points[i - 1];

                        points[left + 1] = pos;

                        if (pointCount < maxpoints)
                            points[pointCount].X = CurveTerminator;
                    }
                }
                else if (action == ActionType.DeletePoint)
                {
                    // delete point
                    if (currentSelection > 0 && currentSelection < maxpoints - 1)
                    {
                        for (i = currentSelection; i < maxpoints - 1; ++i)
                            points[i] = points[i + 1];

                        --pointCount;
                        points[pointCount].X = CurveTerminator;
                        currentSelection = -1;
                    }
                }
            }

            // handle point dragging
            bool draggingPoint = ImGui.IsMouseDragging(0) && currentSelection != -1;

            if (draggingPoint)
            {
                if (selection != null)
                    ImGui.SetActiveID(id, window);

                ImGui.SetFocusID(id, window);
                ImGui.FocusWindow(window, ImGuiFocusRequestFlags.None);

                modified = 1;

                Vector2 pos = (io.MousePos - bb.Min) / (bb.Max - bb.Min);

                // constrain Y to min/max
                pos.Y = 1.0f - pos.Y;
                pos = Remap(pos, new Vector2(0, 0), new Vector2(1, 1), rangeMin, rangeMax);

                // constrain X to the min left/ max right
                float pointXRangeMin = (currentSelection > 0) ? points[currentSelection - 1].X : rangeMin.X;
                float pointXRangeMax = (currentSelection + 1 < pointCount) ? points[currentSelection + 1].X : rangeMax.X;

                pos = Vector2.Clamp(pos, new Vector2(pointXRangeMin, rangeMin.Y), new Vector2(pointXRangeMax, rangeMax.Y));

                points[currentSelection] = pos;

                // snap X first/last to min/max
                if (points[0].X < points[pointCount - 1].X)
                {
                    points[0].X = rangeMin.Y;
                    points[pointCount - 1].X = rangeMax.X;
                }
                else
                {
                    points[0].X = rangeMax.X;
                    points[pointCount - 1].X = rangeMin.Y;
                }
            }

            if (!ImGui.IsMouseDragging(0) && ImGui.GetActiveID() == id && selection != null && *selection != -1 && currentSelection == -1)
            {
                ImGui.ClearActiveID();
            }

            uint gridColor1 = ImGui.GetColorU32(ImGuiCol.TextDisabled, 0.5f);
            uint gridColor2 = ImGui.GetColorU32(ImGuiCol.TextDisabled, 0.25f);

            ImDrawList* drawList = ImGui.GetWindowDrawList();

            // bg grid
            {
                int horizontalGridSpacing = 25;
                int verticalGridSpacing = 25;

                int horizontalLinesCount = (int)(size.X / horizontalGridSpacing);
                int verticalLinesCount = (int)(size.Y / verticalGridSpacing);

                float horizontalStep = ht / (horizontalLinesCount + 1);
                float verticalStep = wd / (verticalLinesCount + 1);

                for (int j = 1; j <= horizontalLinesCount; j++)
                {
                    float yPos = bb.Min.Y + horizontalStep * j;
                    drawList->AddLine(new Vector2(bb.Min.X, yPos), new Vector2(bb.Max.X, yPos), gridColor1, (j == horizontalLinesCount / 2 + 1) ? 3 : 1);
                }

                for (int ii = 1; ii <= verticalLinesCount; ii++)
                {
                    float xPos = bb.Min.X + verticalStep * ii;
                    drawList->AddLine(new Vector2(xPos, bb.Min.Y), new Vector2(xPos, bb.Max.Y), gridColor2);
                }
            }

            drawList->PushClipRect(bb.Min, bb.Max);

            uint lineColor = ImGui.GetColorU32(ImGuiCol.PlotHistogram);

            for (i = 0; i <= Smoothness - 1; ++i)
            {
                float px = (i + 0) / (float)Smoothness;
                float qx = (i + 1) / (float)Smoothness;

                px = Remap(px, 0, 1, rangeMin.X, rangeMax.X);
                qx = Remap(qx, 0, 1, rangeMin.X, rangeMax.X);

                float py = CurveValueSmooth(px, pointCount, points);
                float qy = CurveValueSmooth(qx, pointCount, points);

                Vector2 p = Remap(new Vector2(px, py), rangeMin, rangeMax, new Vector2(0, 0), new Vector2(1, 1));
                Vector2 q = Remap(new Vector2(qx, qy), rangeMin, rangeMax, new Vector2(0, 0), new Vector2(1, 1));
                p.Y = 1.0f - p.Y;
                q.Y = 1.0f - q.Y;

                p = Remap(p, new Vector2(0, 0), new Vector2(1, 1), bb.Min, bb.Max);
                q = Remap(q, new Vector2(0, 0), new Vector2(1, 1), bb.Min, bb.Max);

                drawList->AddLine(p, q, lineColor, hovered ? 3f : 2f);
            }

            // looks visually bad, idk.
            /*
            // lines
            for (i = 1; i < pointCount; i++)
            {
                Vector2 a = ImRemap(points[i - 1], rangeMin, rangeMax, new Vector2(0, 0), new Vector2(1, 1));
                Vector2 b = ImRemap(points[i], rangeMin, rangeMax, new Vector2(0, 0), new Vector2(1, 1));

                a.Y = 1.0f - a.Y;
                b.Y = 1.0f - b.Y;

                a = ImRemap(a, new Vector2(0, 0), new Vector2(1, 1), bb.Min, bb.Max);
                b = ImRemap(b, new Vector2(0, 0), new Vector2(1, 1), bb.Min, bb.Max);

                drawList->AddLine(a, b, ImGui.GetColorU32(ImGuiCol.PlotLines, 0.5f));
            }
            */

            if (hovered || draggingPoint)
            {
                // control points
                for (i = 0; i < pointCount; i++)
                {
                    Vector2 p = Remap(points[i], rangeMin, rangeMax, new Vector2(0, 0), new Vector2(1, 1));
                    p.Y = 1.0f - p.Y;
                    p = Remap(p, new Vector2(0, 0), new Vector2(1, 1), bb.Min, bb.Max);

                    Vector2 a = p - new Vector2(4, 4);
                    Vector2 b = p + new Vector2(4, 4);
                    if (hoveredPoint == i)
                        drawList->AddRect(a, b, ImGui.GetColorU32(ImGuiCol.PlotLinesHovered));
                    else
                        drawList->AddCircle(p, pointRadiusInPixels, ImGui.GetColorU32(ImGuiCol.PlotLinesHovered));
                }
            }

            drawList->PopClipRect();

            // draw the text at mouse position

            if (hovered || draggingPoint)
            {
                byte* buf = stackalloc byte[128];
                Vector2 pos = (io.MousePos - bb.Min) / (bb.Max - bb.Min);
                pos.Y = 1.0f - pos.Y;

                pos.X = MathUtil.Lerp(rangeMin.X, rangeMax.X, pos.X);
                pos.Y = MathUtil.Lerp(rangeMin.Y, rangeMax.Y, pos.Y);

                nint* args = stackalloc nint[2];
                args[0] = (nint)(&pos.X);
                args[1] = (nint)(&pos.Y);

                byte* textEnd = buf + ImGui.ImFormatStringV(buf, 128, " (%.2f,%.2f)", (nuint)args);
                ImGui.RenderTextClipped(new Vector2(bb.Min.X, bb.Min.Y + style.FramePadding.Y), bb.Max, buf, textEnd, (Vector2*)null, new Vector2(1f, 1f), &bb);
            }

            // curve selector

            // buttons; @todo: mirror, smooth, tessellate
            if (ImGui.BeginPopupContextItem(label))
            {
                if (ImGui.Selectable("Reset"))
                {
                    points[0] = rangeMin;
                    points[1] = rangeMax;
                    pointCount = 2;
                }
                if (ImGui.Selectable("Flip"))
                {
                    for (i = 0; i < pointCount; ++i)
                    {
                        float yVal = 1.0f - Remap(points[i].Y, rangeMin.Y, rangeMax.Y, 0, 1);
                        points[i].Y = Remap(yVal, 0, 1, rangeMin.Y, rangeMax.Y);
                    }
                }
                if (ImGui.Selectable("Mirror"))
                {
                    for (int ii = 0, j = pointCount - 1; ii < j; ii++, j--)
                    {
                        SwapT(&points[ii], &points[j]);
                    }
                    for (i = 0; i < pointCount; ++i)
                    {
                        float xVal = 1.0f - Remap(points[i].X, rangeMin.X, rangeMax.X, 0, 1);
                        points[i].X = Remap(xVal, 0, 1, rangeMin.X, rangeMax.X);
                    }
                }
                ImGui.Separator();
                if (ImGui.BeginMenu("Presets"))
                {
                    ImGui.PushID("curve_items");
                    for (int row = 0; row < items.Length; ++row)
                    {
                        if (ImGui.MenuItem(items[row]))
                        {
                            for (i = 0; i < pointCount; ++i)
                            {
                                float px = i / (float)(pointCount - 1);
                                float py = (float)(ease(row - 1, px));

                                points[i] = Remap(new Vector2(px, py), new Vector2(0, 0), new Vector2(1, 1), rangeMin, rangeMax);
                            }
                        }
                    }
                    ImGui.PopID();
                    ImGui.EndMenu();
                }

                ImGui.EndPopup();
            }

            ImGui.PopID();

            if (selection != null)
            {
                *selection = currentSelection;
            }

            return modified;
        }
    }
}