namespace HexaEngine.Core
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Unsafes;
    using System;
    using System.Collections.Concurrent;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// Utilities for allocation, freeing, reallocating, moving, copying native memory and conversation from managed to unmanaged.
    /// </summary>
    public static unsafe class Utils
    {
        /// <summary>
        /// Converts a UTF-8 encoded null-terminated byte pointer to a managed string.
        /// </summary>
        /// <param name="ptr">The pointer to the UTF-8 encoded string.</param>
        /// <returns>A managed string representing the UTF-8 data.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string? ToStringFromUTF8(byte* ptr)
        {
            return Marshal.PtrToStringUTF8((nint)ptr);
        }

        /// <summary>
        /// Counts the number of set bits (1s) in the binary representation of an unsigned integer.
        /// </summary>
        /// <param name="value">The unsigned integer to count bits in.</param>
        /// <returns>The count of set bits in the binary representation of the integer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Bitcount(this uint value)
        {
            uint v = value;
            v -= ((v >> 1) & 0x55555555); // reuse input as temporary
            v = (v & 0x33333333) + ((v >> 2) & 0x33333333); // temp
            uint c = ((v + (v >> 4) & 0xF0F0F0F) * 0x1010101) >> 24; // count
            return c;
        }

        /// <summary>
        /// Swaps the values of two pointers to unmanaged types.
        /// </summary>
        /// <typeparam name="T">The type of the pointers.</typeparam>
        /// <param name="a">The first pointer.</param>
        /// <param name="b">The second pointer.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap<T>(T* a, T* b) where T : unmanaged
        {
            (*b, *a) = (*a, *b);
        }

        /// <summary>
        /// Swaps the values of two variables.
        /// </summary>
        /// <typeparam name="T">The type of the variables.</typeparam>
        /// <param name="a">The first variable.</param>
        /// <param name="b">The second variable.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap<T>(ref T a, ref T b)
        {
            (b, a) = (a, b);
        }

        /// <summary>
        /// Copies the bytes of a value of an unmanaged type to a destination byte pointer.
        /// </summary>
        /// <typeparam name="T">The type of the value to copy.</typeparam>
        /// <param name="t">The value to copy.</param>
        /// <param name="offset">The offset in the destination byte array.</param>
        /// <param name="dst">The destination byte pointer.</param>
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

        /// <summary>
        /// Copies the bytes of an array of values of an unmanaged type to a destination byte pointer.
        /// </summary>
        /// <typeparam name="T">The type of the values in the array.</typeparam>
        /// <param name="t">The array of values to copy.</param>
        /// <param name="dst">The destination byte pointer.</param>
        /// <param name="stride">The stride between copied elements in bytes.</param>
        /// <param name="offset">The offset in the destination byte array.</param>
        /// <param name="count">The number of elements to copy.</param>
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
        /// Copies memory from the source to the destination with specified lengths.
        /// </summary>
        /// <param name="src">Pointer to the source memory.</param>
        /// <param name="dst">Pointer to the destination memory.</param>
        /// <param name="dstLength">Length of the destination memory to copy to.</param>
        /// <param name="srcLength">Length of the source memory to copy from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Memcpy(void* src, void* dst, uint dstLength, uint srcLength)
        {
            Buffer.MemoryCopy(src, dst, dstLength, srcLength);
        }

        /// <summary>
        /// Copies memory from the source to the destination with specified lengths.
        /// </summary>
        /// <param name="src">Pointer to the source memory.</param>
        /// <param name="dst">Pointer to the destination memory.</param>
        /// <param name="dstLength">Length of the destination memory to copy to.</param>
        /// <param name="srcLength">Length of the source memory to copy from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Memcpy(void* src, void* dst, int dstLength, int srcLength)
        {
            Buffer.MemoryCopy(src, dst, dstLength, srcLength);
        }

        /// <summary>
        /// Copies memory from the source to the destination with specified lengths.
        /// </summary>
        /// <param name="src">Pointer to the source memory.</param>
        /// <param name="dst">Pointer to the destination memory.</param>
        /// <param name="dstLength">Length of the destination memory to copy to.</param>
        /// <param name="srcLength">Length of the source memory to copy from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Memcpy(void* src, void* dst, long dstLength, long srcLength)
        {
            Buffer.MemoryCopy(src, dst, dstLength, srcLength);
        }

        /// <summary>
        /// Copies memory from the source to the destination with specified lengths.
        /// </summary>
        /// <param name="src">Pointer to the source memory.</param>
        /// <param name="dst">Pointer to the destination memory.</param>
        /// <param name="dstLength">Length of the destination memory to copy to.</param>
        /// <param name="srcLength">Length of the source memory to copy from.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Memcpy(void* src, void* dst, ulong dstLength, ulong srcLength)
        {
            Buffer.MemoryCopy(src, dst, dstLength, srcLength);
        }

        /// <summary>
        /// Copies memory from the source to the destination with the same length for both source and destination.
        /// </summary>
        /// <param name="src">Pointer to the source memory.</param>
        /// <param name="dst">Pointer to the destination memory.</param>
        /// <param name="length">Length of the source and destination memory to copy.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Memcpy(void* src, void* dst, uint length)
        {
            Buffer.MemoryCopy(src, dst, length, length);
        }

        /// <summary>
        /// Copies memory from the source to the destination with the same length for both source and destination.
        /// </summary>
        /// <param name="src">Pointer to the source memory.</param>
        /// <param name="dst">Pointer to the destination memory.</param>
        /// <param name="length">Length of the source and destination memory to copy.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Memcpy(void* src, void* dst, int length)
        {
            Buffer.MemoryCopy(src, dst, length, length);
        }

        /// <summary>
        /// Copies memory from the source to the destination with the same length for both source and destination.
        /// </summary>
        /// <param name="src">Pointer to the source memory.</param>
        /// <param name="dst">Pointer to the destination memory.</param>
        /// <param name="length">Length of the source and destination memory to copy.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Memcpy(void* src, void* dst, long length)
        {
            Buffer.MemoryCopy(src, dst, length, length);
        }

        /// <summary>
        /// Copies memory from the source to the destination with the same length for both source and destination.
        /// </summary>
        /// <param name="src">Pointer to the source memory.</param>
        /// <param name="dst">Pointer to the destination memory.</param>
        /// <param name="length">Length of the source and destination memory to copy.</param>
        public static void Memcpy(void* src, void* dst, ulong length)
        {
            Buffer.MemoryCopy(src, dst, length, length);
        }

        /// <summary>
        /// Copies memory from the source to the destination with the same length for both source and destination.
        /// </summary>
        /// <param name="src">Pointer to the source memory.</param>
        /// <param name="dst">Pointer to the destination memory.</param>
        /// <param name="dstLength">Length of the destination memory to copy to.</param>
        /// <param name="srcLength">Length of the source memory to copy from.</param>
        /// <typeparam name="T">Type of elements to copy.</typeparam>
        public static void MemcpyT<T>(T* src, T* dst, uint dstLength, uint srcLength) where T : unmanaged
        {
            Buffer.MemoryCopy(src, dst, dstLength * sizeof(T), srcLength * sizeof(T));
        }

        /// <summary>
        /// Copies memory from the source to the destination with the same length for both source and destination.
        /// </summary>
        /// <param name="src">Pointer to the source memory.</param>
        /// <param name="dst">Pointer to the destination memory.</param>
        /// <param name="dstLength">Length of the destination memory to copy to.</param>
        /// <param name="srcLength">Length of the source memory to copy from.</param>
        /// <typeparam name="T">Type of elements to copy.</typeparam>
        public static void MemcpyT<T>(T* src, T* dst, int dstLength, int srcLength) where T : unmanaged
        {
            Buffer.MemoryCopy(src, dst, dstLength * sizeof(T), srcLength * sizeof(T));
        }

        /// <summary>
        /// Copies memory from the source to the destination with the same length for both source and destination.
        /// </summary>
        /// <param name="src">Pointer to the source memory.</param>
        /// <param name="dst">Pointer to the destination memory.</param>
        /// <param name="length">Length of the source and destination memory to copy.</param>
        /// <typeparam name="T">Type of elements to copy.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MemcpyT<T>(T* src, T* dst, uint length) where T : unmanaged
        {
            Buffer.MemoryCopy(src, dst, length * sizeof(T), length * sizeof(T));
        }

        /// <summary>
        /// Copies memory from the source to the destination with the same length for both source and destination.
        /// </summary>
        /// <param name="src">Pointer to the source memory.</param>
        /// <param name="dst">Pointer to the destination memory.</param>
        /// <param name="length">Length of the source and destination memory to copy.</param>
        /// <typeparam name="T">Type of elements to copy.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MemcpyT<T>(T* src, T* dst, int length) where T : unmanaged
        {
            Buffer.MemoryCopy(src, dst, length * sizeof(T), length * sizeof(T));
        }

        /// <summary>
        /// Sets all bytes in memory to zero for a span of type T.
        /// </summary>
        /// <typeparam name="T">The type of elements in the span.</typeparam>
        /// <param name="pointer">Pointer to the memory to clear.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ZeroMemoryT<T>(T* pointer) where T : unmanaged
        {
            new Span<byte>(pointer, sizeof(T)).Clear();
        }

        /// <summary>
        /// Sets all bytes in memory to zero for a span of type T.
        /// </summary>
        /// <typeparam name="T">The type of elements in the span.</typeparam>
        /// <param name="pointer">Pointer to the memory to clear.</param>
        /// <param name="length">Number of elements of type T to clear.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ZeroMemoryT<T>(T* pointer, int length) where T : unmanaged
        {
            new Span<byte>(pointer, sizeof(T) * length).Clear();
        }

        /// <summary>
        /// Sets all bytes in memory to zero for a span of type T.
        /// </summary>
        /// <typeparam name="T">The type of elements in the span.</typeparam>
        /// <param name="pointer">Pointer to the memory to clear.</param>
        /// <param name="length">Number of elements of type T to clear.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ZeroMemoryT<T>(T* pointer, uint length) where T : unmanaged
        {
            ZeroMemory(pointer, sizeof(T) * (int)length);
        }

        /// <summary>
        /// Sets all bytes in memory to zero for a specified number of bytes.
        /// </summary>
        /// <param name="pointer">Pointer to the memory to clear.</param>
        /// <param name="size">Number of bytes to clear.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ZeroMemory(void* pointer, uint size)
        {
            byte* pointerCopy = (byte*)pointer;
            for (uint i = 0; i < size; i++)
            {
                pointerCopy[i] = 0;
            }
        }

        /// <summary>
        /// Sets all bytes in memory to zero for a specified number of bytes.
        /// </summary>
        /// <param name="pointer">Pointer to the memory to clear.</param>
        /// <param name="size">Number of bytes to clear.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ZeroMemory(void* pointer, int size)
        {
            new Span<byte>(pointer, size).Clear();
        }

        /// <summary>
        /// Sets all bytes in memory to zero for a specified number of bytes.
        /// </summary>
        /// <param name="pointer">Pointer to the memory to clear.</param>
        /// <param name="size">Number of bytes to clear.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ZeroMemory(void* pointer, ulong size)
        {
            byte* pointerCopy = (byte*)pointer;
            for (ulong i = 0; i < size; i++)
            {
                pointerCopy[i] = 0;
            }
        }

        /// <summary>
        /// Sets all bytes in memory to zero for a specified number of bytes.
        /// </summary>
        /// <param name="pointer">Pointer to the memory to clear.</param>
        /// <param name="size">Number of bytes to clear.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ZeroMemory(void* pointer, long size)
        {
            byte* pointerCopy = (byte*)pointer;
            for (long i = 0; i < size; i++)
            {
                pointerCopy[i] = 0;
            }
        }

        /// <summary>
        /// Sets all bytes in memory to zero for a specified number of bytes.
        /// </summary>
        /// <param name="pointer">Pointer to the memory to clear.</param>
        /// <param name="size">Number of bytes to clear.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ZeroMemory(void* pointer, nint size)
        {
            byte* pointerCopy = (byte*)pointer;
            for (nint i = 0; i < size; i++)
            {
                pointerCopy[i] = 0;
            }
        }

        /// <summary>
        /// Allocates and returns a pointer to memory for a single element of type T, initializing it with the provided data.
        /// </summary>
        /// <typeparam name="T">The type of the element to allocate.</typeparam>
        /// <param name="data">The data to initialize the allocated memory with.</param>
        /// <returns>A pointer to the allocated memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* AllocT<T>(T data) where T : unmanaged
        {
            T* result = (T*)Marshal.AllocHGlobal(sizeof(T));
            *result = data;
            return result;
        }

        /// <summary>
        /// Allocates and returns a pointer to memory for a single element of type T, initializing it with the default value.
        /// </summary>
        /// <typeparam name="T">The type of the element to allocate.</typeparam>
        /// <returns>A pointer to the allocated memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* AllocT<T>() where T : unmanaged
        {
            T* result = (T*)Marshal.AllocHGlobal(sizeof(T));
            *result = new T();
            return result;
        }

        /// <summary>
        /// Allocates and returns a pointer to memory for an array of elements of type T.
        /// </summary>
        /// <typeparam name="T">The type of the elements to allocate.</typeparam>
        /// <param name="count">The number of elements to allocate.</param>
        /// <returns>A pointer to the allocated memory for the array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* AllocT<T>(int count) where T : unmanaged
        {
            return (T*)Marshal.AllocHGlobal(sizeof(T) * count);
        }

        /// <summary>
        /// Allocates and returns a pointer to memory for an array of elements of type T.
        /// </summary>
        /// <typeparam name="T">The type of the elements to allocate.</typeparam>
        /// <param name="count">The number of elements to allocate.</param>
        /// <returns>A pointer to the allocated memory for the array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* AllocT<T>(uint count) where T : unmanaged
        {
            return (T*)Marshal.AllocHGlobal((int)(sizeof(T) * count));
        }

        /// <summary>
        /// Allocates and returns a pointer to unmanaged memory.
        /// </summary>
        /// <param name="count">The number of bytes to allocate.</param>
        /// <returns>A pointer to the allocated memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* Alloc(nint count)
        {
            return (void*)Marshal.AllocHGlobal(count);
        }

        /// <summary>
        /// Allocates and returns a pointer to unmanaged memory.
        /// </summary>
        /// <param name="count">The number of bytes to allocate.</param>
        /// <returns>A pointer to the allocated memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* Alloc(nuint count)
        {
            return (void*)Marshal.AllocHGlobal((nint)count);
        }

        /// <summary>
        /// Allocates and returns a pointer to unmanaged memory.
        /// </summary>
        /// <param name="size">The size, in bytes, to allocate.</param>
        /// <returns>A pointer to the allocated memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* Alloc(int size)
        {
            return (void*)Marshal.AllocHGlobal(size);
        }

        /// <summary>
        /// Allocates and returns a pointer to unmanaged memory.
        /// </summary>
        /// <param name="size">The size, in bytes, to allocate.</param>
        /// <returns>A pointer to the allocated memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* Alloc(uint size)
        {
            return (void*)Marshal.AllocHGlobal((int)size);
        }

        /// <summary>
        /// Reallocates and returns a pointer to unmanaged memory for an array of elements of type T, preserving the existing data.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="pointer">A pointer to the existing memory.</param>
        /// <param name="count">The new number of elements to allocate.</param>
        /// <returns>A pointer to the reallocated memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* ReAllocT<T>(T* pointer, int count) where T : unmanaged
        {
            return (T*)Marshal.ReAllocHGlobal((nint)pointer, count * sizeof(T));
        }

        /// <summary>
        /// Reallocates and returns a pointer to unmanaged memory for an array of elements of type T, preserving the existing data.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="pointer">A pointer to the existing memory.</param>
        /// <param name="count">The new number of elements to allocate.</param>
        /// <returns>A pointer to the reallocated memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* ReAllocT<T>(T* pointer, uint count) where T : unmanaged
        {
            return (T*)Marshal.ReAllocHGlobal((nint)pointer, (nint)(count * (uint)sizeof(T)));
        }

        /// <summary>
        /// Reallocates and returns a pointer to unmanaged memory for an array of elements of type T, preserving the existing data.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="pointer">A pointer to the existing memory.</param>
        /// <param name="count">The new number of elements to allocate.</param>
        /// <returns>A pointer to the reallocated memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* ReAllocT<T>(T* pointer, nint count) where T : unmanaged
        {
            return (T*)Marshal.ReAllocHGlobal((nint)pointer, count * sizeof(T));
        }

        /// <summary>
        /// Reallocates and returns a pointer to unmanaged memory, preserving the existing data.
        /// </summary>
        /// <param name="pointer">A pointer to the existing memory.</param>
        /// <param name="count">The new number of bytes to allocate.</param>
        /// <returns>A pointer to the reallocated memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* ReAlloc(void* pointer, int count)
        {
            return (void*)Marshal.ReAllocHGlobal((nint)pointer, count);
        }

        /// <summary>
        /// Reallocates and returns a pointer to unmanaged memory, preserving the existing data.
        /// </summary>
        /// <param name="pointer">A pointer to the existing memory.</param>
        /// <param name="count">The new number of bytes to allocate.</param>
        /// <returns>A pointer to the reallocated memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* ReAlloc(void* pointer, uint count)
        {
            return (void*)Marshal.ReAllocHGlobal((nint)pointer, (nint)count);
        }

        /// <summary>
        /// Reallocates and returns a pointer to unmanaged memory, preserving the existing data.
        /// </summary>
        /// <param name="pointer">A pointer to the existing memory.</param>
        /// <param name="count">The new number of bytes to allocate.</param>
        /// <returns>A pointer to the reallocated memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* ReAlloc(void* pointer, nint count)
        {
            return (void*)Marshal.ReAllocHGlobal((nint)pointer, count);
        }

        /// <summary>
        /// Allocates memory for an array of elements of type T, copies data from the source pointer to the new memory, and returns a pointer to the new memory.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="pointer">A pointer to the source data to be copied.</param>
        /// <param name="length">The number of elements in the array.</param>
        /// <returns>A pointer to the newly allocated memory with the copied data.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* AllocCopyT<T>(T* pointer, int length) where T : unmanaged
        {
            T* result = AllocT<T>(length);
            MemcpyT(pointer, result, length, length);
            return result;
        }

        /// <summary>
        /// Allocates memory for an array of elements of type T, copies data from the source pointer to the new memory, and returns a pointer to the new memory.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="pointer">A pointer to the source data to be copied.</param>
        /// <param name="length">The number of elements in the array.</param>
        /// <returns>A pointer to the newly allocated memory with the copied data.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* AllocCopyT<T>(T* pointer, uint length) where T : unmanaged
        {
            T* result = AllocT<T>(length);
            MemcpyT(pointer, result, length, length);
            return result;
        }

        /// <summary>
        /// Allocates memory for an array of elements of type T, copies data from the source span to the new memory, and returns a pointer to the new memory.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="source">The source span to copy from.</param>
        /// <returns>A pointer to the newly allocated memory with the copied data.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* AllocCopyT<T>(T[] source) where T : unmanaged
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
        /// Allocates memory for an array of elements of type T, copies data from the source span to the new memory, and returns a pointer to the new memory.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="source">The source span to copy from.</param>
        /// <returns>A pointer to the newly allocated memory with the copied data.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* AllocCopyT<T>(Span<T> source) where T : unmanaged
        {
            int length = source.Length;
            T* result = AllocT<T>(length);
            for (int i = 0; i < length; i++)
            {
                result[i] = source[i];
            }

            return result;
        }

        /// <summary>
        /// Allocates memory for an array of elements of type T, copies data from the source span to the new memory, and returns a pointer to the new memory.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="source">The source span to copy from.</param>
        /// <returns>A pointer to the newly allocated memory with the copied data.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* AllocCopyT<T>(ReadOnlySpan<T> source) where T : unmanaged
        {
            int length = source.Length;
            T* result = AllocT<T>(length);
            for (int i = 0; i < length; i++)
            {
                result[i] = source[i];
            }

            return result;
        }

        /// <summary>
        /// Allocates memory for an array of bytes, copies data from the source pointer to the new memory, and returns a pointer to the new memory.
        /// </summary>
        /// <param name="pointer">A pointer to the source data to be copied.</param>
        /// <param name="length">The number of bytes to allocate and copy.</param>
        /// <returns>A pointer to the newly allocated memory with the copied data.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AllocCopy(void* pointer, int length)
        {
            void* result = Alloc(length);
            Memcpy(pointer, result, length, length);
            return result;
        }

        /// <summary>
        /// Allocates memory for an array of bytes, copies data from the source pointer to the new memory, and returns a pointer to the new memory.
        /// </summary>
        /// <param name="pointer">A pointer to the source data to be copied.</param>
        /// <param name="length">The number of bytes to allocate and copy.</param>
        /// <returns>A pointer to the newly allocated memory with the copied data.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AllocCopy(void* pointer, uint length)
        {
            void* result = Alloc(length);
            Memcpy(pointer, result, length, length);
            return result;
        }

        /// <summary>
        /// Allocates memory for an array of bytes, copies data from the source pointer to the new memory, and returns a pointer to the new memory.
        /// </summary>
        /// <param name="pointer">A pointer to the source data to be copied.</param>
        /// <param name="length">The number of bytes to allocate and copy.</param>
        /// <returns>A pointer to the newly allocated memory with the copied data.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AllocCopy(void* pointer, nint length)
        {
            void* result = Alloc(length);
            Memcpy(pointer, result, length, length);
            return result;
        }

        /// <summary>
        /// Allocates memory for an array of bytes, copies data from the source pointer to the new memory, and returns a pointer to the new memory.
        /// </summary>
        /// <param name="pointer">A pointer to the source data to be copied.</param>
        /// <param name="length">The number of bytes to allocate and copy.</param>
        /// <returns>A pointer to the newly allocated memory with the copied data.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AllocCopy(void* pointer, nuint length)
        {
            void* result = Alloc(length);
            Memcpy(pointer, result, length, length);
            return result;
        }

        /// <summary>
        /// Allocates memory for an array of pointers and returns a pointer to the newly allocated memory.
        /// </summary>
        /// <param name="length">The number of pointers to allocate.</param>
        /// <returns>A pointer to the newly allocated memory for an array of pointers.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void** AllocArray(uint length)
        {
            return (void**)Marshal.AllocHGlobal((int)(sizeof(nint) * length));
        }

        /// <summary>
        /// Allocates memory for an array of pointers, sets the memory to zero, and returns a pointer to the newly allocated memory.
        /// </summary>
        /// <param name="length">The number of pointers to allocate and set to zero.</param>
        /// <returns>A pointer to the newly allocated memory for an array of pointers with zeroed memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void** AllocArrayAndZero(uint length)
        {
            var result = AllocArray(length);
            ZeroMemory(result, (int)(sizeof(nint) * length));
            return result;
        }

        /// <summary>
        /// Frees memory allocated for an unmanaged resource associated with a pointer to a type that implements <see cref="IFreeable"/>.
        /// </summary>
        /// <typeparam name="T">The unmanaged type that implements <see cref="IFreeable"/>.</typeparam>
        /// <param name="pointer">A pointer to the unmanaged resource to be released.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free<T>(T* pointer) where T : unmanaged, IFreeable
        {
            pointer->Release();
            Marshal.FreeHGlobal((nint)pointer);
        }

        /// <summary>
        /// Frees memory allocated for an unmanaged resource associated with a pointer.
        /// </summary>
        /// <param name="pointer">A pointer to the unmanaged resource to be released.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free(void* pointer)
        {
            Marshal.FreeHGlobal((nint)pointer);
        }

        /// <summary>
        /// Frees memory allocated for an array of pointers.
        /// </summary>
        /// <param name="pointer">A pointer to the array of pointers to be released.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int StringSizeNullTerminated(byte* str)
        {
            int ret = 0;
            while (str[ret] != (byte)'\0')
            {
                ret++;
            }

            return ret + 1;
        }

        /// <summary>
        /// Copies unmanaged memory to a new managed array.
        /// </summary>
        /// <typeparam name="T">The unmanaged type of elements.</typeparam>
        /// <param name="src">A pointer to the source unmanaged memory.</param>
        /// <param name="length">The number of elements to copy.</param>
        /// <returns>A new managed array containing the copied elements or null if <paramref name="src"/> is null.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        /// <summary>
        /// Copies unmanaged memory to a new managed array.
        /// </summary>
        /// <typeparam name="T">The unmanaged type of elements.</typeparam>
        /// <param name="src">A pointer to the source unmanaged memory.</param>
        /// <param name="length">The number of elements to copy.</param>
        /// <returns>A new managed array containing the copied elements or null if <paramref name="src"/> is null.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        /// <summary>
        /// Sets the memory at the specified pointer to the specified value for a given number of elements.
        /// </summary>
        /// <typeparam name="T">The unmanaged type of elements.</typeparam>
        /// <param name="ptr">A pointer to the memory to set.</param>
        /// <param name="value">The byte value to set the memory to.</param>
        /// <param name="count">The number of elements to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Memset<T>(T* ptr, byte value, int count) where T : unmanaged
        {
            new Span<byte>(ptr, count * sizeof(T)).Fill(value);
        }

        /// <summary>
        /// Sets the memory at the specified pointer to the specified value for a given number of elements.
        /// </summary>
        /// <typeparam name="T">The unmanaged type of elements.</typeparam>
        /// <param name="ptr">A pointer to the memory to set.</param>
        /// <param name="value">The byte value to set the memory to.</param>
        /// <param name="count">The number of elements to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Memset<T>(T* ptr, byte value, uint count) where T : unmanaged
        {
            new Span<byte>(ptr, (int)(count * sizeof(T))).Fill(value);
        }

        /// <summary>
        /// Sets the memory at the specified pointer to the specified value for a given number of bytes.
        /// </summary>
        /// <param name="ptr">A pointer to the memory to set.</param>
        /// <param name="value">The byte value to set the memory to.</param>
        /// <param name="length">The number of bytes to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Memset(void* ptr, byte value, int length)
        {
            new Span<byte>(ptr, length).Fill(value);
        }

        /// <summary>
        /// Sets the memory at the specified pointer to the specified value for a given number of bytes.
        /// </summary>
        /// <param name="ptr">A pointer to the memory to set.</param>
        /// <param name="value">The byte value to set the memory to.</param>
        /// <param name="length">The number of bytes to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Memset(void* ptr, byte value, uint length)
        {
            new Span<byte>(ptr, (int)length).Fill(value);
        }
    }

    /// <summary>
    /// Utilities for managed and unmanaged arrays.
    /// </summary>
    public static unsafe class ArrayUtils
    {
        /// <summary>
        /// Adds a value to an array and resizes it to accommodate the new value.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">Reference to the array to which the value will be added.</param>
        /// <param name="value">The value to add to the array.</param>
        public static void Add<T>(ref T[] array, T value)
        {
            Array.Resize(ref array, array.Length + 1);
            array[^1] = value;
        }

        /// <summary>
        /// Removes the first occurrence of a value from an array and resizes it accordingly.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">Reference to the array from which the value will be removed.</param>
        /// <param name="value">The value to remove from the array.</param>
        public static void Remove<T>(ref T[] array, T value)
        {
            int index = Array.IndexOf(array, value);
            var count = array.Length - index;
            Buffer.BlockCopy(array, index + 1, array, index, count);
            Array.Resize(ref array, array.Length - 1);
        }

        /// <summary>
        /// Adds a value to a list if it doesn't already exist and returns a boolean indicating success.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to which the value will be added.</param>
        /// <param name="t">The value to add to the list.</param>
        /// <returns>Returns true if the value was added (not already in the list), otherwise false.</returns>
        public static bool AddUnique<T>(this IList<T> list, T t)
        {
            if (list.Contains(t))
            {
                return false;
            }

            list.Add(t);

            return true;
        }

        /// <summary>
        /// Finds the index of a specified item within an unmanaged array.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="ptr">Pointer to the beginning of the array.</param>
        /// <param name="item">Pointer to the item to search for.</param>
        /// <param name="count">The number of items in the array.</param>
        /// <returns>The index of the item if found; otherwise, -1.</returns>
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

    /// <summary>
    /// A thread-safe object pool for <see cref="List{T}"/> instances.
    /// </summary>
    /// <typeparam name="T">The type of elements in the lists.</typeparam>
    public class ListPool<T>
    {
        private readonly ConcurrentStack<List<T>> pool = new();

        /// <summary>
        /// Gets a shared instance of the <see cref="ListPool{T}"/> for convenient use.
        /// </summary>
        public static ListPool<T> Shared { get; } = new();

        /// <summary>
        /// Rents a <see cref="List{T}"/> instance from the pool. If the pool is empty, a new instance is created.
        /// </summary>
        /// <returns>A <see cref="List{T}"/> instance from the pool or a new instance if the pool is empty.</returns>
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

        /// <summary>
        /// Returns a rented <see cref="List{T}"/> instance to the pool after clearing its contents.
        /// </summary>
        /// <param name="list">The <see cref="List{T}"/> instance to return to the pool.</param>
        public void Return(List<T> list)
        {
            list.Clear();
            pool.Push(list);
        }

        /// <summary>
        /// Clears the pool, removing all <see cref="List{T}"/> instances from it.
        /// </summary>
        public void Clear()
        {
            pool.Clear();
        }
    }
}