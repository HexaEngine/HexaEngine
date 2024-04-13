namespace HexaEngine.Core.Text
{
    /// <summary>
    /// Provides helper methods for formatting data sizes.
    /// </summary>
    public static class DataSizeFormatHelper
    {
        /// <summary>
        /// Formats a nuint data size into a human-readable format.
        /// </summary>
        /// <param name="s">The data size in bytes.</param>
        /// <returns>A human-readable format of the data size.</returns>
        public static string Format(nuint s)
        {
            if (s > 1152921504606846976)
            {
                return $"{s / 1152921504606846976f}EiB";
            }

            if (s > 1125899906842624)
            {
                return $"{s / 1125899906842624f}PiB";
            }

            if (s > 1099511627776)
            {
                return $"{s / 1099511627776f}TiB";
            }

            if (s > 1073741824)
            {
                return $"{s / 1073741824f}GiB";
            }

            if (s > 1048576)
            {
                return $"{s / 1048576f}MiB";
            }

            if (s > 1024)
            {
                return $"{s / 1024f}KiB";
            }

            return $"{s}B";
        }

        /// <summary>
        /// Formats a nint data size into a human-readable format.
        /// </summary>
        /// <param name="s">The data size in bytes.</param>
        /// <returns>A human-readable format of the data size.</returns>
        public static string Format(nint s)
        {
            return Format((long)s);
        }

        /// <summary>
        /// Formats a ulong data size into a human-readable format.
        /// </summary>
        /// <param name="s">The data size in bytes.</param>
        /// <returns>A human-readable format of the data size.</returns>
        public static string Format(ulong s)
        {
            if (s > 1152921504606846976)
            {
                return $"{s / 1152921504606846976f}EiB";
            }

            if (s > 1125899906842624)
            {
                return $"{s / 1125899906842624f}PiB";
            }

            if (s > 1099511627776)
            {
                return $"{s / 1099511627776f}TiB";
            }

            if (s > 1073741824)
            {
                return $"{s / 1073741824f}GiB";
            }

            if (s > 1048576)
            {
                return $"{s / 1048576f}MiB";
            }

            if (s > 1024)
            {
                return $"{s / 1024f}KiB";
            }

            return $"{s}B";
        }

        /// <summary>
        /// Formats a long data size into a human-readable format.
        /// </summary>
        /// <param name="s">The data size in bytes.</param>
        /// <returns>A human-readable format of the data size.</returns>
        public static string Format(long s)
        {
            var ab = Math.Abs(s);
            if (ab > 1152921504606846976)
            {
                return $"{s / 1152921504606846976f}EiB";
            }

            if (ab > 1125899906842624)
            {
                return $"{s / 1125899906842624f}PiB";
            }

            if (ab > 1099511627776)
            {
                return $"{s / 1099511627776f}TiB";
            }

            if (ab > 1073741824)
            {
                return $"{s / 1073741824f}GiB";
            }

            if (ab > 1048576)
            {
                return $"{s / 1048576f}MiB";
            }

            if (ab > 1024)
            {
                return $"{s / 1024f}KiB";
            }

            return $"{s}B";
        }

        /// <summary>
        /// Formats a uint data size into a human-readable format.
        /// </summary>
        /// <param name="s">The data size in bytes.</param>
        /// <returns>A human-readable format of the data size.</returns>
        public static string Format(uint s)
        {
            if (s > 1073741824)
            {
                return $"{s / 1073741824f}GiB";
            }

            if (s > 1048576)
            {
                return $"{s / 1048576f}MiB";
            }

            if (s > 1024)
            {
                return $"{s / 1024f}KiB";
            }

            return $"{s}B";
        }

        /// <summary>
        /// Formats an int data size into a human-readable format.
        /// </summary>
        /// <param name="s">The data size in bytes.</param>
        /// <returns>A human-readable format of the data size.</returns>
        public static string Format(int s)
        {
            if (s > 1073741824)
            {
                return $"{s / 1073741824f}GiB";
            }

            if (s > 1048576)
            {
                return $"{s / 1048576f}MiB";
            }

            if (s > 1024)
            {
                return $"{s / 1024f}KiB";
            }

            return $"{s}B";
        }

        /// <summary>
        /// Formats a ushort data size into a human-readable format.
        /// </summary>
        /// <param name="s">The data size in bytes.</param>
        /// <returns>A human-readable format of the data size.</returns>
        public static string Format(ushort s)
        {
            if (s > 1024)
            {
                return $"{s / 1024f}KiB";
            }

            return $"{s}B";
        }

        /// <summary>
        /// Formats a short data size into a human-readable format.
        /// </summary>
        /// <param name="s">The data size in bytes.</param>
        /// <returns>A human-readable format of the data size.</returns>
        public static string Format(short s)
        {
            if (s > 1024)
            {
                return $"{s / 1024f}KiB";
            }

            return $"{s}B";
        }

        /// <summary>
        /// Formats a byte data size into a human-readable format.
        /// </summary>
        /// <param name="s">The data size in bytes.</param>
        /// <returns>A human-readable format of the data size.</returns>
        public static string Format(byte s)
        {
            return $"{s}B";
        }

        /// <summary>
        /// Formats a sbyte data size into a human-readable format.
        /// </summary>
        /// <param name="s">The data size in bytes.</param>
        /// <returns>A human-readable format of the data size.</returns>
        public static string Format(sbyte s)
        {
            return $"{s}B";
        }
    }
}