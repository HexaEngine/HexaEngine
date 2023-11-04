namespace HexaEngine.Core.Debugging.Device
{
    /// <summary>
    /// Represents a locally unique identifier (LUID) with a 64-bit value.
    /// </summary>
    public struct LUID
    {
        /// <summary>
        /// Gets or sets the low-order part of the LUID (32 bits).
        /// </summary>
        public uint Low;

        /// <summary>
        /// Gets or sets the high-order part of the LUID (32 bits).
        /// </summary>
        public int High;

        /// <summary>
        /// Initializes a new instance of the LUID struct with the specified low and high parts.
        /// </summary>
        /// <param name="low">The low-order part of the LUID (32 bits).</param>
        /// <param name="high">The high-order part of the LUID (32 bits).</param>
        public LUID(uint low, int high)
        {
            Low = low;
            High = high;
        }
    }
}