namespace FontEditor.IO
{
    using System;
    using System.Buffers;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class Extensions
    {
        public static string ReadString(this Stream stream)
        {
            var length = stream.ReadInt();
            var buffer = new byte[length];
            _ = stream.Read(buffer);
            return Encoding.UTF8.GetString(buffer);
        }

        public static int ReadInt(this Stream stream)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(4);
            _ = stream.Read(buffer, 0, 4);
            var val = BitConverter.ToInt32(buffer);
            ArrayPool<byte>.Shared.Return(buffer);
            return val;
        }

        public static long ReadInt64(this Stream stream)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(8);
            _ = stream.Read(buffer, 0, 8);
            var val = BitConverter.ToInt64(buffer);
            ArrayPool<byte>.Shared.Return(buffer);
            return val;
        }

        public static byte[] Read(this Stream stream, long length)
        {
            var buffer = new byte[length];
            _ = stream.Read(buffer, 0, (int)length);
            return buffer;
        }

        /// <summary>
        /// Wandelt ein unmanaged struct um und speichert die Bytes in die Ziel-Array
        /// </summary>
        /// <typeparam name="T">unmanaged struct</typeparam>
        /// <param name="str">T</param>
        /// <param name="dest">Ziel-Array</param>
        /// <returns>Ziel-Array</returns>
        public static byte[] GetBytes<T>(this T str, byte[] dest) where T : unmanaged
        {
            int size = Marshal.SizeOf(str);

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, dest, 0, size);
            Marshal.FreeHGlobal(ptr);
            return dest;
        }

        /// <summary>
        /// Gibt die Größe in bytes eines unmanaged struct zurück.
        /// </summary>
        /// <typeparam name="T">unmanaged struct</typeparam>
        /// <param name="str">T</param>
        /// <returns>count bytes</returns>
        public static int GetSize<T>(this T str) where T : unmanaged
        {
            return Marshal.SizeOf(str);
        }

        public static T FromBytes<T>(this byte[] arr) where T : unmanaged
        {
            T str = new();

            int size = Marshal.SizeOf(str);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, size);

            str = (T)Marshal.PtrToStructure(ptr, str.GetType());
            Marshal.FreeHGlobal(ptr);

            return str;
        }

        public static T FromStream<T>(this Stream stream) where T : unmanaged
        {
            T str = new();

            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];
            _ = stream.Read(arr);

            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, size);

            str = (T)Marshal.PtrToStructure(ptr, str.GetType());
            Marshal.FreeHGlobal(ptr);

            return str;
        }
    }
}