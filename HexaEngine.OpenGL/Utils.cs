namespace HexaEngine.OpenGL
{
    using Hexa.NET.OpenGL;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    public static unsafe class Utils
    {
        public static void CheckError()
        {
            var error = (GLErrorCode)OpenGLGraphicsDevice.GL.GetError();
            if (error != GLErrorCode.NoError)
            {
                var msg = ToStringFromUTF8(OpenGLGraphicsDevice.GL.GetString((GLStringName)error))!;
                throw new OpenGLException(error, msg);
            }
        }

        public static Guid* Guid(Guid guid)
        {
            return (Guid*)Unsafe.AsPointer(ref guid);
        }

        public static T2* Cast<T1, T2>(T1* t) where T1 : unmanaged where T2 : unmanaged
        {
            return (T2*)t;
        }

        public static byte* ToBytes(this string str)
        {
            return (byte*)Marshal.StringToHGlobalAnsi(str);
        }

        internal static string ToStr(byte* name)
        {
            var bytes = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(name);
            return Encoding.UTF8.GetString(bytes);
        }

        internal static string ToStr(byte* name, int length)
        {
            return Encoding.UTF8.GetString(new Span<byte>(name, length));
        }

        internal static string ToStr(byte* name, uint length)
        {
            return Encoding.UTF8.GetString(new Span<byte>(name, (int)length));
        }
    }
}