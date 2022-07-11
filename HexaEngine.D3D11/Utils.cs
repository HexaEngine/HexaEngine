namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Core.Native;
    using System;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    public static unsafe class Utils
    {
        public static Guid* Guid(Guid guid)
        {
            return (Guid*)Unsafe.AsPointer(ref guid);
        }

        public static string GetString(ID3D10Blob* error)
        {
            return error != null ? Marshal.PtrToStringUTF8(new IntPtr(error->GetBufferPointer())) : string.Empty;
        }

        public static T2** ToPointerArray<T1, T2>(T1[] values) where T1 : IDeviceChild where T2 : unmanaged
        {
            T2*[] ptrs = new T2*[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                ptrs[i] = (T2*)values[i].NativePointer;
            }
            return AsPointer(ptrs);
        }

        public static T** AsPointer<T>(T* value) where T : unmanaged
        {
            fixed (T** ptr = new T*[] { value })
            {
                return ptr;
            }
        }

        public static T** AsPointer<T>(T*[] value) where T : unmanaged
        {
            fixed (T** ptr = value)
            {
                return ptr;
            }
        }

        public static T* AsPointer<T>(T[] value) where T : unmanaged
        {
            fixed (T* ptr = value)
            {
                return ptr;
            }
        }

        public static T* AsPointer<T>(T value) where T : unmanaged
        {
            fixed (T* ptr = new T[] { value })
            {
                return ptr;
            }
        }

        public static T2* Cast<T1, T2>(T1* t) where T1 : unmanaged where T2 : unmanaged
        {
            return (T2*)t;
        }

        public static byte* ToBytes(this string str)
        {
            fixed (byte* ptr = Encoding.UTF8.GetBytes(str))
            {
                return ptr;
            }
        }

        public static float* ToFloatPtr(this Vector4 vector)
        {
            fixed (float* ptr = new float[] { vector.X, vector.Y, vector.Z, vector.W })
            {
                return ptr;
            }
        }

        public static void ThrowHResult(this int code)
        {
            ResultCode resultCode = (ResultCode)code;
            if (resultCode != ResultCode.S_OK)
                throw new D3D11Exception(resultCode);
        }
    }
}