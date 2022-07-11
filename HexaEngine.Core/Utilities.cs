namespace HexaEngine.Core
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    public static class Utilities
    {
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false), SuppressUnmanagedCodeSecurity]
        private static extern IntPtr memcpy(IntPtr dest, IntPtr src, ulong sizeInBytesToCopy);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyMemory(IntPtr dest, IntPtr src, ulong sizeInBytesToCopy)
        {
            memcpy(dest, src, sizeInBytesToCopy);
        }
    }
}