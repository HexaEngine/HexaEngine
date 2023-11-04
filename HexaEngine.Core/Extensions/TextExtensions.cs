namespace HexaEngine.Core.Extensions
{
    using HexaEngine.Core.Text;

    /// <summary>
    /// Provides extension methods for formatting data sizes into human-readable formats.
    /// </summary>
    public static class TextExtensions
    {
        /// <summary>
        /// Formats a <see cref="nuint"/> data size into a human-readable format.
        /// </summary>
        /// <param name="value">The data size in bytes.</param>
        /// <returns>A human-readable format of the data size.</returns>
        public static string FormatDataSize(this nuint value)
        {
            return DataSizeFormatHelper.Format(value);
        }

        /// <summary>
        /// Formats a <see cref="nint"/> data size into a human-readable format.
        /// </summary>
        /// <param name="value">The data size in bytes.</param>
        /// <returns>A human-readable format of the data size.</returns>
        public static string FormatDataSize(this nint value)
        {
            return DataSizeFormatHelper.Format(value);
        }

        /// <summary>
        /// Formats a <see cref="ulong"/> data size into a human-readable format.
        /// </summary>
        /// <param name="value">The data size in bytes.</param>
        /// <returns>A human-readable format of the data size.</returns>
        public static string FormatDataSize(this ulong value)
        {
            return DataSizeFormatHelper.Format(value);
        }

        /// <summary>
        /// Formats a <see cref="long"/> data size into a human-readable format.
        /// </summary>
        /// <param name="value">The data size in bytes.</param>
        /// <returns>A human-readable format of the data size.</returns>
        public static string FormatDataSize(this long value)
        {
            return DataSizeFormatHelper.Format(value);
        }

        /// <summary>
        /// Formats a <see cref="uint"/> data size into a human-readable format.
        /// </summary>
        /// <param name="value">The data size in bytes.</param>
        /// <returns>A human-readable format of the data size.</returns>
        public static string FormatDataSize(this uint value)
        {
            return DataSizeFormatHelper.Format(value);
        }

        /// <summary>
        /// Formats a <see cref="int"/> data size into a human-readable format.
        /// </summary>
        /// <param name="value">The data size in bytes.</param>
        /// <returns>A human-readable format of the data size.</returns>
        public static string FormatDataSize(this int value)
        {
            return DataSizeFormatHelper.Format(value);
        }

        /// <summary>
        /// Formats a <see cref="ushort"/> data size into a human-readable format.
        /// </summary>
        /// <param name="value">The data size in bytes.</param>
        /// <returns>A human-readable format of the data size.</returns>
        public static string FormatDataSize(this ushort value)
        {
            return DataSizeFormatHelper.Format(value);
        }

        /// <summary>
        /// Formats a <see cref="short"/> data size into a human-readable format.
        /// </summary>
        /// <param name="value">The data size in bytes.</param>
        /// <returns>A human-readable format of the data size.</returns>
        public static string FormatDataSize(this short value)
        {
            return DataSizeFormatHelper.Format(value);
        }

        /// <summary>
        /// Formats a <see cref="byte"/> data size into a human-readable format.
        /// </summary>
        /// <param name="value">The data size in bytes.</param>
        /// <returns>A human-readable format of the data size.</returns>
        public static string FormatDataSize(this byte value)
        {
            return DataSizeFormatHelper.Format(value);
        }

        /// <summary>
        /// Formats a <see cref="sbyte"/> data size into a human-readable format.
        /// </summary>
        /// <param name="value">The data size in bytes.</param>
        /// <returns>A human-readable format of the data size.</returns>
        public static string FormatDataSize(this sbyte value)
        {
            return DataSizeFormatHelper.Format(value);
        }
    }
}