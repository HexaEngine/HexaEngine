namespace HexaEngine.Mathematics
{
    /// <summary>
    /// Specifies the byte order or endianness of data storage or communication.
    /// </summary>
    public enum Endianness : byte
    {
        /// <summary>
        /// Represents little-endian byte order where the least significant byte is stored first.
        /// </summary>
        LittleEndian = byte.MinValue,

        /// <summary>
        /// Represents big-endian byte order where the most significant byte is stored first.
        /// </summary>
        BigEndian = byte.MaxValue,
    }
}