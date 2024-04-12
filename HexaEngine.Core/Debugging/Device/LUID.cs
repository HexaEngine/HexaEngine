namespace HexaEngine.Core.Debugging.Device
{
    using System;

    /// <summary>
    /// Represents a locally unique identifier (LUID) with a 64-bit value.
    /// </summary>
    public struct LUID : IEquatable<LUID>
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

        public override readonly bool Equals(object? obj)
        {
            return obj is LUID lUID && Equals(lUID);
        }

        public readonly bool Equals(LUID other)
        {
            return Low == other.Low &&
                   High == other.High;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Low, High);
        }

        public static bool operator ==(LUID left, LUID right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LUID left, LUID right)
        {
            return !(left == right);
        }
    }
}