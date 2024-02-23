namespace HexaEngine.Mathematics
{
    /// <summary>
    /// Represents the mode of an interval.
    /// </summary>
    public enum IntervalMode : byte
    {
        /// <summary>
        /// Indicates an inclusive interval mode, where both ends of the interval are included.
        /// </summary>
        Inclusive,

        /// <summary>
        /// Indicates an exclusive interval mode, where neither end of the interval is included.
        /// </summary>
        Exclusive
    }
}