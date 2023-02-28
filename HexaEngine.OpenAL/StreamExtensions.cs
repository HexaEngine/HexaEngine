namespace HexaEngine.OpenAL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

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