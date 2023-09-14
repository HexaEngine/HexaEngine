﻿namespace D3D12Testing.Unsafes
{
    using System.Runtime.InteropServices;

    public readonly struct Allocator : IAllocator
    {
        public static readonly unsafe Allocator* Default = AllocT<Allocator>();

        public unsafe void* Allocate(nint width)
        {
            byte* result = (byte*)Marshal.AllocHGlobal(width);
            for (nint i = 0; i < width; i++)
            {
                result[i] = 0;
            }

            return result;
        }

        public unsafe void** Allocate(uint width, uint height)
        {
            uint size = width * height;
            byte* result = (byte*)Marshal.AllocHGlobal((nint)size);
            for (int i = 0; i < size; i++)
            {
                result[i] = 0;
            }

            return (void**)result;
        }

        public unsafe T* Allocate<T>() where T : unmanaged
        {
            return (T*)Allocate(sizeof(T));
        }

        public unsafe T* Allocate<T>(int count) where T : unmanaged
        {
            return (T*)Allocate(sizeof(T) * count);
        }

        public unsafe T* Allocate<T>(uint count) where T : unmanaged
        {
            return (T*)Allocate((nint)(sizeof(T) * count));
        }

        public unsafe T* Allocate<T>(ulong count) where T : unmanaged
        {
            return (T*)Allocate((nint)((ulong)sizeof(T) * count));
        }

        public unsafe T** AllocateArray<T>(int count) where T : unmanaged
        {
            return (T**)Allocate(sizeof(nint) * count);
        }

        public unsafe T** AllocateArray<T>(uint count) where T : unmanaged
        {
            return (T**)Allocate((nint)(sizeof(nint) * count));
        }

        public unsafe void Free(void* ptr)
        {
            Marshal.FreeHGlobal((nint)ptr);
        }

        public unsafe void Free<T>(T* ptr) where T : unmanaged, IFreeable
        {
            ptr->Release();
            Marshal.FreeHGlobal((nint)ptr);
        }
    }
}