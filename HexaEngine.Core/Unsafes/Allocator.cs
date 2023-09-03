namespace HexaEngine.Core.Unsafes
{
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public struct Pair<T1, T2> : IEquatable<Pair<T1, T2>>
    {
        public T1 First;
        public T2 Second;

        public Pair(T1 first, T2 second)
        {
            First = first;
            Second = second;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is Pair<T1, T2> pair && Equals(pair);
        }

        public readonly bool Equals(Pair<T1, T2> other)
        {
            return EqualityComparer<T1>.Default.Equals(First, other.First) &&
                   EqualityComparer<T2>.Default.Equals(Second, other.Second);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(First, Second);
        }

        public static bool operator ==(Pair<T1, T2> left, Pair<T1, T2> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Pair<T1, T2> left, Pair<T1, T2> right)
        {
            return !(left == right);
        }
    }

    /// <summary>
    /// Represents an allocator for memory management.
    /// </summary>
    public readonly struct Allocator : IAllocator
    {
        /// <summary>
        /// Gets the default allocator.
        /// </summary>
        public static readonly unsafe Allocator* Default = Utils.AllocT<Allocator>();

        /// <inheritdoc/>
        public unsafe void* Alloc(nint width)
        {
            byte* result = (byte*)Marshal.AllocHGlobal(width);
            for (nint i = 0; i < width; i++)
            {
                result[i] = 0;
            }

            return result;
        }

        /// <inheritdoc/>
        public unsafe void* ReAlloc(void* pv, nint width)
        {
            byte* result = (byte*)Marshal.ReAllocHGlobal((nint)pv, width);
            return result;
        }

        /// <inheritdoc/>
        public unsafe T* ReAlloc<T>(T* pv, uint count) where T : unmanaged
        {
            T* result = (T*)Marshal.ReAllocHGlobal((nint)pv, (nint)(count * sizeof(T)));
            return result;
        }

        /// <inheritdoc/>
        public unsafe void** Alloc(uint width, uint height)
        {
            uint size = width * height;
            byte* result = (byte*)Marshal.AllocHGlobal((nint)size);
            for (int i = 0; i < size; i++)
            {
                result[i] = 0;
            }

            return (void**)result;
        }

        /// <inheritdoc/>
        public unsafe T* Alloc<T>() where T : unmanaged
        {
            return (T*)Alloc(sizeof(T));
        }

        /// <inheritdoc/>
        public unsafe T* Allocate<T>(int count) where T : unmanaged
        {
            return (T*)Alloc((nint)(sizeof(T) * count));
        }

        /// <inheritdoc/>
        public unsafe T* Alloc<T>(uint count) where T : unmanaged
        {
            return (T*)Alloc((nint)(sizeof(T) * count));
        }

        /// <inheritdoc/>
        public unsafe T* Allocate<T>(ulong count) where T : unmanaged
        {
            return (T*)Alloc((nint)((ulong)sizeof(T) * count));
        }

        /// <inheritdoc/>
        public unsafe T** AllocateArray<T>(int count) where T : unmanaged
        {
            return (T**)Alloc((nint)(sizeof(nint) * count));
        }

        /// <inheritdoc/>
        public unsafe T** AllocateArray<T>(uint count) where T : unmanaged
        {
            return (T**)Alloc((nint)(sizeof(nint) * count));
        }

        /// <inheritdoc/>
        public unsafe void Free(void* ptr)
        {
            Marshal.FreeHGlobal((nint)ptr);
        }

        /// <inheritdoc/>
        public unsafe void Free<T>(T* ptr) where T : unmanaged, IFreeable
        {
            ptr->Release();
            Marshal.FreeHGlobal((nint)ptr);
        }
    }
}