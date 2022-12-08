namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
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

        public static T2** ToPointerArray<T1, T2>(T1[]? values) where T1 : IDeviceChild where T2 : unmanaged
        {
            if (values == null) return null;
            T2*[] ptrs = new T2*[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                ptrs[i] = (T2*)values[i]?.NativePointer;
            }
            return AsPointer(ptrs);
        }

        public static void** ToPointerArray<T1>(T1[] values) where T1 : IDeviceChild
        {
            void*[] ptrs = new void*[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                ptrs[i] = (void*)values[i].NativePointer;
            }
            fixed (void** ptr = ptrs)
            {
                return ptr;
            }
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

        public static void** AsPointerArray<T>(T*[] value) where T : unmanaged
        {
            fixed (T** ptr = value)
            {
                return (void**)ptr;
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
            return (float*)&vector;
        }

        public static void ThrowHResult(this int code)
        {
            ResultCode resultCode = (ResultCode)code;
            if (resultCode != ResultCode.S_OK)
                throw new D3D11Exception(resultCode);
        }

        internal static string ToStr(byte* name)
        {
            var bytes = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(name);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}