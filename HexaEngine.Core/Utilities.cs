namespace HexaEngine.Core
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Unsafes;
    using System;
    using System.Runtime.InteropServices;

    public static unsafe class Utilities
    {
        public static void** ToPointerArray<T1>(T1[] values) where T1 : IDeviceChild
        {
            void*[] ptrs = new void*[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                ptrs[i] = (void*)(values[i]?.NativePointer ?? IntPtr.Zero);
            }
            fixed (void** ptr = ptrs)
            {
                return ptr;
            }
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

        public static T* AsPointer<T>(T[] values) where T : unmanaged
        {
            fixed (T* ptr = values)
            {
                return ptr;
            }
        }

        public static char* UTF16(string str)
        {
            return (char*)Marshal.StringToHGlobalUni(str);
        }

        public static T** AsPointer<T>(T*[] value) where T : unmanaged
        {
            fixed (T** ptr = value)
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

        public static T** AsPointer<T>(Pointer<T>[] pointers) where T : unmanaged
        {
            T*[] ts = new T*[pointers.Length];
            for (int i = 0; i < pointers.Length; i++)
            {
                ts[i] = pointers[i];
            }
            return AsPointer(ts);
        }

        public static T** AsPointer<T>(T[][] pointers) where T : unmanaged
        {
            T*[] ts = new T*[pointers.Length];
            for (int i = 0; i < pointers.Length; i++)
            {
                ts[i] = AsPointer(pointers[i]);
            }
            return AsPointer(ts);
        }
    }
}