namespace HexaEngine.Core.Unsafes
{
    using System.Runtime.InteropServices;

    public unsafe interface IAllocator
    {
        void* Allocate(nint width);

        T* Allocate<T>() where T : unmanaged;

        void** Allocate(uint width, uint height);

        T* Allocate<T>(uint count) where T : unmanaged;

        T** AllocateArray<T>(uint count) where T : unmanaged;

        void Free(void* ptr);

        void Free<T>(T* ptr) where T : unmanaged, IFreeable;
    }

    public interface IFreeable
    {
        void Free();
    }

    public struct Allocator : IAllocator
    {
        public unsafe void* Allocate(nint width)
        {
            byte* result = (byte*)Marshal.AllocHGlobal(width);
            for (nint i = 0; i < width; i++)
                result[i] = 0;
            return result;
        }

        public unsafe void** Allocate(uint width, uint height)
        {
            uint size = width * height;
            byte* result = (byte*)Marshal.AllocHGlobal((nint)size);
            for (int i = 0; i < size; i++)
                result[i] = 0;
            return (void**)result;
        }

        public unsafe T* Allocate<T>() where T : unmanaged
        {
            return (T*)Allocate(sizeof(T));
        }

        public unsafe T* Allocate<T>(uint count) where T : unmanaged
        {
            return (T*)Allocate((nint)(sizeof(T) * count));
        }

        public unsafe T* Allocate<T>(ulong count) where T : unmanaged
        {
            return (T*)Allocate((nint)((ulong)sizeof(T) * count));
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
            ptr->Free();
            Marshal.FreeHGlobal((nint)ptr);
        }
    }
}