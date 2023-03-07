namespace HexaEngine.OpenAL
{
    internal static class StreamExtensions
    {
        public static byte[] Read(this Stream stream, long length)
        {
            var buffer = new byte[length];
            stream.Read(buffer, 0, (int)length);
            return buffer;
        }
    }
}