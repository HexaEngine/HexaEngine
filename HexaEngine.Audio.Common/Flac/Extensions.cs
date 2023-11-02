namespace HexaEngine.Audio.Common.Flac
{
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Mathematics;
    using System.Diagnostics;

    public static class Extensions
    {
        public static void LogBin(this sbyte value)
        {
            const int bitLength = 8;
            var s = value.ToString("B" + (bitLength / 4).ToString()).PadLeft(bitLength, '0');
            Trace.WriteLine(s);
        }

        public static void LogBin(this byte value)
        {
            const int bitLength = 8;
            var s = value.ToString("B" + (bitLength / 4).ToString()).PadLeft(bitLength, '0');
            Trace.WriteLine(s);
        }

        public static void LogBin(this short value)
        {
            const int bitLength = 16;
            var s = value.ToString("B" + (bitLength / 4).ToString()).PadLeft(bitLength, '0');
            Trace.WriteLine(s);
        }

        public static void LogBin(this ushort value)
        {
            const int bitLength = 16;
            var s = value.ToString("B" + (bitLength / 4).ToString()).PadLeft(bitLength, '0');
            Trace.WriteLine(s);
        }

        public static void LogBin(this int value)
        {
            const int bitLength = 32;
            var s = value.ToString("B" + (bitLength / 4).ToString()).PadLeft(bitLength, '0');
            Trace.WriteLine(s);
        }

        public static void LogBin(this uint value)
        {
            const int bitLength = 32;
            var s = value.ToString("B" + (bitLength / 4).ToString()).PadLeft(bitLength, '0');
            Trace.WriteLine(s);
        }

        public static void LogBin(this long value)
        {
            const int bitLength = 64;
            var s = value.ToString("B" + (bitLength / 4).ToString()).PadLeft(bitLength, '0');
            Trace.WriteLine(s);
        }

        public static void LogBin(this ulong value)
        {
            const int bitLength = 64;
            var s = value.ToString("B" + (bitLength / 4).ToString()).PadLeft(bitLength, '0');
            Trace.WriteLine(s);
        }

        public static void LogBin(this sbyte value, string label)
        {
            const int bitLength = 8;
            var s = value.ToString("B" + (bitLength / 4).ToString()).PadLeft(bitLength, '0');
            Trace.WriteLine($"{label}:\t\t{s}");
        }

        public static void LogBin(this byte value, string label)
        {
            const int bitLength = 8;
            var s = value.ToString("B" + (bitLength / 4).ToString()).PadLeft(bitLength, '0');
            Trace.WriteLine($"{label}:\t\t{s}");
        }

        public static void LogBin(this short value, string label)
        {
            const int bitLength = 16;
            var s = value.ToString("B" + (bitLength / 4).ToString()).PadLeft(bitLength, '0');
            Trace.WriteLine($"{label}:\t\t{s}");
        }

        public static void LogBin(this ushort value, string label)
        {
            const int bitLength = 16;
            var s = value.ToString("B" + (bitLength / 4).ToString()).PadLeft(bitLength, '0');
            Trace.WriteLine($"{label}:\t\t{s}");
        }

        public static void LogBin(this int value, string label)
        {
            const int bitLength = 32;
            var s = value.ToString("B" + (bitLength / 4).ToString()).PadLeft(bitLength, '0');
            Trace.WriteLine($"{label}:\t\t{s}");
        }

        public static void LogBin(this uint value, string label)
        {
            const int bitLength = 32;
            var s = value.ToString("B" + (bitLength / 4).ToString()).PadLeft(bitLength, '0');
            Trace.WriteLine($"{label}:\t\t{s}");
        }

        public static void LogBin(this long value, string label)
        {
            const int bitLength = 64;
            var s = value.ToString("B" + (bitLength / 4).ToString()).PadLeft(bitLength, '0');
            Trace.WriteLine($"{label}:\t\t{s}");
        }

        public static void LogBin(this ulong value, string label)
        {
            const int bitLength = 64;
            var s = value.ToString("B" + (bitLength / 4).ToString()).PadLeft(bitLength, '0');
            Trace.WriteLine($"{label}:\t\t{s}");
        }

        public static StdString ReadStdString(this Stream stream, Endianness endianness)
        {
            uint length = stream.ReadUInt32(endianness);
            StdString str = default;
            str.Resize((int)length);
            stream.Read(str.AsSpan());
            return str;
        }

        public static UInt24 ReadUInt24(this Stream stream, Endianness endianness)
        {
            Span<byte> buffer = stackalloc byte[3];
            stream.Read(buffer);
            if (endianness == Endianness.LittleEndian)
            {
                return UInt24.ReadLittleEndian(buffer);
            }
            else
            {
                return UInt24.ReadBigEndian(buffer);
            }
        }

        public static void WriteUInt24(this Stream stream, UInt24 value, Endianness endianness)
        {
            Span<byte> buffer = stackalloc byte[3];
            if (endianness == Endianness.LittleEndian)
            {
                UInt24.WriteLittleEndian(buffer, value);
            }
            else
            {
                UInt24.WriteBigEndian(buffer, value);
            }
            stream.Write(buffer);
        }
    }
}