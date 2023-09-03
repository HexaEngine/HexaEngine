namespace HexaEngine.Core
{
    using HexaEngine.Core.Unsafes;
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    public static unsafe class Utils
    {
        public static void Assert(bool condition)
        {
#if DEBUG
            Trace.Assert(condition);
#endif
        }

        public static void Assert(bool condition, string message)
        {
#if DEBUG
            Trace.Assert(condition, message);
#endif
        }

        public static void ThrowIf(bool condition, string message)
        {
            if (condition)
            {
                throw new(message);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIf(this Exception? exception)
        {
#if DEBUG
            if (exception != null)
            {
                throw exception;
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SdlThrowIf(this int result)
        {
#if DEBUG
            if (result == 0)
            {
                Application.sdl.GetErrorAsException().ThrowIf();
            }
            return result;
#else
            return result;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SdlThrowIfNeg(this int result)
        {
#if DEBUG
            if (result < 0)
            {
                Application.sdl.GetErrorAsException().ThrowIf();
            }
            return result;
#else
            return result;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint SdlThrowIf(this uint result)
        {
#if DEBUG
            if (result == 0)
            {
                Application.sdl.GetErrorAsException().ThrowIf();
            }
            return result;
#else
            return result;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SdlCheckError()
        {
#if DEBUG
            Application.sdl.GetErrorAsException().ThrowIf();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* SdlCheckError(void* ptr)
        {
#if DEBUG
            if (ptr == null)
            {
                Application.sdl.GetErrorAsException().ThrowIf();
            }
            return ptr;
#else
            return ptr;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* SdlCheckError<T>(T* ptr) where T : unmanaged
        {
#if DEBUG
            if (ptr == null)
            {
                Application.sdl.GetErrorAsException().ThrowIf();
            }
            return ptr;
#else
            return ptr;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string? ToStringFromUTF8(byte* ptr)
        {
            return Marshal.PtrToStringUTF8((nint)ptr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Bitcount(this uint value)
        {
            uint v = value;
            v -= ((v >> 1) & 0x55555555); // reuse input as temporary
            v = (v & 0x33333333) + ((v >> 2) & 0x33333333); // temp
            uint c = ((v + (v >> 4) & 0xF0F0F0F) * 0x1010101) >> 24; // count
            return c;
        }

        public static void Swap<T>(T* a, T* b) where T : unmanaged
        {
            (*b, *a) = (*a, *b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this T t, int* offset, byte* dst) where T : unmanaged
        {
            var size = sizeof(T);
            var p = (byte*)&t;
            var dst_ptr = &dst[*offset];
            for (int i = 0; i < size; i++)
            {
                dst_ptr[i] = p[i];
            }
            *offset += size;
        }

        public static void CopyTo<T>(this T[] t, byte* dst, int stride, int* offset, int count) where T : unmanaged
        {
            var size = sizeof(T);
            fixed (T* src = t)
            {
                var dst_ptr = dst;
                var src_ptr = (byte*)src;
                for (int i = 0; i < count; i++)
                {
                    var srcByteOffset = i * size;
                    var dstByteOffset = i * stride + *offset;

                    for (int j = 0; j < size; j++)
                    {
                        dst_ptr[j + dstByteOffset] = src_ptr[j + srcByteOffset];
                    }
                }
            }
            *offset += size;
        }

        /// <summary>
        /// Converts to utf16 pointer.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>The pointer, must be freed after usage.</returns>
        public static char* ToUTF16(this string str)
        {
            char* dst = AllocT<char>(str.Length + 1);
            fixed (char* src = str)
            {
                MemcpyT(src, dst, str.Length, str.Length);
            }
            dst[str.Length] = '\0';
            return dst;
        }

        /// <summary>
        /// Converts an array to an native pointer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* ToPtr<T>(this T[] values) where T : unmanaged
        {
            uint bytesToCopy = (uint)values.Length * (uint)sizeof(T);
            T* result = (T*)Alloc(bytesToCopy);
            fixed (T* src = values)
            {
                MemcpyT(src, result, bytesToCopy, bytesToCopy);
            }
            return result;
        }

        /// <summary>
        /// Converts to utf8 pointer.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>The pointer, must be freed after usage.</returns>
        public static byte* ToUTF8(this string str)
        {
            var byteCount = Encoding.UTF8.GetByteCount(str);
            byte* dst = AllocT<byte>(Encoding.UTF8.GetByteCount(str) + 1);
            fixed (char* src = str)
            {
                Encoding.UTF8.GetBytes(src, str.Length, dst, byteCount);
            }
            dst[str.Length] = 0;
            return dst;
        }

        /// <summary>
        /// Copies an pointer to another pointer with the specified lengthes.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <param name="dst">The DST.</param>
        /// <param name="dstLength">Length of the DST.</param>
        /// <param name="srcLength">Length of the source.</param>
        public static void Memcpy(void* src, void* dst, uint dstLength, uint srcLength)
        {
            Buffer.MemoryCopy(src, dst, dstLength, srcLength);
        }

        /// <summary>
        /// Copies an pointer to another pointer with the specified lengthes.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <param name="dst">The DST.</param>
        /// <param name="dstLength">Length of the DST.</param>
        /// <param name="srcLength">Length of the source.</param>
        public static void Memcpy(void* src, void* dst, int dstLength, int srcLength)
        {
            Buffer.MemoryCopy(src, dst, dstLength, srcLength);
        }

        /// <summary>
        /// Copies an pointer to another pointer with the specified lengthes.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <param name="dst">The DST.</param>
        /// <param name="dstLength">Length of the DST.</param>
        /// <param name="srcLength">Length of the source.</param>
        public static void Memcpy(void* src, void* dst, long dstLength, long srcLength)
        {
            Buffer.MemoryCopy(src, dst, dstLength, srcLength);
        }

        /// <summary>
        /// Copies an pointer to another pointer with the specified lengthes.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <param name="dst">The DST.</param>
        /// <param name="dstLength">Length of the DST.</param>
        /// <param name="srcLength">Length of the source.</param>
        public static void Memcpy(void* src, void* dst, ulong dstLength, ulong srcLength)
        {
            Buffer.MemoryCopy(src, dst, dstLength, srcLength);
        }

        /// <summary>
        /// Copies an pointer to another pointer with the specified lengths.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <param name="dst">The DST.</param>
        /// <param name="dstLength">Length of the DST.</param>
        /// <param name="srcLength">Length of the source.</param>
        public static void Memcpy(void* src, void* dst, uint length)
        {
            Buffer.MemoryCopy(src, dst, length, length);
        }

        /// <summary>
        /// Copies an pointer to another pointer with the specified lengths.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <param name="dst">The DST.</param>
        /// <param name="dstLength">Length of the DST.</param>
        /// <param name="srcLength">Length of the source.</param>
        public static void Memcpy(void* src, void* dst, int length)
        {
            Buffer.MemoryCopy(src, dst, length, length);
        }

        /// <summary>
        /// Copies an pointer to another pointer with the specified lengths.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <param name="dst">The DST.</param>
        /// <param name="dstLength">Length of the DST.</param>
        /// <param name="srcLength">Length of the source.</param>
        public static void Memcpy(void* src, void* dst, long length)
        {
            Buffer.MemoryCopy(src, dst, length, length);
        }

        /// <summary>
        /// Copies an pointer to another pointer with the specified lengths.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <param name="dst">The DST.</param>
        /// <param name="dstLength">Length of the DST.</param>
        /// <param name="srcLength">Length of the source.</param>
        public static void Memcpy(void* src, void* dst, ulong length)
        {
            Buffer.MemoryCopy(src, dst, length, length);
        }

        /// <summary>
        /// Copies an pointer to another pointer with the specified lengthes.
        /// Automatically calculates byte widths with sizeof(T) * length.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src">The source.</param>
        /// <param name="dst">The DST.</param>
        /// <param name="dstLength">Length of the DST.</param>
        /// <param name="srcLength">Length of the source.</param>
        public static void MemcpyT<T>(T* src, T* dst, uint dstLength, uint srcLength) where T : unmanaged
        {
            Buffer.MemoryCopy(src, dst, dstLength * sizeof(T), srcLength * sizeof(T));
        }

        /// <summary>
        /// Copies an pointer to another pointer with the specified lengthes.
        /// Automatically calculates byte widths with sizeof(T) * length.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src">The source.</param>
        /// <param name="dst">The DST.</param>
        /// <param name="dstLength">Length of the DST.</param>
        /// <param name="srcLength">Length of the source.</param>
        public static void MemcpyT<T>(T* src, T* dst, int dstLength, int srcLength) where T : unmanaged
        {
            Buffer.MemoryCopy(src, dst, dstLength * sizeof(T), srcLength * sizeof(T));
        }

        /// <summary>
        /// Copies an pointer to another pointer with the specified lengths.
        /// Automatically calculates byte widths with sizeof(T) * length.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src">The source.</param>
        /// <param name="dst">The DST.</param>
        /// <param name="dstLength">Length of the DST.</param>
        /// <param name="srcLength">Length of the source.</param>
        public static void MemcpyT<T>(T* src, T* dst, uint length) where T : unmanaged
        {
            Buffer.MemoryCopy(src, dst, length * sizeof(T), length * sizeof(T));
        }

        /// <summary>
        /// Copies an pointer to another pointer with the specified lengthes.
        /// Automatically calculates byte widths with sizeof(T) * length.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src">The source.</param>
        /// <param name="dst">The DST.</param>
        /// <param name="dstLength">Length of the DST.</param>
        /// <param name="srcLength">Length of the source.</param>
        public static void MemcpyT<T>(T* src, T* dst, int length) where T : unmanaged
        {
            Buffer.MemoryCopy(src, dst, length * sizeof(T), length * sizeof(T));
        }

        /// <summary>
        /// Zeroes the specified pointer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pointer">The pointer.</param>
        public static void ZeroMemoryT<T>(T* pointer) where T : unmanaged
        {
            new Span<byte>(pointer, sizeof(T)).Clear();
        }

        /// <summary>
        /// Zeroes the specified pointer range.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pointer">The pointer.</param>
        /// <param name="length">The length.</param>
        public static void ZeroMemoryT<T>(T* pointer, int length) where T : unmanaged
        {
            new Span<byte>(pointer, sizeof(T) * length).Clear();
        }

        /// <summary>
        /// Zeroes the specified pointer range.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pointer">The pointer.</param>
        /// <param name="length">The length.</param>
        public static void ZeroMemoryT<T>(T* pointer, uint length) where T : unmanaged
        {
            ZeroMemory(pointer, sizeof(T) * (int)length);
        }

        /// <summary>
        /// Zeroes the specified pointer.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="size">The size.</param>
        public static void ZeroMemory(void* pointer, uint size)
        {
            byte* pointerCopy = (byte*)pointer;
            for (uint i = 0; i < size; i++)
            {
                pointerCopy[i] = 0;
            }
        }

        /// <summary>
        /// Zeroes the specified pointer.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="size">The size.</param>
        public static void ZeroMemory(void* pointer, int size)
        {
            new Span<byte>(pointer, size).Clear();
        }

        /// <summary>
        /// Zeroes the specified pointer.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="size">The size.</param>
        public static void ZeroMemory(void* pointer, ulong size)
        {
            byte* pointerCopy = (byte*)pointer;
            for (ulong i = 0; i < size; i++)
            {
                pointerCopy[i] = 0;
            }
        }

        /// <summary>
        /// Zeroes the specified pointer.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="size">The size.</param>
        public static void ZeroMemory(void* pointer, long size)
        {
            byte* pointerCopy = (byte*)pointer;
            for (long i = 0; i < size; i++)
            {
                pointerCopy[i] = 0;
            }
        }

        /// <summary>
        /// Zeroes the specified pointer.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="size">The size.</param>
        public static void ZeroMemory(void* pointer, nint size)
        {
            byte* pointerCopy = (byte*)pointer;
            for (nint i = 0; i < size; i++)
            {
                pointerCopy[i] = 0;
            }
        }

        /// <summary>
        /// Allocates the specified struct.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The struct.</param>
        /// <returns></returns>
        public static T* AllocT<T>(T data) where T : unmanaged
        {
            T* result = (T*)Marshal.AllocHGlobal(sizeof(T));
            *result = data;
            return result;
        }

        /// <summary>
        /// Allocates a new instance and calls the default constructor.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T* AllocT<T>() where T : unmanaged
        {
            T* result = (T*)Marshal.AllocHGlobal(sizeof(T));
            *result = new T();
            return result;
        }

        /// <summary>
        /// Allocates the specified count of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public static T* AllocT<T>(int count) where T : unmanaged
        {
            return (T*)Marshal.AllocHGlobal(sizeof(T) * count);
        }

        /// <summary>
        /// Allocates the specified count of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public static void* Alloc(nint count)
        {
            return (void*)Marshal.AllocHGlobal(count);
        }

        /// <summary>
        /// Allocates the specified count of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public static void* Alloc(nuint count)
        {
            return (void*)Marshal.AllocHGlobal((nint)count);
        }

        public static T* ReAllocT<T>(T* pointer, int count) where T : unmanaged
        {
            return (T*)Marshal.ReAllocHGlobal((nint)pointer, count * sizeof(T));
        }

        public static T* ReAllocT<T>(T* pointer, uint count) where T : unmanaged
        {
            return (T*)Marshal.ReAllocHGlobal((nint)pointer, (nint)(count * (uint)sizeof(T)));
        }

        public static T* ReAllocT<T>(T* pointer, nint count) where T : unmanaged
        {
            return (T*)Marshal.ReAllocHGlobal((nint)pointer, count * sizeof(T));
        }

        public static void* ReAlloc(void* pointer, int count)
        {
            return (void*)Marshal.ReAllocHGlobal((nint)pointer, count);
        }

        public static void* ReAlloc(void* pointer, uint count)
        {
            return (void*)Marshal.ReAllocHGlobal((nint)pointer, (nint)(count));
        }

        public static void* ReAlloc(void* pointer, nint count)
        {
            return (void*)Marshal.ReAllocHGlobal((nint)pointer, count);
        }

        /// <summary>
        /// Allocates an new pointer and copies the data from the source
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pointer">The pointer.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static T* AllocCopy<T>(T[] source) where T : unmanaged
        {
            int length = source.Length;
            T* result = AllocT<T>(length);
            fixed (T* pointer = source)
            {
                MemcpyT(pointer, result, length, length);
            }

            return result;
        }

        /// <summary>
        /// Allocates an new pointer and copies the data from the source
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pointer">The pointer.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static T* AllocCopy<T>(T* pointer, int length) where T : unmanaged
        {
            T* result = AllocT<T>(length);
            MemcpyT(pointer, result, length, length);
            return result;
        }

        /// <summary>
        /// Allocates an new pointer and copies the data from the source
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static void* AllocCopy(void* pointer, int length)
        {
            void* result = Alloc(length);
            Memcpy(pointer, result, length, length);
            return result;
        }

        /// <summary>
        /// Allocates an new pointer and copies the data from the source
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pointer">The pointer.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static T* AllocCopy<T>(T* pointer, uint length) where T : unmanaged
        {
            T* result = AllocT<T>(length);
            MemcpyT(pointer, result, length, length);
            return result;
        }

        /// <summary>
        /// Allocates an new pointer and copies the data from the source
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static void* AllocCopy(void* pointer, uint length)
        {
            void* result = Alloc(length);
            Memcpy(pointer, result, length, length);
            return result;
        }

        /// <summary>
        /// Allocates the specified count of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static T* AllocT<T>(uint count) where T : unmanaged
        {
            return (T*)Marshal.AllocHGlobal((int)(sizeof(T) * count));
        }

        /// <summary>
        /// Allocates the specified size.
        /// </summary>
        /// <param name="size">The size.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void* Alloc(int size)
        {
            return (void*)Marshal.AllocHGlobal(size);
        }

        /// <summary>
        /// Allocates the specified size.
        /// </summary>
        /// <param name="size">The size.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void* Alloc(uint size)
        {
            return (void*)Marshal.AllocHGlobal((int)size);
        }

        /// <summary>
        /// Allocs the specified stride multiplied by length.
        /// </summary>
        /// <param name="stride">The stride.</param>
        /// <param name="length">The length.</param>
        public static void** Alloc(int stride, int length)
        {
            return (void**)Marshal.AllocHGlobal(stride * length);
        }

        /// <summary>
        /// Allocs the specified stride multiplied by length.
        /// </summary>
        /// <param name="stride">The stride.</param>
        /// <param name="length">The length.</param>
        public static void** Alloc(uint size, uint length)
        {
            return (void**)Marshal.AllocHGlobal((int)(size * length));
        }

        /// <summary>
        /// Resizes the array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">The array.</param>
        /// <param name="oldLength">The old length.</param>
        /// <param name="newLength">The new length.</param>
        public static void ResizeArray<T>(T** array, int oldLength, int newLength) where T : unmanaged
        {
            var oldArray = *array;
            var newArray = AllocT<T>(newLength);

            Buffer.MemoryCopy(oldArray, newArray, newLength * sizeof(T), oldLength * sizeof(T));
            Free(oldArray);

            *array = newArray;
        }

        /// <summary>
        /// Resizes the array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">The array.</param>
        /// <param name="oldLength">The old length.</param>
        /// <param name="newLength">The new length.</param>
        public static void ResizeArray<T>(T** array, uint oldLength, uint newLength) where T : unmanaged
        {
            var oldArray = *array;
            var newArray = AllocT<T>(newLength);

            Buffer.MemoryCopy(oldArray, newArray, newLength * sizeof(T), oldLength * sizeof(T));
            Free(oldArray);

            *array = newArray;
        }

        /// <summary>
        /// Resizes the array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">The array.</param>
        /// <param name="oldLength">The old length.</param>
        /// <param name="newLength">The new length.</param>
        public static void ResizeArray<T>(ref T* array, int oldLength, int newLength) where T : unmanaged
        {
            var oldArray = array;
            var newArray = AllocT<T>(newLength);

            Buffer.MemoryCopy(oldArray, newArray, newLength * sizeof(T), oldLength * sizeof(T));
            Free(oldArray);

            array = newArray;
        }

        /// <summary>
        /// Resizes the array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">The array.</param>
        /// <param name="oldLength">The old length.</param>
        /// <param name="newLength">The new length.</param>
        public static void ResizeArray<T>(ref T* array, uint oldLength, uint newLength) where T : unmanaged
        {
            var oldArray = array;
            var newArray = AllocT<T>(newLength);

            Buffer.MemoryCopy(oldArray, newArray, newLength * sizeof(T), oldLength * sizeof(T));
            Free(oldArray);

            array = newArray;
        }

        /// <summary>
        /// Allocates an pointer array.
        /// </summary>
        /// <param name="length">The length.</param>
        public static void** AllocArray(uint length)
        {
            return (void**)Marshal.AllocHGlobal((int)(sizeof(nint) * length));
        }

        /// <summary>
        /// Allocates an pointer array and zeros it.
        /// </summary>
        /// <param name="length">The length.</param>
        public static void** AllocArrayAndZero(uint length)
        {
            var result = AllocArray(length);
            ZeroMemory(result, (int)(sizeof(nint) * length));
            return result;
        }

        /// <summary>
        /// Frees the specified pointer. And automatically calls <see cref="IFreeable.Release"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pointer">The pointer.</param>
        public static void Free<T>(T* pointer) where T : unmanaged, IFreeable
        {
            pointer->Release();
            Marshal.FreeHGlobal((nint)pointer);
        }

        /// <summary>
        /// Frees the specified pointer.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        public static void Free(void* pointer)
        {
            Marshal.FreeHGlobal((nint)pointer);
        }

        /// <summary>
        /// Frees the specified pointer.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        public static void Free(void** pointer)
        {
            Marshal.FreeHGlobal((nint)pointer);
        }

        /// <summary>
        /// Compares the data of two pointers.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <param name="length">The length.</param>
        /// <returns>true if the data matches the other, otherwise false.</returns>
        public static unsafe bool Compare(byte* a, byte* b, uint length)
        {
            Span<byte> aSpan = new(a, (int)length);
            Span<byte> bSpan = new(b, (int)length);
            return aSpan.SequenceEqual(bSpan);
        }

        /// <summary>
        /// Compares the data of two pointers.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <param name="length">The length.</param>
        /// <returns>true if the data matches the other, otherwise false.</returns>
        public static unsafe bool Compare(char* a, char* b, uint length)
        {
            Span<char> spanA = new(a, (int)length);
            Span<char> spanB = new(b, (int)length);
            return spanA.SequenceEqual(spanB);
        }

        /// <summary>
        /// Compares two strings.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">b.</param>
        /// <returns>true if the data matches the other, otherwise false.</returns>
        public static unsafe bool StringCompare(char* a, char* b)
        {
            int n1 = StringSizeNullTerminated(a);
            int n2 = StringSizeNullTerminated(b);
            ReadOnlySpan<char> chars1 = new(a, n1);
            ReadOnlySpan<char> chars2 = new(b, n2);
            if (n1 != n2)
            {
                return false;
            }

            return chars1.SequenceEqual(chars2);
        }

        /// <summary>
        /// Compares two strings.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">b.</param>
        /// <returns>true if the data matches the other, otherwise false.</returns>
        public static unsafe bool StringCompare(char* a, string b)
        {
            int n1 = StringSizeNullTerminated(a);
            int n2 = StringSizeNullTerminated(b);
            ReadOnlySpan<char> chars1 = new(a, n1);
            fixed (char* bp = b)
            {
                ReadOnlySpan<char> chars2 = new(bp, n2);
                if (n1 != n2)
                {
                    return false;
                }

                return chars1.SequenceEqual(chars2);
            }
        }

        /// <summary>
        /// Returns the size of the null terminated string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static int StringSizeNullTerminated(char* str)
        {
            int ret = 0;
            while (str[ret] != '\0')
            {
                ret++;
            }

            return (ret + 1) * 2;
        }

        /// <summary>
        /// Returns the size of the null terminated string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static int StringSizeNullTerminated(ReadOnlySpan<char> str)
        {
            int ret = 0;
            while (str[ret] != '\0')
            {
                ret++;
            }

            return (ret + 1) * 2;
        }

        /// <summary>
        /// Returns the size of the null terminated string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static int StringSizeNullTerminated(byte* str)
        {
            int ret = 0;
            while (str[ret] != (byte)'\0')
            {
                ret++;
            }

            return ret + 1;
        }

        public static T[]? ToManaged<T>(T* src, int length) where T : unmanaged
        {
            if (src == null)
            {
                return null;
            }

            T[] values = new T[length];
            fixed (T* dst = values)
            {
                MemcpyT(src, dst, length, length);
            }
            return values;
        }

        public static T[]? ToManaged<T>(T* src, uint length) where T : unmanaged
        {
            if (src == null)
            {
                return null;
            }

            T[] values = new T[length];
            fixed (T* dst = values)
            {
                MemcpyT(src, dst, length, length);
            }
            return values;
        }

        public static void Memset(void* ptr, byte value, int length)
        {
            new Span<byte>(ptr, length).Fill(value);
        }

        public static void Memset<T>(T* ptr, byte value, int count) where T : unmanaged
        {
            new Span<byte>(ptr, count * sizeof(T)).Fill(value);
        }
    }

    public static unsafe class ArrayUtils
    {
        public static void Add<T>(ref T[] array, T value)
        {
            Array.Resize(ref array, array.Length + 1);
            array[^1] = value;
        }

        public static void Remove<T>(ref T[] array, T value)
        {
            int index = Array.IndexOf(array, value);
            var count = array.Length - index;
            Buffer.BlockCopy(array, index + 1, array, index, count);
            Array.Resize(ref array, array.Length - 1);
        }

        public static bool AddUnique<T>(this IList<T> list, T t)
        {
            if (list.Contains(t))
            {
                return false;
            }

            list.Add(t);

            return true;
        }

        public static int IndexOf<T>(T* ptr, T* item, int count) where T : unmanaged
        {
            for (int i = 0; i < count; i++)
            {
                if (&ptr[i] == item)
                {
                    return i;
                }
            }
            return -1;
        }
    }

    public class ListPool<T>
    {
        private readonly ConcurrentStack<List<T>> pool = new();

        public static ListPool<T> Shared { get; } = new();

        public List<T> Rent()
        {
            if (pool.IsEmpty)
            {
                return new();
            }
            else
            {
                if (pool.TryPop(out var list))
                {
                    return list;
                }
                return new();
            }
        }

        public void Return(List<T> list)
        {
            list.Clear();
            pool.Push(list);
        }

        public void Clear()
        {
            pool.Clear();
        }
    }
}