namespace HexaEngine.Core
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Unsafes;
    using System;
    using System.Buffers.Binary;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

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
            fixed (char* ptr = str)
            {
                return ptr;
            }
        }

        public static byte* UTF8(string str)
        {
            fixed (byte* ptr = Encoding.UTF8.GetBytes(str))
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

        public static T* Alloc<T>() where T : unmanaged
        {
            fixed (void* ptr = new byte[sizeof(T)])
                return (T*)ptr;
        }

        public static void* Alloc(int size)
        {
            fixed (void* ptr = new byte[size])
                return ptr;
        }

        public static void* Alloc(uint size)
        {
            fixed (void* ptr = new byte[size])
                return ptr;
        }

        public static int StringSizeNullTerminated(char* str)
        {
            int ret = 0;
            while (str[ret] != '\0') ret++;
            return ret * 2;
        }

        public static int StringSizeNullTerminated(byte* str)
        {
            int ret = 0;
            while (str[ret] != (byte)'\0') ret++;
            return ret;
        }

        public static int WriteString(Span<byte> dest, Endianness endianness, char* str)
        {
            if (endianness == Endianness.LittleEndian)
            {
                return WriteStringLittleEndian(dest, str);
            }
            else if (endianness == Endianness.BigEndian)
            {
                return WriteStringBigEndian(dest, str);
            }
            else
            {
                return 0;
            }
        }

        public static int WriteStringLittleEndian(Span<byte> dest, char* str)
        {
            int size = StringSizeNullTerminated(str);

            if (BitConverter.IsLittleEndian)
            {
                fixed (void* pDest = dest)
                {
                    Unsafe.CopyBlockUnaligned(str, pDest, (uint)size);
                }
            }
            else
            {
                var src = new ReadOnlySpan<char>(str, size);
                for (int i = 0; i < size; i++)
                {
                    BinaryPrimitives.WriteInt16LittleEndian(dest[(2 * i)..], (short)src[i]);
                }
            }
            return size;
        }

        public static int WriteStringBigEndian(Span<byte> dest, char* str)
        {
            int size = StringSizeNullTerminated(str);

            if (!BitConverter.IsLittleEndian)
            {
                fixed (void* pDest = dest)
                {
                    Unsafe.CopyBlockUnaligned(str, pDest, (uint)size);
                }
            }
            else
            {
                var src = new ReadOnlySpan<char>(str, size);
                for (int i = 0; i < size; i++)
                {
                    BinaryPrimitives.WriteInt16BigEndian(dest[(2 * i)..], (short)src[i]);
                }
            }
            return size;
        }

        public static int ReadStringLittleEndian(Span<byte> dest, char* str)
        {
            int size = StringSizeNullTerminated(str);

            if (BitConverter.IsLittleEndian)
            {
                fixed (void* pDest = dest)
                {
                    Unsafe.CopyBlockUnaligned(str, pDest, (uint)size);
                }
            }
            else
            {
                var src = new ReadOnlySpan<char>(str, size);
                for (int i = 0; i < size; i++)
                {
                    BinaryPrimitives.WriteInt16LittleEndian(dest[(2 * i)..], (short)src[i]);
                }
            }
            return size;
        }

        public static int ReadStringBigEndian(Span<byte> dest, char* str)
        {
            int size = StringSizeNullTerminated(str);

            if (!BitConverter.IsLittleEndian)
            {
                fixed (void* pDest = dest)
                {
                    Unsafe.CopyBlockUnaligned(str, pDest, (uint)size);
                }
            }
            else
            {
                var src = new ReadOnlySpan<char>(str, size);
                for (int i = 0; i < size; i++)
                {
                    BinaryPrimitives.WriteInt16BigEndian(dest[(2 * i)..], (short)src[i]);
                }
            }
            return size;
        }
    }
}