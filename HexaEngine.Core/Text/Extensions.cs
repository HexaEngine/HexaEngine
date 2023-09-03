namespace HexaEngine.Core.Text
{
    public static class Extensions
    {
        /// <summary>
        /// Formats a nuint data size into a human-readable format.
        /// </summary>
        /// <param name="s">The data size in bytes.</param>
        /// <returns>A human-readable format of the data size.</returns>
        public static string FormatDataSize(this nuint value)
        {
            return DataSizeFormatHelper.Format(value);
        }

        /// <summary>
        /// Formats a nint data size into a human-readable format.
        /// </summary>
        /// <param name="s">The data size in bytes.</param>
        /// <returns>A human-readable format of the data size.</returns>
        public static string FormatDataSize(this nint value)
        {
            return DataSizeFormatHelper.Format(value);
        }

        /// <summary>
        /// Formats a ulong data size into a human-readable format.
        /// </summary>
        /// <param name="s">The data size in bytes.</param>
        /// <returns>A human-readable format of the data size.</returns>
        public static string FormatDataSize(this ulong value)
        {
            return DataSizeFormatHelper.Format(value);
        }

        /// <summary>
        /// Formats a long data size into a human-readable format.
        /// </summary>
        /// <param name="s">The data size in bytes.</param>
        /// <returns>A human-readable format of the data size.</returns>
        public static string FormatDataSize(this long value)
        {
            return DataSizeFormatHelper.Format(value);
        }

        /// <summary>
        /// Formats a uint data size into a human-readable format.
        /// </summary>
        /// <param name="s">The data size in bytes.</param>
        /// <returns>A human-readable format of the data size.</returns>
        public static string FormatDataSize(this uint value)
        {
            return DataSizeFormatHelper.Format(value);
        }

        /// <summary>
        /// Formats a int data size into a human-readable format.
        /// </summary>
        /// <param name="s">The data size in bytes.</param>
        /// <returns>A human-readable format of the data size.</returns>
        public static string FormatDataSize(this int value)
        {
            return DataSizeFormatHelper.Format(value);
        }

        /// <summary>
        /// Formats a ushort data size into a human-readable format.
        /// </summary>
        /// <param name="s">The data size in bytes.</param>
        /// <returns>A human-readable format of the data size.</returns>
        public static string FormatDataSize(this ushort value)
        {
            return DataSizeFormatHelper.Format(value);
        }

        /// <summary>
        /// Formats a short data size into a human-readable format.
        /// </summary>
        /// <param name="s">The data size in bytes.</param>
        /// <returns>A human-readable format of the data size.</returns>
        public static string FormatDataSize(this short value)
        {
            return DataSizeFormatHelper.Format(value);
        }

        /// <summary>
        /// Formats a byte data size into a human-readable format.
        /// </summary>
        /// <param name="s">The data size in bytes.</param>
        /// <returns>A human-readable format of the data size.</returns>
        public static string FormatDataSize(this byte value)
        {
            return DataSizeFormatHelper.Format(value);
        }

        /// <summary>
        /// Formats a sbyte data size into a human-readable format.
        /// </summary>
        /// <param name="s">The data size in bytes.</param>
        /// <returns>A human-readable format of the data size.</returns>
        public static string FormatDataSize(this sbyte value)
        {
            return DataSizeFormatHelper.Format(value);
        }
    }
}