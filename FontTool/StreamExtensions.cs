namespace FontTool
{
    using System;
    using System.Buffers;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class StreamExtensions
    {
        public static void WriteString(this Stream stream, string str)
        {
            stream.WriteInt(str.Length);
            stream.Write(Encoding.UTF8.GetBytes(str));
        }

        public static string ReadString(this Stream stream)
        {
            var length = stream.ReadInt();
            var buffer = new byte[length];
            stream.Read(buffer);
            return Encoding.UTF8.GetString(buffer);
        }

        public static void WriteInt(this Stream stream, int val)
        {
            stream.Write(BitConverter.GetBytes(val));
        }

        public static int ReadInt(this Stream stream)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(4);
            stream.Read(buffer, 0, 4);
            var val = BitConverter.ToInt32(buffer);
            ArrayPool<byte>.Shared.Return(buffer);
            return val;
        }

        public static void WriteInt64(this Stream stream, long val)
        {
            stream.Write(BitConverter.GetBytes(val));
        }

        public static long ReadInt64(this Stream stream)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(8);
            stream.Read(buffer, 0, 8);
            var val = BitConverter.ToInt64(buffer);
            ArrayPool<byte>.Shared.Return(buffer);
            return val;
        }

        public static byte[] Read(this Stream stream, long length)
        {
            var buffer = new byte[length];
            stream.Read(buffer, 0, (int)length);
            return buffer;
        }

        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
        private static unsafe extern void CopyMemory(void* dest, void* src, int count);

        public static unsafe void Write<T>(this Stream stream, T[] ts) where T : unmanaged
        {
            stream.WriteInt(ts.Length);
            var buffer = new byte[Marshal.SizeOf(typeof(T)) * ts.Length];
            fixed (void* d = &buffer[0])
            {
                fixed (void* s = &ts[0])
                {
                    CopyMemory(d, s, buffer.Length);
                }
            }
            stream.Write(buffer);
        }

        public static unsafe T[] Read<T>(this Stream stream) where T : unmanaged
        {
            var count = stream.ReadInt();
            var ts = new T[count];
            var buffer = new byte[Marshal.SizeOf(typeof(T)) * ts.Length];
            stream.Read(buffer);
            fixed (void* d = &ts[0])
            {
                fixed (void* s = &buffer[0])
                {
                    CopyMemory(d, s, buffer.Length);
                }
            }
            return ts;
        }
    }
}