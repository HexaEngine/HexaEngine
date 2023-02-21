namespace TestApp
{
    using HexaEngine.Core.Unsafes;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    public class Program
    {
        public static unsafe void Main()
        {
            U8String str = new(1024);
            for (int i = 0; i < 1000000000; i++)
            {
                for (int j = 0; j < 1024; j++)
                {
                    j.ToString();
                    str[j] = (byte)Random.Shared.Next(byte.MinValue, byte.MaxValue);
                }
            }
        }

        /*
        public static string Int32ToDecStr(int value)
        {
            return value >= 0 ?
                UInt32ToDecStr((uint)value) :
                NegativeInt32ToDecStr(value, -1, NumberFormatInfo.CurrentInfo.NegativeSign);
        }

        internal static unsafe string UInt32ToDecStr(uint value)
        {
            int bufferLength = CountDigits(value);

            // For single-digit values that are very common, especially 0 and 1, just return cached strings.
            if (bufferLength == 1)
            {
                return s_singleDigitStringCache[value];
            }

            U8String result = new(bufferLength);
            byte* buffer = result;
            {
                byte* p = buffer + bufferLength;
                do
                {
                    uint remainder = value % 10;
                    value /= 10;
                    *(--p) = (byte)(remainder + '0');
                }
                while (value != 0);
                Debug.Assert(p == buffer);
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountDigits(uint value)
        {
            int digits = 1;
            if (value >= 100000)
            {
                value /= 100000;
                digits += 5;
            }

            if (value < 10)
            {
                // no-op
            }
            else if (value < 100)
            {
                digits++;
            }
            else if (value < 1000)
            {
                digits += 2;
            }
            else if (value < 10000)
            {
                digits += 3;
            }
            else
            {
                Debug.Assert(value < 100000);
                digits += 4;
            }

            return digits;
        }*/
    }
}