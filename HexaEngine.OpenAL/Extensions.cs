namespace HexaEngine.OpenAL
{
    using HexaEngine.Audio.Common.Wave;
    using Hexa.NET.OpenAL;

    internal static class Extensions
    {
        public static byte[] Read(this Stream stream, long length)
        {
            var buffer = new byte[length];
            stream.ReadExactly(buffer, 0, (int)length);
            return buffer;
        }

        public static ALFormat GetBufferFormat(this WaveHeader header)
        {
            ALFormat format;
            if (header.NumChannels == 1 && header.BitsPerSample == 8)
                format = ALFormat.Mono8;
            else if (header.NumChannels == 1 && header.BitsPerSample == 16)
                format = ALFormat.Mono16;
            else if (header.NumChannels == 1 && header.BitsPerSample == 32)
                format = ALFormat.MonoFloat32;
            else if (header.NumChannels == 2 && header.BitsPerSample == 8)
                format = ALFormat.Stereo8;
            else if (header.NumChannels == 2 && header.BitsPerSample == 16)
                format = ALFormat.Stereo16;
            else if (header.NumChannels == 2 && header.BitsPerSample == 32)
                format = ALFormat.StereoFloat32;
            else
            {
                throw new InvalidDataException();
            }

            return format;
        }
    }
}