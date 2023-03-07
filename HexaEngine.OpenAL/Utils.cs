namespace HexaEngine.OpenAL
{
    using System.Runtime.InteropServices;

    internal static unsafe class Utils
    {
        public static T* Alloc<T>(int count) where T : unmanaged
        {
            return (T*)Marshal.AllocHGlobal(sizeof(T) * count);
        }

        public static void Free(void* pointer)
        {
            Marshal.FreeHGlobal((nint)pointer);
        }
    }
}