namespace HexaEngine.Core
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Unsafes;
    using System;
    using System.Buffers.Binary;
    using System.Drawing;
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

        public static T* GCAlloc<T>() where T : unmanaged
        {
            fixed (void* ptr = new byte[sizeof(T)])
                return (T*)ptr;
        }

        public static T** GCAlloc<T>(int count) where T : unmanaged
        {
            fixed (T** ptr = &(new T*[count])[0])
                return ptr;
        }

        public static T** GCAlloc<T>(uint count) where T : unmanaged
        {
            fixed (T** ptr = &(new T*[count])[0])
                return ptr;
        }

        public static void* GCAlloc(int size)
        {
            fixed (void* ptr = new byte[size])
                return ptr;
        }

        public static void* GCAlloc(uint size)
        {
            fixed (void* ptr = new byte[size])
                return ptr;
        }

        public static void Zero<T>(T* pointer) where T : unmanaged
        {
            Zero(pointer, (uint)sizeof(T));
        }

        public static void Zero(void* pointer, uint size)
        {
            byte* ptr = (byte*)pointer;
            for (int i = 0; i < size; i++)
            {
                ptr[i] = 0;
            }
        }

        public static T* Alloc<T>() where T : unmanaged
        {
            return (T*)Marshal.AllocHGlobal(sizeof(T));
        }

        public static T* Alloc<T>(int count) where T : unmanaged
        {
            return (T*)Marshal.AllocHGlobal(sizeof(T) * count);
        }

        public static T** Alloc<T>(uint count) where T : unmanaged
        {
            return (T**)Marshal.AllocHGlobal((int)(sizeof(T) * count));
        }

        public static void* Alloc(int size)
        {
            return (void*)Marshal.AllocHGlobal(size);
        }

        public static void* Alloc(uint size)
        {
            return (void*)Marshal.AllocHGlobal((int)size);
        }

        public static void** Alloc(int size, int length)
        {
            return (void**)Marshal.AllocHGlobal(size * length);
        }

        public static void** Alloc(uint size, uint length)
        {
            return (void**)Marshal.AllocHGlobal((int)(size * length));
        }

        public static void** AllocArray(uint length)
        {
            return (void**)Marshal.AllocHGlobal((int)(sizeof(nint) * length));
        }

        public static void Free(void* p)
        {
            Marshal.FreeHGlobal((nint)p);
        }

        public static void Free(void** p)
        {
            Marshal.FreeHGlobal((nint)p);
        }

        public static unsafe bool Cmp(byte* a, byte* b, uint length)
        {
            for (uint i = 0; i < length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }

        public static unsafe bool Cmp(char* a, char* b, uint length)
        {
            for (uint i = 0; i < length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }

        public static unsafe bool StrCmp(char* a, char* b)
        {
            int n1 = StringSizeNullTerminated(a);
            int n2 = StringSizeNullTerminated(b);
            ReadOnlySpan<char> chars1 = new(a, n1);
            ReadOnlySpan<char> chars2 = new(b, n2);
            if (n1 != n2)
                return false;
            return chars1.SequenceEqual(chars2);
        }

        public static int StringSizeNullTerminated(char* str)
        {
            int ret = 0;
            while (str[ret] != '\0') ret++;
            return (ret + 1) * 2;
        }

        public static int StringSizeNullTerminated(ReadOnlySpan<char> str)
        {
            int ret = 0;
            while (str[ret] != '\0') ret++;
            return (ret + 1) * 2;
        }

        public static int StringSizeNullTerminated(byte* str)
        {
            int ret = 0;
            while (str[ret] != (byte)'\0') ret++;
            return ret + 1;
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
                ReadOnlySpan<byte> chars = new(str, size);
                chars.CopyTo(dest);
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
                var src = new ReadOnlySpan<char>(str, size / 2);
                for (int i = 0; i < size / 2; i++)
                {
                    BinaryPrimitives.WriteInt16BigEndian(dest[(2 * i)..], (short)src[i]);
                }
            }
            return size;
        }

        public static int ReadString(Span<byte> dest, Endianness endianness, char** str)
        {
            if (endianness == Endianness.LittleEndian)
            {
                return ReadStringLittleEndian(dest, str);
            }
            else if (endianness == Endianness.BigEndian)
            {
                return ReadStringBigEndian(dest, str);
            }
            else
            {
                return 0;
            }
        }

        public static int ReadStringLittleEndian(Span<byte> dest, char** ppStr)
        {
            int size = StringSizeNullTerminated(MemoryMarshal.Cast<byte, char>(dest));
            char* pStr = (char*)GCAlloc(size);
            *ppStr = pStr;

            if (BitConverter.IsLittleEndian)
            {
                fixed (void* pDest = dest)
                {
                    Unsafe.CopyBlockUnaligned(pStr, pDest, (uint)size);
                }
            }
            else
            {
                var src = new ReadOnlySpan<char>(pStr, size);
                for (int i = 0; i < size; i++)
                {
                    BinaryPrimitives.WriteInt16LittleEndian(dest[(2 * i)..], (short)src[i]);
                }
            }
            return size;
        }

        public static int ReadStringBigEndian(Span<byte> dest, char** ppStr)
        {
            int size = StringSizeNullTerminated(MemoryMarshal.Cast<byte, char>(dest));
            char* pStr = (char*)GCAlloc(size);
            *ppStr = pStr;

            if (!BitConverter.IsLittleEndian)
            {
                fixed (void* pDest = dest)
                {
                    Unsafe.CopyBlockUnaligned(pStr, pDest, (uint)size);
                }
            }
            else
            {
                var src = new ReadOnlySpan<char>(pStr, size);
                for (int i = 0; i < size; i++)
                {
                    BinaryPrimitives.WriteInt16BigEndian(dest[(2 * i)..], (short)src[i]);
                }
            }
            return size;
        }
    }
}