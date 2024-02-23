namespace HexaEngine.Mathematics
{
    /// <summary>
    /// Represents an interval between two floating-point values.
    /// </summary>
    public struct Interval
    {
        /// <summary>
        /// The starting point of the interval.
        /// </summary>
        public float Start;

        /// <summary>
        /// The ending point of the interval.
        /// </summary>
        public float End;

        /// <summary>
        /// The mode of the starting point of the interval.
        /// </summary>
        public IntervalMode StartMode;

        /// <summary>
        /// The mode of the ending point of the interval.
        /// </summary>
        public IntervalMode EndMode;

        /// <summary>
        /// Initializes a new instance of the <see cref="Interval"/> struct with the specified parameters.
        /// </summary>
        /// <param name="start">The starting point of the interval.</param>
        /// <param name="end">The ending point of the interval.</param>
        /// <param name="startMode">The mode of the starting point of the interval.</param>
        /// <param name="endMode">The mode of the ending point of the interval.</param>
        public Interval(float start, float end, IntervalMode startMode, IntervalMode endMode)
        {
            if (start > end)
            {
                (start, end) = (end, start);
            }
            Start = start;
            End = end;
            StartMode = startMode;
            EndMode = endMode;
        }

        /// <summary>
        /// Gets the length of the interval.
        /// </summary>
        public readonly float Length => End - Start;

        /// <summary>
        /// Gets the midpoint of the interval.
        /// </summary>
        public readonly float Midpoint => (Start + End) / 2f;

        /// <summary>
        /// Splits the interval into two intervals.
        /// </summary>
        /// <param name="a">The first interval resulting from the split.</param>
        /// <param name="b">The second interval resulting from the split.</param>
        public readonly void Split(out Interval a, out Interval b)
        {
            float mid = (Start + End) / 2f;

            a = new Interval(Start, mid, StartMode, IntervalMode.Exclusive);
            b = new Interval(mid, End, IntervalMode.Exclusive, EndMode);
        }

        /// <summary>
        /// Merges two intervals into one interval.
        /// </summary>
        /// <param name="a">The first interval to merge.</param>
        /// <param name="b">The second interval to merge.</param>
        /// <returns>The merged interval.</returns>
        public static Interval Merge(Interval a, Interval b)
        {
            Interval interval;

            interval.Start = MathF.Min(a.Start, b.Start);
            interval.End = MathF.Max(a.End, b.End);
            interval.StartMode = a.StartMode;
            interval.EndMode = a.EndMode;

            return interval;
        }
    }
}