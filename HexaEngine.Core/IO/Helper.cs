namespace HexaEngine.Core.IO
{
    using K4os.Compression.LZ4;
    using K4os.Compression.LZ4.Streams;
    using System.IO.Compression;

    public static class CompressionHelper
    {
        public static Stream CreateDecompressionStream(this Stream stream, Compression compression, out bool isCompressed)
        {
            if (compression == Compression.Deflate)
            {
                isCompressed = true;
                return new DeflateStream(stream, CompressionMode.Decompress, true);
            }

            if (compression == Compression.LZ4)
            {
                isCompressed = true;
                return LZ4Stream.Decode(stream, 0, true);
            }

            isCompressed = false;
            return stream;
        }

        public static Stream CreateCompressionStream(this Stream stream, Compression compression, out bool isCompressed)
        {
            if (compression == Compression.Deflate)
            {
                isCompressed = true;
                return new DeflateStream(stream, CompressionLevel.SmallestSize, true);
            }

            if (compression == Compression.LZ4)
            {
                isCompressed = true;
                return LZ4Stream.Encode(stream, LZ4Level.L10_OPT, 0, true);
            }

            isCompressed = false;
            return stream;
        }
    }
}